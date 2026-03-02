using ChatApp.DataContext;
using ChatApp.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
        return int.Parse(
            Context.User.FindFirst(ClaimTypes.NameIdentifier)!.Value
        );
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
            .SendAsync("UserJoined", userId);
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

        var groupName = GetGroupName(conversationId);

        await Clients.Group(groupName).SendAsync(
            "ReceiveMessage",
            new
            {
                MessageId = chatMessage.MessageId,
                ConversationId = conversationId,
                SenderId = userId,
                Content = message,
                SentAt = chatMessage.SentAt
            });
    }


    public async Task LeaveConversation(int conversationId)
    {
        var userId = GetUserId();
        var groupName = GetGroupName(conversationId);

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

        await Clients.Group(groupName)
            .SendAsync("UserLeft", userId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}