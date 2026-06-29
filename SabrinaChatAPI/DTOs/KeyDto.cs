namespace ChatApp.DTOs
{
    public class KeyDto
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public int UserId { get; set; }
        public int FirstMessageId { get; set; }
        public int? LastMessageId { get; set; } = null;
        public string KeyValue { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool IsActive { get; set; }
        
    }
}
