using System.ComponentModel.DataAnnotations;

namespace ChatApp.DTOs
{
    public class CreateConversationDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public List<int> ParticipantIds { get; set; } = new();
    }
}
