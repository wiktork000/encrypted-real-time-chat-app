using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ChatApp.Data;
using ChatApp.DTOs;
using ChatApp.Services;

namespace ChatApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class KeysController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IKeyService _keyService;
        private readonly IConversationService _conversationService;

        public KeysController(IAuthService authService, IKeyService keyService, IConversationService conversationService)
        {
            _authService = authService;
            _keyService = keyService;
            _conversationService = conversationService;
        }

        [HttpGet("conversation/{conversationId}")]
        public async Task<ActionResult<IEnumerable<KeyDto>>> GetKeysForConversation(int conversationId)
        {
            var userId = _authService.GetCurrentUserId(User);
            if (userId == null) return Unauthorized();

            var isParticipant = await _conversationService.IsUserInConversationAsync(conversationId, userId.Value);

            if (!isParticipant)
            {
                return Forbid("You are not a participant in this conversation");
            }

            var keys = await _keyService.GetUserConversationKeys(conversationId, userId.Value);

            return Ok(keys);
        }

        [HttpGet("conversation/{conversationId}/current")]
        public async Task<ActionResult<IEnumerable<KeyDto>>> GetCurrentKeyForConversation(int conversationId)
        {
            var userId = _authService.GetCurrentUserId(User);
            if (userId == null) return Unauthorized();

            var isParticipant = await _conversationService.IsUserInConversationAsync(conversationId, userId.Value);

            if (!isParticipant)
            {
                return Forbid("You are not a participant in this conversation");
            }

            var keys = await _keyService.GetCurrentKeyForConversation(conversationId, userId.Value);

            return Ok(keys);
        }
    }
}
