using System.ComponentModel.DataAnnotations;

namespace ChatApp.Models
{
    public class Conversation
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
        public virtual ICollection<Key> Keys { get; set; } = new List<Key>();
        public virtual ICollection<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();
    }
}
