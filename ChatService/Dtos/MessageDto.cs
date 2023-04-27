namespace ChatService.Dtos
{
    public class MessageDto
    {
        public Guid UserId { get; set; }
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
