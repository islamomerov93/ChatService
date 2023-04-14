namespace ChatService.Data.Models
{
    public class Chat
    {
        public Chat()
        {
            this.Messages = new List<Message>();
            this.Users = new List<ChatUser>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public ChatType Type { get; set; }
        public ICollection<Message> Messages { get; set; }
        public ICollection<ChatUser> Users { get; set; }
    }
}