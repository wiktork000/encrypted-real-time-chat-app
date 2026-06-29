namespace ChatApp.DTOs
{
    public class CurrentUserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public string PrivateKey { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
