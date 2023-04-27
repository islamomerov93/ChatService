namespace ChatService.Repositories
{
    using ChatService.Data.Models;

    public interface IChatRepository
    {
        Chat GetGeneralChat();

        Chat GetChatByRequestId(int id);

        Task<Chat> GetChat(int chatId);

        Task<IEnumerable<Notification>> GetNotifications(Guid userId);

        Task<Chat> CreateGeneralChat();

        Task<Chat> CreateRequestChat(int requestId, string name = null);

        Task<Tuple<Message, ChatUser, Notification>> CreateMessage(
            IEnumerable<Guid> mentionedUsers,
            string message,
            ApplicationUser user,
            int? requestId,
            string requestNumber);
    }
}
