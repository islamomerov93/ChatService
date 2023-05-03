using Microsoft.EntityFrameworkCore;

namespace ChatService.Repositories
{
    using ChatService.Data;
    using ChatService.Data.Models;
    using ChatService.Dtos;

    public class ChatRepository : IChatRepository
    {
        private ApplicationDbContext _ctx;

        public ChatRepository(ApplicationDbContext ctx) => _ctx = ctx;

        public async Task<Chat> CreateGeneralChat()
        {
            var chat = new Chat
            {
                Name = "General Chat",
                Type = ChatType.General
            };

            _ctx.Chats.Add(chat);

            await _ctx.SaveChangesAsync();
            return chat;
        }

        public async Task<Chat> CreateRequestChat(int requestId, string name = null)
        {
            var chat = new Chat
            {
                Name = name,
                RequestId = requestId,
                Type = ChatType.Request
            };

            _ctx.Chats.Add(chat);
            await _ctx.SaveChangesAsync();
            return chat;
        }

        public Chat GetChatByRequestId(int requestId)
        {
            return _ctx.Chats
                .Include(x => x.Users)
                    .ThenInclude(cu => cu.User)
                .Include(x => x.Messages)
                .FirstOrDefault(x => x.RequestId == requestId);
        }

        public Chat GetGeneralChat()
        {
            return _ctx.Chats
                .Include(x => x.Users)
                    .ThenInclude(cu => cu.User)
                .Include(x => x.Messages)
                .FirstOrDefault(x => x.Type == ChatType.General);
        }

        public async Task<Tuple<Message, ChatUser, Notification>> CreateMessage(IEnumerable<Guid> mentionedUsers, string message,ApplicationUser user, int? requestId, string requestNumber)
        {
            Chat chat;
            Notification notification = new Notification
            {
                Message = $"{user.FirstName} {user.LastName} added new message."
            };

            if (requestId.HasValue)
            {
                chat = await _ctx.Chats
                           .Include(c => c.Users)
                                .ThenInclude(cu => cu.User)
                           .FirstOrDefaultAsync(c => c.RequestId == requestId);

                if (chat == null)
                {
                    chat = await CreateRequestChat(requestId.Value);
                }

                notification.Subjet = $"New Message - Request chat -{requestNumber}";
            }
            else
            {
                chat = await _ctx.Chats
                           .Include(c => c.Users)
                                .ThenInclude(cu => cu.User)
                           .FirstOrDefaultAsync(c => c.Type == ChatType.General);

                if (chat == null)
                {
                    chat = await CreateGeneralChat();
                }

                notification.Subjet = $"New Message - General chat";
            }
            
            var createMessage = new Message
            {
                ChatId = chat.Id,
                Text = message,
                UserId = user.Id,
                Timestamp = DateTime.UtcNow
            };

            ChatUser chatUser = chat.Users.FirstOrDefault(u => u.UserId == user.Id);
            if (chatUser == null)
            {
                chatUser = new ChatUser
                {
                    UserId = user.Id,
                    User = user,
                    Role = ChatUserRole.Member
                };
                chat.Users.Add(chatUser);
            }

            notification.MentionedUsers =
                mentionedUsers.Select(u => new NotificationUser { ChatUserId = chatUser.Id }).ToList();
            notification.ChatId = chat.Id;
            notification.Timestamp = createMessage.Timestamp;
            _ctx.Notifications.Add(notification);
            _ctx.Messages.Add(createMessage);
            await _ctx.SaveChangesAsync();

            return Tuple.Create(createMessage, chatUser, notification);
        }


        public IEnumerable<Chat> GetChats(Guid userId)
        {
            return _ctx.Chats
                .Include(x => x.Users)
                .Where(x => !x.Users
                    .Any(y => y.UserId == userId))
                .ToList();
        }

        public Task<Chat> GetChat(int chatId)
        {
            return _ctx.Chats
                .Include(x => x.Users)
                .FirstOrDefaultAsync(x => x.Id == chatId);
        }

        public async Task<IEnumerable<Notification>> GetNotifications(Guid userId)
        {
            return await  _ctx.Notifications
                .Include(n=>n.Chat)
                .Where(x => x.MentionedUsers.Any(u => u.ChatUser.UserId != userId))
                .ToListAsync();
        }

        public Task<int> GetNotificationsCount(Guid userId)
        {
            return this._ctx.Notifications
                .Where(x => x.MentionedUsers.Any(u => u.ChatUser.UserId != userId))
                .CountAsync();
        }

        //public async Task<int> CreatePrivateRoom(Guid rootId, Guid targetId)
        //{
        //    var chat = new Chat
        //                   {
        //                       Type = ChatType.Private
        //                   };

        //    chat.Users.Add(new ChatUser
        //                       {
        //                           UserId = targetId
        //                       });

        //    chat.Users.Add(new ChatUser
        //                       {
        //                           UserId = rootId
        //                       });

        //    _ctx.Chats.Add(chat);

        //    await _ctx.SaveChangesAsync();

        //    return chat.Id;
        //}

        //public IEnumerable<Chat> GetPrivateChats(Guid userId)
        //{
        //    return _ctx.Chats
        //           .Include(x => x.Users)
        //               .ThenInclude(x => x.User)
        //           .Where(x => x.Type == ChatType.Private
        //               && x.Users
        //                   .Any(y => y.UserId == userId))
        //           .ToList();
        //}

        //public async Task JoinRoom(int chatId, Guid userId)
        //{
        //    var chatUser = new ChatUser
        //    {
        //        ChatId = chatId,
        //        UserId = userId,
        //        Role = ChatUserRole.Member
        //    };

        //    _ctx.ChatUsers.Add(chatUser);

        //    await _ctx.SaveChangesAsync();
        //}
    }
}