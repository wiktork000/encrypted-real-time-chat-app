using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ChatApp.Data;
using ChatApp.Models;
using ChatApp.DTOs;
using ChatApp.Services;
using System.ComponentModel.DataAnnotations;

namespace ChatApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MessagesController(SabrinaChatDbContext context, IAuthService authService, IConversationService conversationService, IMessageService messageService) : ControllerBase
    {
        private readonly SabrinaChatDbContext _context = context;
        private readonly IAuthService _authService = authService;
        private readonly IConversationService _conversationService = conversationService;
        private readonly IMessageService _messageService = messageService;

        [HttpPost]
        public async Task<ActionResult<MessageDto>> SendMessage(SendMessageDto sendMessageDto)
        {
            var userId = _authService.GetCurrentUserId(User);
            if (userId == null) return Unauthorized();

            // Check if user is participant in conversation
            var isParticipant = await _conversationService.IsUserInConversationAsync(sendMessageDto.ConversationId, userId.Value);

            if (!isParticipant)
            {
                return Forbid("You are not a participant in this conversation");
            }

            var message = await _messageService.SendMessageAsync(sendMessageDto, userId.Value);
            
            return Ok(message);
        }

        [HttpGet("conversation/{conversationId}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForConversation(int conversationId, [FromQuery] int? limit = 50, [FromQuery] int? offset = 0)
        {
            var userId = _authService.GetCurrentUserId(User);
            if (userId == null) return Unauthorized();

            var isParticipant = await _conversationService.IsUserInConversationAsync(conversationId, userId.Value);

            if (!isParticipant)
            {
                return Forbid("You are not a participant in this conversation");
            }

            var messages = await _messageService.GetMessagesForConversationAsync(conversationId, userId.Value, limit, offset);

            return Ok(messages);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MessageDto>> GetMessage(int id)
        {
            var userId = _authService.GetCurrentUserId(User);
            if (userId == null) return Unauthorized();

            var message = await _messageService.GetMessageByIdAsync(id);

            if (message == null)
            {
                return NotFound();
            }

            // Check if user is participant in the conversation
            var isParticipant = await _conversationService.IsUserInConversationAsync(message.ConversationId, userId.Value);

            if (!isParticipant)
            {
                return Forbid("You are not a participant in this conversation");
            }

            

            return Ok(message);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<MessageDto>> UpdateMessage(int id, [FromBody] string newContent)
        {
            var userId = _authService.GetCurrentUserId(User);
            if (userId == null) return Unauthorized();
            var message = await _messageService.GetMessageByIdAsync(id);
            if (message == null)
            {
                return NotFound();
            }
            // Check if user is the author of the message
            if (message.Author.Id != userId.Value)
            {
                return Forbid("You can only update your own messages");
            }
            var updatedMessage = await _messageService.UpdateMessage(id, newContent, userId.Value);
            return Ok(updatedMessage);
        }
    }
}
