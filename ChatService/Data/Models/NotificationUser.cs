namespace ChatService.Data.Models
{
    public class NotificationUser
    {
        public int NotificationId { get; set; }
        public Notification Notification { get; set; }
        public int ChatUserId { get; set; }
        public ChatUser ChatUser { get; set; }
    }
}