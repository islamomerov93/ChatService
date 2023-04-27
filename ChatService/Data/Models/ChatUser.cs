namespace ChatService.Data.Models
{
    public class ChatUser
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int ChatId { get; set; }
        public Chat Chat { get; set; }
        public ChatUserRole Role { get; set; }
    }
}