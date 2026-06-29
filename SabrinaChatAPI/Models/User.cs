using System.ComponentModel.DataAnnotations;

namespace ChatApp.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string PrivateKey { get; set; } = string.Empty;

        [Required]
        public string PublicKey { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
        public virtual ICollection<ConversationParticipant> ConversationParticipants { get; set; } = new List<ConversationParticipant>();

        public virtual ICollection<UserCredentials> UsersCredentials { get; set; } = new List<UserCredentials>();
        public virtual ICollection<Key> Keys { get; set; } = new List<Key>();

    }
}
