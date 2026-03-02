using ChatApp.DataContext;
using ChatApp.DTOs.Chat;
using ChatApp.DTOs.Helper;
using ChatApp.Interfaces;
using ChatApp.Utils;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Services
{
    public class UserService:IUserService
    {
        private readonly ChatAppDbContext _context;
        private readonly JWT jwt;

        public UserService(ChatAppDbContext context,JWT jwt)
        {
            this._context = context;
            this.jwt = jwt; 
        }
        public async Task<PagedResponse<ChatUserDto>> GetUsersAsync(int pageNumber,int pageSize)
        {
            int currentUserId = await jwt.GetUserID();
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var baseQuery = _context.Users
                .Where(u => u.UserId != currentUserId)
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName);

            var totalCount = await baseQuery.CountAsync();

            var users = await baseQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new List<ChatUserDto>();

            foreach (var user in users)
            {
                var conversation = await _context.Conversations
                    .Where(c =>
                        (c.User1Id == currentUserId && c.User2Id == user.UserId) ||
                        (c.User2Id == currentUserId && c.User1Id == user.UserId))
                    .FirstOrDefaultAsync();

                string? lastMessage = null;
                DateTime? lastMessageTime = null;
                int unreadCount = 0;
                int? conversationId = null;
                if (conversation != null)
                {
                    var lastMsg = await _context.Messages
                        .Where(m => m.ConversationId == conversation.ConversationId)
                        .OrderByDescending(m => m.SentAt)
                        .FirstOrDefaultAsync();

                    if (lastMsg != null)
                    {
                        lastMessage = lastMsg.MessageText;
                        lastMessageTime = lastMsg.SentAt;
                    }

                    unreadCount = await _context.Messages
                        .Where(m =>
                            m.ConversationId == conversation.ConversationId &&
                            m.SenderId != currentUserId &&
                            !m.IsRead)
                        .CountAsync();
                    conversationId = conversation.ConversationId;   
                }

                result.Add(new ChatUserDto
                {
                    UserId = user.UserId,
                    FullName = user.FirstName + " " + user.LastName,
                    Email = user.Email,
                    LastMessage = lastMessage,
                    LastMessageTime = lastMessageTime,
                    UnreadCount = unreadCount,
                    ConversationId = conversationId
                });
            }

            return new PagedResponse<ChatUserDto>
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Items = result
            };
        }
    }
}
