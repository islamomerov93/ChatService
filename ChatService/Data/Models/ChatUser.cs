namespace ChatService.Data.Models
{
    public class ChatUser
    {
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int ChatId { get; set; }
        public Chat Chat { get; set; }
    }
}