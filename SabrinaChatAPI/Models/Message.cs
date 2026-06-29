using System.ComponentModel.DataAnnotations;

namespace ChatApp.Models
{
    public class Message
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public int ConversationId { get; set; }
        public virtual Conversation Conversation { get; set; } = null!;

        public int AuthorId { get; set; }
        public virtual User Author { get; set; } = null!;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
