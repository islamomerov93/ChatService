namespace ChatService.Dtos.ChatDtos
{
    using ChatService.Data.Models;

    public class ChatUserDto
    {
        public Guid UserId { get; set; }
        public ChatUserRole Role { get; set; }
        public string Username { get; set; }
        public bool IsActive { get; set; }
    }
}
