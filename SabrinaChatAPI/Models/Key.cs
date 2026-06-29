using System.ComponentModel.DataAnnotations;

namespace ChatApp.Models
{
    public class Key
    {
        public int Id { get; set; }

        public int ConversationId { get; set; }
        public virtual Conversation Conversation { get; set; } = null!;
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;


        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public int FromMessageId { get; set; }
        public virtual Message FromMessage { get; set; } = null!;
        public int? ToMessageId { get; set; } = null;
        public virtual Message? ToMessage { get; set; } = null!;

        [Required]
        [MaxLength(500)]
        public string KeyValue { get; set; } = string.Empty;

        public bool IsValid => ToMessage == null;

        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
