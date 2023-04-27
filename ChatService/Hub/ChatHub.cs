using Microsoft.AspNetCore.SignalR;

namespace ChatService.Hub
{
    using System.IdentityModel.Tokens.Jwt;

    using ChatService.Data.Models;
    using ChatService.Dtos;
    using ChatService.Repositories;

    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    using Hub = Microsoft.AspNetCore.SignalR.Hub;

    public class ChatHub : Hub
    {
        private readonly IDictionary<string, UserConnection> _connections;
        private readonly IChatRepository _repo;
        private readonly UserManager<ApplicationUser> _userManager;


        public ChatHub(
            IDictionary<string, UserConnection> connections,
            UserManager<ApplicationUser> userManager,
            IChatRepository repo)
        {
            _connections = connections;
            _userManager = userManager;
            _repo = repo;
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            if (_connections.Keys.Contains(Context.ConnectionId))
            {
                _connections.Remove(Context.ConnectionId);
            }

            return base.OnDisconnectedAsync(exception);
        }

        public override async Task OnConnectedAsync()
        {
            var user = await GetUserAsync();
            _connections[Context.ConnectionId] = new UserConnection
            {
                UserId = user.Id,
                ConnectedDate = DateTime.UtcNow
            };

            var users = _connections.Values
                .Select(c => c?.UserId);

            await Clients.Others.SendAsync("SetActiveUsers", users);

            await base.OnConnectedAsync();
        }

        public async Task<object> GetUsers()
        {
            var user = await GetUserAsync();

            return await _userManager.Users
                       .Where(u => u.Id != user.Id)
                       .Select(u=> new { UserName = $"{u.FirstName} {u.LastName}", UserId = u.Id})
                       .ToListAsync();
        }

        public async Task GetNotifications()
        {
            var user = await GetUserAsync();

            var notifications = await _repo.GetNotifications(user.Id);

            await Clients.Caller.SendAsync("SetUsers", notifications.Select(n=> new NotificationDto
            {
                Subject = n.Subjet,
                Message = n.Message,
                Timestamp = n.Timestamp,
                RequestId = n.Chat.RequestId
            }));
        }

        public async Task<ChatDto> GetGeneralChat()
        {
            var chat = _repo.GetGeneralChat();

            return new ChatDto
            {
                Id = chat.Id,
                RequestId = chat.RequestId,
                Messages = chat.Messages.Select(m => new MessageDto
                {
                    Text = m.Text,
                    UserId = m.UserId,
                    Timestamp = m.Timestamp
                }).ToList(),
                Users = chat.Users.Select(u => new ChatUserDto
                {
                    UserId = u.UserId,
                    Role = u.Role,
                    Username = $"{u.User.FirstName} {u.User.LastName}",
                    IsActive = _connections.Values.Any(c => c.UserId == u.UserId)
                }).ToList()
            };
        }

        public Task GetChatByRequestId(int requestId)
        {
            var chat = _repo.GetChatByRequestId(requestId);

            return Clients.Caller.SendAsync("SetChat", chat);
        }

        public async Task SendMessage(IEnumerable<Guid> mentionedUsers, string message, int? requestId, string requestNumber)
        {
            var user = await GetUserAsync();

            var result = await _repo.CreateMessage(mentionedUsers, message, user, requestId, requestNumber);

            var newMessage = result.Item1;
            var chatUser = result.Item2;
            var notification = result.Item3;

            await Clients.All.SendAsync(
                    "SetGeneralChat",
                    new MessageDto
                    {
                        Text = newMessage.Text,
                        UserId = newMessage.UserId,
                        Timestamp = newMessage.Timestamp
                    },
                    new ChatUserDto
                    {
                        UserId = newMessage.UserId,
                        Role = chatUser.Role,
                        Username = $"{chatUser.User.FirstName} {chatUser.User.LastName}",
                        IsActive = _connections.Values.Any(c => c.UserId == chatUser.UserId)
                    });

            List<string> usersNeedToNotify = new List<string>();

            foreach (var connection in _connections)
            {
                if (mentionedUsers.Contains(connection.Value.UserId))
                {
                    usersNeedToNotify.Add(connection.Key);
                }
            }
            
            await Clients.Clients(usersNeedToNotify)
                .SendAsync("SetNewNotification",
                new NotificationDto
                {
                    Subject = notification.Subjet,
                    Message = notification.Message,
                    Timestamp = notification.Timestamp,
                    RequestId = requestId
                });
        }

        private async Task<ApplicationUser> GetUserAsync()
        {
            var context = Context.GetHttpContext();
            try
            {
                string jwtToken = context.Request.Headers[key: "Authorization"].ToString();
                if (string.IsNullOrWhiteSpace(value: jwtToken))
                {
                    return default;
                }

                jwtToken = jwtToken.Replace(oldValue: "Bearer ", newValue: string.Empty);
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                JwtSecurityToken token = handler.ReadToken(token: jwtToken) as JwtSecurityToken;

                UserManager<ApplicationUser> userManager = context.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();

                Guid userId = Guid.Parse(input: token.Claims.First(predicate: c => c.Type == "userId").Value);

                ApplicationUser user = await userManager.Users.FirstAsync(predicate: u => u.Id == userId);

                await userManager.UpdateAsync(user: user);

                return user;
            }
            catch
            {
                return default;
            }
        }
    }
}
