namespace ChatService.Hub
{
    using System.IdentityModel.Tokens.Jwt;

    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.EntityFrameworkCore;

    using Reset.Domain.Dtos.SignalRDtos.ChatDtos;
    using Reset.Domain.Entities.Identity;
    using Reset.Application.Interfaces.Repositories;
    using Reset.Domain.Dtos.SignalRDtos.MessageDtos;
    using Reset.Domain.Dtos.SignalRDtos.NotificationDtos;
    using Reset.Domain.Dtos.SignalRDtos.UserDtos;
    using Reset.Infrastructure.DbContext;

    using Hub = Microsoft.AspNetCore.SignalR.Hub;
    using static Reset.Common.Constants.Permissions;

    public class ChatHub : Hub
    {
        private readonly IDictionary<string, UserConnection> _connections;
        private readonly IChatRepository _chatRepository;
        private readonly IUserRepository _userRepository;
        private readonly ApplicationDbContext _dbContext;


        public ChatHub(
            IDictionary<string, UserConnection> connections,
            IChatRepository chatRepository,
            IUserRepository userRepository,
            ApplicationDbContext dbContext)
        {
            _connections = connections;
            _chatRepository = chatRepository;
            _userRepository = userRepository;
            _dbContext = dbContext;
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
            var user = GetUserAsync();
            _connections[Context.ConnectionId] = new UserConnection
            {
                UserId = user.Id,
                ConnectedDate = DateTime.UtcNow
            };

            await base.OnConnectedAsync();
        }

        public async Task<IEnumerable<UserDto>> GetUsers()
        {
            var user = GetUserAsync();

            return await _userRepository.GetUsers()
                       .Where(u => u.Id != user.Id)
                       .Select( u => new UserDto
                       {
                           FirstName = u.FirstName,
                           LastName = u.LastName,
                           UserId = u.Id,
                           IsOnline = u.IsOnline,
                           LastActiveDate = u.LastActiveDate
                       })
                       .ToListAsync();
        }

        public async Task SetUserActivenessStatus(bool isOnline)
        {
            var currentUser = GetUserAsync();
            _dbContext.currentUser = currentUser;

            currentUser.IsOnline = isOnline;
            currentUser.LastActiveDate = DateTime.UtcNow;

            await _userRepository.UpdateUserAsync(currentUser);

            //TODO: filter users
            await Clients.Others.SendAsync(
                "SetUserStatus",
                new UserDto
                    {
                        FirstName = currentUser.FirstName,
                        LastName = currentUser.LastName,
                        UserId = currentUser.Id,
                        IsOnline = currentUser.IsOnline,
                        LastActiveDate = currentUser.LastActiveDate
                    });
        }

        public async Task<IEnumerable<NotificationDto>> GetNotifications()
        {
            var user = GetUserAsync();

            var notifications = await _chatRepository.GetNotifications(user.Id);

            return notifications.Select(n => new NotificationDto
            {
                Subject = n.Subjet,
                Message = n.Message,
                Timestamp = n.CreatedDate,
                RequestId = n.Chat.RequestId
            });
        }

        public async Task<int> GetNotificationsCount()
        {
            try
            {
                var user = GetUserAsync();

                var notifs = await _chatRepository.GetNotificationsCount(user.Id);
                return notifs;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return 1;
        }

        public async Task<ChatDto> GetGeneralChat()
        {
            try
            {
                var chat = _chatRepository.GetGeneralChat();

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
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return new ChatDto();
        }

        public async Task<ChatDto> GetChatByRequestId(int requestId)
        {
            var currentUser = GetUserAsync();
            _dbContext.currentUser = currentUser;

            var chat = await _chatRepository.GetChatByRequestId(requestId);

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

        public async Task SendRequestMessage(IEnumerable<Guid> mentionedUsers, string message, int? requestId, string requestNumber)
        {
            var currentUser = GetUserAsync();
            _dbContext.currentUser = currentUser;

            var result = await _chatRepository.CreateMessage(mentionedUsers, message, currentUser, requestId, requestNumber);

            var newMessage = result.Item1;
            var chatUser = result.Item2;
            var notification = result.Item3;

            await Clients.All.SendAsync(
                    "SetRequestChat",
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
                    }, requestId);

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
                    Timestamp = notification.CreatedDate,
                    RequestId = requestId
                });
        }

        public async Task SendMessage(IEnumerable<Guid> mentionedUsers, string message)
        {
            var currentUser = GetUserAsync();
            _dbContext.currentUser = currentUser;

            var result = await _chatRepository.CreateMessage(mentionedUsers, message, currentUser, null, null);

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
                        Timestamp = notification.CreatedDate
                    });
        }

        private ApplicationUser GetUserAsync()
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

                Guid userId = Guid.Parse(input: token.Claims.First(predicate: c => c.Type == "userId").Value);

                ApplicationUser user = _userRepository.GetUser(u => u.Id == userId);

                return user;
            }
            catch
            {
                return default;
            }
        }
    }
}
