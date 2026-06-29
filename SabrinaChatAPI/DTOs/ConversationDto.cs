namespace ChatApp.DTOs
{
    public class ConversationDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<UserDto> Participants { get; set; } = new();
        public int NumberOfUnreadMessages { get; set; } = 0;
        public DateTime LastMessageDate { get; set; } = DateTime.MinValue;
    }
}
