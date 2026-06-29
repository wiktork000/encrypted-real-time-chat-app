using ChatApp.DTOs;
using ChatApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Net;
using System.Threading.Tasks;

namespace ChatApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ConversationsController(IAuthService authService, IConversationService conversationService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;
        private readonly IConversationService _conversationService = conversationService;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConversationDto>>> GetConversations()
        {
            var userId = _authService.GetCurrentUserId(User);
            if (userId == null) return Unauthorized();

            var conversations = await _conversationService.GetConversationsForUserAsync(userId.Value);

            return Ok(conversations);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ConversationDto>> GetConversation(int id)
        {
            var userId = _authService.GetCurrentUserId(User);
            if (userId == null) return Unauthorized();

            var isParticipant = await _conversationService.IsUserInConversationAsync(id, userId.Value); 

            if (!isParticipant)
            {
                return Forbid("You are not a participant in this conversation");
            }

            try
            {
                var conversation = await _conversationService.GetConversationByIdAsync(id, userId.Value);
                return Ok(conversation);
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<ActionResult<ConversationDto>> CreateConversation(CreateConversationDto createConversationDto)
        {
            var userId = _authService.GetCurrentUserId(User);
            if (userId == null) return Unauthorized();

            try
            {
                var conversation = await _conversationService.CreateConversationAsync(createConversationDto.Name, createConversationDto.ParticipantIds, userId.Value);
                return CreatedAtAction(nameof(GetConversation), new { id = conversation.Id }, conversation);
            }
            catch
            {
                return BadRequest();
            }
            
        }

        //[HttpPost("{id}/participants")]
        //public async Task<IActionResult> AddParticipant(int id, AddParticipantDto addParticipantDto)
        //{
        //    var userId = _authService.GetCurrentUserId(User);
        //    if (userId == null) return Unauthorized();

        //    var isParticipant = await _context.ConversationParticipants
        //        .AnyAsync(cp => cp.ConversationId == id && cp.UserId == userId);

        //    if (!isParticipant)
        //    {
        //        return Forbid("You are not a participant in this conversation");
        //    }

        //    var existingParticipant = await _context.ConversationParticipants
        //        .AnyAsync(cp => cp.ConversationId == id && cp.UserId == addParticipantDto.UserId);

        //    if (existingParticipant)
        //    {
        //        return BadRequest("User is already a participant");
        //    }

        //    var participant = new ConversationParticipant
        //    {
        //        UserId = addParticipantDto.UserId,
        //        ConversationId = id
        //    };

        //    _context.ConversationParticipants.Add(participant);
        //    await _context.SaveChangesAsync();

        //    return Ok();
        //}

        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> RemoveConversation(int id)
        {
            var userId = _authService.GetCurrentUserId(User);
            if (userId == null) return Unauthorized();

            if (!await _conversationService.IsUserInConversationAsync(id, userId.Value))
            {
                return Forbid("You are not a participant in this conversation");
            }

            await _conversationService.RemoveConversationAsync(id, userId.Value);    

            return Ok();
        }

    }
}
