using ChatApp.Data;
using ChatApp.DTOs;
using ChatApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Services
{
    public interface IMessageService
    {
        public Task<MessageDto> SendMessageAsync(SendMessageDto sendMessageDto, int userId);
        public Task<IEnumerable<MessageDto>> GetMessagesForConversationAsync(int conversationId, int userId, int? limit = 50, int? offset = 0);
        public Task<MessageDto> UpdateMessage(int messageId, string newContent, int userId);
        public Task<MessageDto> GetMessageByIdAsync(int messageId);
    }

    public class MessageService : IMessageService
    {
        private readonly SabrinaChatDbContext _context;

        public MessageService(SabrinaChatDbContext context)
        {
            _context = context;
        }

        public async Task<MessageDto> SendMessageAsync(SendMessageDto sendMessageDto, int userId)
        {
            var message = new Message
            {
                Content = sendMessageDto.Content,
                ConversationId = sendMessageDto.ConversationId,
                AuthorId = userId
            };
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            // Load the message with related data
            var messageWithData = await _context.Messages
                .Include(m => m.Author)
                .Include(m => m.Conversation)
                .FirstAsync(m => m.Id == message.Id);
            return new MessageDto
            {
                Id = messageWithData.Id,
                Content = messageWithData.Content,
                ConversationId = messageWithData.ConversationId,
                ConversationName = messageWithData.Conversation.Name,
                Author = new UserDto
                {
                    Id = messageWithData.Author.Id,
                    Name = messageWithData.Author.Name,
                    CreatedAt = messageWithData.Author.CreatedAt
                }
            };
        }

        public async Task<IEnumerable<MessageDto>> GetMessagesForConversationAsync(int conversationId, int userId, int? limit = 50, int? offset = 0)
        {
            await _context.ConversationParticipants
                .Where(cp => cp.ConversationId == conversationId && cp.UserId == userId)
                .ExecuteUpdateAsync(cp => cp.SetProperty(c => c.NumberOfUnreadMessages, 0));
            await _context.SaveChangesAsync();
            return await _context.Messages
                .Where(m => m.ConversationId == conversationId)
                .OrderByDescending(m => m.Timestamp)
                .Skip(offset ?? 0)
                .Take(limit ?? 50)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    Content = m.Content,
                    ConversationId = m.ConversationId,
                    ConversationName = m.Conversation.Name,
                    Author = new UserDto
                    {
                        Id = m.Author.Id,
                        Name = m.Author.Name,
                        CreatedAt = m.Author.CreatedAt
                    },
                    Timestamp = m.Timestamp
                })
                .ToListAsync();
        }

        public async Task<MessageDto> GetMessageByIdAsync(int messageId)
        {
            var message = await _context.Messages
                .Include(m => m.Author)
                .Include(m => m.Conversation)
                .FirstOrDefaultAsync(m => m.Id == messageId);
            
            if (message == null)
            {
                throw new KeyNotFoundException("Message not found");
            }
            
            return new MessageDto
            {
                Id = message.Id,
                Content = message.Content,
                ConversationId = message.ConversationId,
                ConversationName = message.Conversation.Name,
                Author = new UserDto
                {
                    Id = message.Author.Id,
                    Name = message.Author.Name,
                    CreatedAt = message.Author.CreatedAt
                },
                Timestamp = message.Timestamp
            };
        }

        public async Task<MessageDto> UpdateMessage(int messageId, string newContent, int userId)
        {
            var message = await _context.Messages
                .Include(m => m.Author)
                .FirstOrDefaultAsync(m => m.Id == messageId);
            
            if (message == null)
            {
                throw new KeyNotFoundException("Message not found or you are not the author");
            }
            message.Content = newContent;
            await _context.SaveChangesAsync();
            return new MessageDto
            {
                Id = message.Id,
                Content = message.Content,
                ConversationId = message.ConversationId,
                ConversationName = message.Conversation.Name,
                Author = new UserDto
                {
                    Id = message.Author.Id,
                    Name = message.Author.Name,
                    CreatedAt = message.Author.CreatedAt
                },
                Timestamp = message.Timestamp
            };
        }
    }
}
