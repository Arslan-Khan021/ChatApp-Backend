using ChatApp.DataContext;
using ChatApp.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChatApp.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ChatAppDbContext _context;

        public ChatHub(ChatAppDbContext context)
        {
            _context = context;
        }

        private string GetGroupName(int conversationId)
        {
            return $"conversation-{conversationId}";
        }

        private int GetUserId()
        {
            // Try UserId first (matches our JWT claim and we cleared the map)
            var userIdClaim = Context.User?.FindFirst("UserId") 
                ?? Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                ?? Context.User?.FindFirst("sub");

            if (userIdClaim == null)
            {
                var allClaims = Context.User?.Claims.Select(c => $"{c.Type}={c.Value}") ?? Enumerable.Empty<string>();
                var claimsList = string.Join(", ", allClaims);
                throw new HubException($"User identity not found. Claims found: {(string.IsNullOrEmpty(claimsList) ? "none" : claimsList)}");
            }

            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                throw new HubException($"User identity claim found but value is not an integer: {userIdClaim.Value}");
            }
            
            return userId;
        }

        public async Task JoinConversation(int conversationId)
        {
            var userId = GetUserId();

            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.ConversationId == conversationId);

            if (conversation == null)
                throw new HubException("Conversation not found");

            if (conversation.User1Id != userId &&
                conversation.User2Id != userId)
                throw new HubException("Unauthorized access");

            var groupName = GetGroupName(conversationId);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName)
                .SendAsync("userJoined", userId);
        }

        public async Task SendMessage(int conversationId, string message)
        {
            var userId = GetUserId();

            if (string.IsNullOrWhiteSpace(message))
                throw new HubException("Message cannot be empty");

            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.ConversationId == conversationId);

            if (conversation == null)
                throw new HubException("Conversation not found");

            if (conversation.User1Id != userId &&
                conversation.User2Id != userId)
                throw new HubException("Unauthorized");

            var chatMessage = new Message
            {
                ConversationId = conversationId,
                SenderId = userId,
                MessageText = message,
                SentAt = DateTime.UtcNow
            };

            await _context.Messages.AddAsync(chatMessage);
            await _context.SaveChangesAsync();

            var sender = await _context.Users.FindAsync(userId);
            var groupName = GetGroupName(conversationId);

            await Clients.Group(groupName).SendAsync(
                "receiveMessage",
                new
                {
                    MessageId = chatMessage.MessageId,
                    ConversationId = conversationId,
                    SenderId = userId,
                    SenderName = sender != null ? $"{sender.FirstName} {sender.LastName}" : "Unknown",
                    MessageText = message,
                    SentAt = chatMessage.SentAt
                });
        }

        public async Task LeaveConversation(int conversationId)
        {
            var userId = GetUserId();
            var groupName = GetGroupName(conversationId);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName)
                .SendAsync("userLeft", userId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}