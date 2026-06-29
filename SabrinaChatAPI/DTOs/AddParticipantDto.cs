using System.ComponentModel.DataAnnotations;

namespace ChatApp.DTOs
{
    public class AddParticipantDto
    {
        [Required]
        public int UserId { get; set; }
    }
}
