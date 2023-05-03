namespace ChatService.Dtos
{
    public class NotificationDto
    {
        public int? RequestId { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
}