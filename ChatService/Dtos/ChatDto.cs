using ChatService.Data.Models;

namespace ChatService.Dtos
{
    public class ChatDto
    {
        public List<MessageDto> Messages { get; set; }
        public List<ChatUserDto> Users { get; set; }
        public int Id { get; set; }
        public int? RequestId { get; set; }
    }
}
