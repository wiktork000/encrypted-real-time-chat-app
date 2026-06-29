using ChatApp.Data;
using ChatApp.DTOs;
using ChatApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace ChatApp.Services
{
    public struct UserKey
    {
        public bool Success { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
    }
    public interface IKeyService
    {
        public Task<UserKey> CreateUserKeyAsync(int userId, string password);
        public Task<UserKey> UpdateUserKeyAsync(int userId, string oldPassword, string newPassword);

        public Task<int> CreateConversationKeyAsync(int conversationId, int authorId);
        public Task<List<KeyDto>?> GetUserConversationKeys(int conversationId, int userId);
        public Task<KeyDto?> GetCurrentKeyForConversation(int conversationId, int userId);
    }

    public class KeyService : IKeyService
    {
        private readonly SabrinaChatDbContext _context;
        private readonly IConfiguration _configuration;
        public KeyService(SabrinaChatDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        public async Task<UserKey> CreateUserKeyAsync(int userId, string password)
        {
            try
            {
                PbeParameters pbeParameters = new PbeParameters(PbeEncryptionAlgorithm.Aes256Cbc, HashAlgorithmName.SHA256, 10000);
                RSA rsa = RSA.Create(2048);
                string publicKey = rsa.ExportRSAPublicKeyPem();
                string privateKey = rsa.ExportEncryptedPkcs8PrivateKeyPem(password, pbeParameters);


                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return new UserKey
                    {
                        Success = false,
                        PublicKey = string.Empty,
                        PrivateKey = string.Empty
                    };
                }

                user.PublicKey = publicKey;
                user.PrivateKey = privateKey;

                await _context.SaveChangesAsync();

                return new UserKey
                {
                    Success = true,
                    PublicKey = publicKey,
                    PrivateKey = privateKey
                };
            }
            catch
            {
                return new UserKey
                {
                    Success = false,
                    PublicKey = string.Empty,
                    PrivateKey = string.Empty
                };
            }
        }

        public async Task<UserKey> UpdateUserKeyAsync(int userId, string oldPassword, string newPassword)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return new UserKey
                    {
                        Success = false,
                        PublicKey = string.Empty,
                        PrivateKey = string.Empty
                    };
                }

                RSA rsa = RSA.Create(2048);
                rsa.ImportFromEncryptedPem(user.PrivateKey, oldPassword);
                PbeParameters pbeParameters = new PbeParameters(PbeEncryptionAlgorithm.Aes256Cbc, HashAlgorithmName.SHA256, 10000);
                user.PrivateKey = rsa.ExportEncryptedPkcs8PrivateKeyPem(newPassword, pbeParameters);

                await _context.SaveChangesAsync();

                return new UserKey
                {
                    Success = true,
                    PublicKey = user.PublicKey,
                    PrivateKey = user.PrivateKey
                };
            }
            catch
            {
                return new UserKey
                {
                    Success = false,
                    PublicKey = string.Empty,
                    PrivateKey = string.Empty
                };
            }
        }

        public async Task<int> CreateConversationKeyAsync(int conversationId, int authorId)
        {
            try
            {
                var userIds = await _context.ConversationParticipants
                    .Where(cp => cp.ConversationId == conversationId)
                    .Select(cp => cp.UserId)
                    .ToListAsync();

                var users = await _context.Users.Where(u => userIds.Contains(u.Id))
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        Name = u.Name,
                        PublicKey = u.PublicKey,
                        CreatedAt = u.CreatedAt
                    }).ToListAsync();

                if (users.Count == 0)
                    return -1; // No users in the conversation

                Aes aes = Aes.Create();
                var key = aes.Key;

                byte[] msgEnc = aes.EncryptCbc(Encoding.UTF8.GetBytes("Conversation key updated"), aes.IV);
                byte[] firstMessage = new byte[aes.IV.Length + msgEnc.Length];
                Buffer.BlockCopy(aes.IV, 0, firstMessage, 0, aes.IV.Length);
                Buffer.BlockCopy(msgEnc, 0, firstMessage, aes.IV.Length, msgEnc.Length);

                var message = new Message
                {
                    ConversationId = conversationId,
                    Content = Convert.ToBase64String(firstMessage),
                    Timestamp = DateTime.UtcNow,
                    AuthorId = authorId,
                };

                _context.Messages.Add(message);
                await _context.SaveChangesAsync();

                await InvalidateKeys(conversationId, message.Id);

                var EncryptedKeys = new Dictionary<int, string>();
                var keys = new List<Key>();
                foreach (var user in users)
                {
                    if (string.IsNullOrEmpty(user.PublicKey))
                        continue;
                    RSA rsa = RSA.Create();
                    rsa.ImportFromPem(user.PublicKey);
                    var encryptedKey = rsa.Encrypt(key, RSAEncryptionPadding.OaepSHA256);
                    _context.Keys.Add(new Key
                    {
                        ConversationId = conversationId,
                        UserId = user.Id,
                        KeyValue = Convert.ToBase64String(encryptedKey),
                        Timestamp = DateTime.UtcNow,
                        FromMessageId = message.Id,
                    });
                }
                await _context.SaveChangesAsync();

                return 0;

            }
            catch
            {
                return -1;
            }
        }

        private async Task<int> InvalidateKeys(int conversationId, int lastMessageId)
        {
            var keys = await _context.Keys
                .Where(k => k.ConversationId == conversationId && k.ToMessageId == null)
                .ToListAsync();
            foreach (var key in keys)
            {
                key.ToMessageId = lastMessageId;
            }

            return await _context.SaveChangesAsync();

        }

        public async Task<List<KeyDto>?> GetUserConversationKeys(int conversationId, int userId)
        {
            var keys = await _context.Keys
                .Where(k => k.UserId == userId && k.ConversationId == conversationId)
                .Select(k => new KeyDto
                {
                    Id = k.Id,
                    ConversationId = k.ConversationId,
                    UserId = k.UserId,
                    FirstMessageId = k.FromMessageId,
                    LastMessageId = k.ToMessageId,
                    KeyValue = k.KeyValue,
                    Timestamp = k.Timestamp,
                    IsActive = k.IsValid,
                })
                .ToListAsync();
            if (keys.Count == 0)
            {
                return null;
            }
            return keys;
        }

        public async Task<KeyDto?> GetCurrentKeyForConversation(int conversationId, int userId)
        {
            var key = await _context.Keys
                .Where(k => k.ConversationId == conversationId && k.UserId == userId && k.ToMessageId == null)
                .Select(k => new KeyDto
                {
                    Id = k.Id,
                    ConversationId = k.ConversationId,
                    UserId = k.UserId,
                    FirstMessageId = k.FromMessageId,
                    LastMessageId = k.ToMessageId,
                    KeyValue = k.KeyValue,
                    Timestamp = k.Timestamp,
                    IsActive = k.IsValid,
                })
                .FirstOrDefaultAsync();
            return key;
        }
    }
}
