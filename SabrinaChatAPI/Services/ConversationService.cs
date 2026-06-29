using ChatApp.Data;
using ChatApp.DTOs;
using ChatApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Services
{
    public interface IConversationService
    {
        public Task<ConversationDto> CreateConversationAsync(string Name, List<int> Participants, int ownerId);
        public Task<ConversationDto?> GetConversationByIdAsync(int conversationId, int userId);
        public Task<IEnumerable<ConversationDto>> GetConversationsForUserAsync(int userId);
        public Task RemoveConversationAsync(int conversationId, int userId);
        public Task<bool> IsUserInConversationAsync(int conversationId, int userId);
    }

    public class ConversationService : IConversationService
    {
        private readonly IKeyService _keyService;
        private readonly SabrinaChatDbContext _context;
        public ConversationService(IKeyService keyService, SabrinaChatDbContext context)
        {
            _keyService = keyService;
            _context = context;
        }

        public async Task<ConversationDto> CreateConversationAsync(string Name, List<int> Participants, int ownerId)
        {
            var conversation = new Conversation
            {
                Name = Name
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            HashSet<int> uniqueParticipants = new HashSet<int>(Participants);
            uniqueParticipants.Add(ownerId);
            foreach (var participantId in uniqueParticipants)
            {
                var participant = new ConversationParticipant
                {
                    ConversationId = conversation.Id,
                    UserId = participantId
                };
                _context.ConversationParticipants.Add(participant);
            }

            await _context.SaveChangesAsync();

            var result = await _keyService.CreateConversationKeyAsync(conversation.Id, ownerId);

            ConversationDto conversationDto = new ConversationDto
            {
                Id = conversation.Id,
                Name = conversation.Name,
                CreatedAt = conversation.CreatedAt,
                Participants = uniqueParticipants.Select(id => new UserDto
                {
                    Id = id,
                    Name = _context.Users.FirstOrDefault(u => u.Id == id)?.Name ?? "Unknown",
                    PublicKey = _context.Users.FirstOrDefault(u => u.Id == id)?.PublicKey ?? string.Empty,
                    CreatedAt = _context.Users.FirstOrDefault(u => u.Id == id)?.CreatedAt ?? DateTime.UtcNow
                }).ToList(),
                NumberOfUnreadMessages = _context.ConversationParticipants
                    .Where(cp => cp.ConversationId == conversation.Id && cp.UserId == ownerId)
                    .Select(cp => cp.NumberOfUnreadMessages).FirstOrDefault(),
                LastMessageDate = conversation.CreatedAt
            };

            if (result < 0)
            {
                throw new Exception("Failed to create conversation key.");
            }

            return conversationDto;
        }

        public async Task<ConversationDto?> GetConversationByIdAsync(int conversationId, int userId)
        {
            var conversation = await _context.Conversations
                .Include(c => c.Participants)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(c => c.Id == conversationId);
            if (conversation == null)
            {
                return null;
            }

            var lastMessage = _context.Messages
                .Where(m => m.ConversationId == conversationId)
                .OrderByDescending(m => m.Timestamp)
                .FirstOrDefault();

            var conversationDto = new ConversationDto
            {
                Id = conversation.Id,
                Name = conversation.Name,
                Participants = conversation.Participants.Select(p => new UserDto
                {
                    Id = p.User.Id,
                    Name = p.User.Name,
                    PublicKey = p.User.PublicKey,
                    CreatedAt = p.User.CreatedAt
                }).ToList(),
                NumberOfUnreadMessages = _context.ConversationParticipants
                    .Where(cp => cp.ConversationId == conversation.Id && cp.UserId == userId)
                    .Select(cp => cp.NumberOfUnreadMessages).FirstOrDefault(),
                LastMessageDate = lastMessage.Timestamp,
            };
            return conversationDto;
        }

        public async Task<IEnumerable<ConversationDto>> GetConversationsForUserAsync(int userId)
        {
            var conversations = await _context.Conversations
                .Include(c => c.Participants)
                    .ThenInclude(p => p.User)
                .Where(c => c.Participants.Any(p => p.UserId == userId))
                .ToListAsync();

            return conversations.Select(c => new ConversationDto
            {
                Id = c.Id,
                Name = c.Name,
                CreatedAt = c.CreatedAt,
                Participants = c.Participants.Select(p => new UserDto
                {
                    Id = p.User.Id,
                    Name = p.User.Name,
                    PublicKey = p.User.PublicKey,
                    CreatedAt = p.User.CreatedAt
                }).ToList(),
                NumberOfUnreadMessages = _context.ConversationParticipants
                    .Where(cp => cp.ConversationId == c.Id && cp.UserId == userId)
                    .Select(cp => cp.NumberOfUnreadMessages).FirstOrDefault(),
                LastMessageDate = _context.Messages
                    .Where(m => m.ConversationId == c.Id)
                    .OrderByDescending(m => m.Timestamp)
                    .Select(m => m.Timestamp)
                    .FirstOrDefault()
            });
        }

        public async Task RemoveConversationAsync(int conversationId, int userId)
        {
            var conversation = await _context.Conversations
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.Id == conversationId);
            if (conversation == null)
            {
                throw new Exception("Conversation not found.");
            }
            var participant = conversation.Participants.FirstOrDefault(p => p.UserId == userId);
            if (participant == null)
            {
                throw new Exception("User is not a participant in this conversation.");
            }

            if (conversation.Participants.Count == 1)
            {
                _context.Conversations.Remove(conversation);
            }
            _context.ConversationParticipants.Remove(participant);

            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsUserInConversationAsync(int conversationId, int userId)
        {
            return await _context.ConversationParticipants
                .AnyAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);
        }
    }
}
