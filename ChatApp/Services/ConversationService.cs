using ChatApp.DataContext;
using ChatApp.DTOs.Chat;
using ChatApp.DTOs.Helper;
using ChatApp.Entities;
using ChatApp.Exceptions;
using ChatApp.Interfaces;
using ChatApp.Utils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Services
{
    public class ConversationService:IConversationService
    {
        private readonly ChatAppDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly JWT jwt;
        public ConversationService(ChatAppDbContext context, IHubContext<ChatHub> hubContext,JWT jwt)    
        {
            _context = context;
            _hubContext = hubContext;
            this.jwt = jwt;
        }

        public async Task<int> CreateOrGetConversation(int currentUserId, int otherUserId)
        {
            if (currentUserId == otherUserId)
                throw new ForbiddenException("Cannot create conversation with yourself");

            var user1 = Math.Min(currentUserId, otherUserId);
            var user2 = Math.Max(currentUserId, otherUserId);

            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    c.User1Id == user1 &&
                    c.User2Id == user2);

            if (conversation != null)
                return conversation.ConversationId;

            conversation = new Conversation
            {
                User1Id = user1,
                User2Id = user2,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Conversations.AddAsync(conversation);
            await _context.SaveChangesAsync();

            return conversation.ConversationId;
        }

        public async Task MarkConversationAsRead(int conversationId)
        {
            int currentUserId = await jwt.GetUserID();
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.ConversationId == conversationId);

            if (conversation == null)
                throw new NotFoundException("Conversation not found");

            if (conversation.User1Id != currentUserId &&
                conversation.User2Id != currentUserId)
                throw new ForbiddenException("Unauthorized");

            await _context.Messages
                .Where(m =>
                    m.ConversationId == conversationId &&
                    m.SenderId != currentUserId &&
                    !m.IsRead)
                .ExecuteUpdateAsync(s =>
                    s.SetProperty(m => m.IsRead, true));

            await _hubContext.Clients
                .Group($"conversation-{conversationId}")
                .SendAsync("MessagesRead", new
                {
                    ConversationId = conversationId,
                    ReaderId = currentUserId
                });
        }

        public async Task<PagedResponse<GetMessageResponse>> GetConversationMessages(
     int conversationId,
     int pageNumber,
     int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 20;

            var query = _context.Messages
                .Where(m => m.ConversationId == conversationId);

            var totalCount = await query.CountAsync();

            var messages = await query
                .OrderByDescending(m => m.SentAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new GetMessageResponse
                {
                    MessageId = m.MessageId,
                    SenderId = m.SenderId,
                    SenderName = m.Sender!.FirstName + " " + m.Sender!.LastName,
                    MessageText = m.MessageText,
                    SentAt = m.SentAt,
                    IsRead = m.IsRead
                })
                .ToListAsync();

            messages.Reverse();

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return new PagedResponse<GetMessageResponse>
            {
                Items = messages,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }

    }
}
