using Microsoft.AspNetCore.SignalR;

namespace ChatService.Hub
{
    using ChatService.Data.Models;
    using ChatService.Repositories;

    using Microsoft.AspNetCore.Identity;

    using Hub = Microsoft.AspNetCore.SignalR.Hub;

    public class ChatHub : Hub
    {
        private readonly string _botUser;
        private readonly IDictionary<string, UserConnection> _connections;
        private IChatRepository _repo;
        private readonly UserManager<ApplicationUser> _userManager;


        public ChatHub(IDictionary<string, UserConnection> connections, UserManager<ApplicationUser> userManager)
        {
            _botUser = "";
            _connections = connections;
            _userManager = userManager;
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                _connections.Remove(Context.ConnectionId);
                Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", _botUser, $"{userConnection.User} has left");
                SendUsersConnected(userConnection.Room);
            }

            return base.OnDisconnectedAsync(exception);
        }

        public Task GetChats(Guid userId)
        {
            // userId = Guid.Parse(input: Context.User.FindFirst(type: "UserId")?.Value);

            var chats = _repo.GetChats(userId);

            return Clients.Caller.SendAsync("GetChats", chats);
        }

        public Task GetChatById(int chatId)
        {
            var chats = _repo.GetPrivateChats(chatId);

            return Clients.Caller.SendAsync("GetChats", chats);
        }

        public async Task JoinRoom(UserConnection userConnection)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.Room);

            _connections[Context.ConnectionId] = userConnection;

            await Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", _botUser, $"{userConnection.User} has joined {userConnection.Room}");

            await SendUsersConnected(userConnection.Room);
        }

        public async Task SendMessage(string message)
        {
            if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                await Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", userConnection.User, message);
            }
        }

        public Task SendUsersConnected(string room)
        {
            var users = _connections.Values
                .Where(c => c.Room == room)
                .Select(c => c.User);

            return Clients.Group(room).SendAsync("UsersInRoom", users);
        }
    }
}
