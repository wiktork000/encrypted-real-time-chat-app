namespace ChatApp.DTOs
{
    public class MessageDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public int ConversationId { get; set; }
        public string ConversationName { get; set; } = string.Empty;
        public UserDto Author { get; set; } = null!;
        public DateTime Timestamp { get; set; }
    }
}
