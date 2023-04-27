namespace ChatService.Data.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Subjet { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public int ChatId { get; set; }
        public Chat Chat { get; set; }
        public ICollection<NotificationUser> MentionedUsers { get; set; }
    }
}