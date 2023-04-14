namespace ChatService.Repositories
{
    using ChatService.Data.Models;

    public interface IChatRepository
    {
        Chat GetChat(int id);
        Task CreateRoom(string name, Guid userId);
        Task JoinRoom(int chatId, Guid userId);
        IEnumerable<Chat> GetChats(Guid userId);
        Task<int> CreatePrivateRoom(Guid rootId, Guid targetId);
        IEnumerable<Chat> GetPrivateChats(Guid userId);

        Task<Message> CreateMessage(int chatId, string message, Guid userId);
    }
}
