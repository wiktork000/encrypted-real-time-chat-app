using System.ComponentModel.DataAnnotations;

namespace ChatApp.DTOs
{
    public class SendMessageDto
    {
        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public int ConversationId { get; set; }
    }
}
