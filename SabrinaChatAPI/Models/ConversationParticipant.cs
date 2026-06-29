namespace ChatApp.Models
{
    public class ConversationParticipant
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public int ConversationId { get; set; }
        public virtual Conversation Conversation { get; set; } = null!;

        public int NumberOfUnreadMessages { get; set; } = 0;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
