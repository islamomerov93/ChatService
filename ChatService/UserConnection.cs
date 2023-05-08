namespace ChatService
{
    public class UserConnection
    {
        public Guid UserId { get; set; }
        public DateTime ConnectedDate { get; set; }
    }

    public class RequestChatUserConnection : UserConnection
    {
        public int RequestId { get; set; }
    }
}
