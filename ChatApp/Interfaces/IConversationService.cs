namespace ChatApp.Interfaces
{
    public interface IConversationService
    {
       Task<int> CreateOrGetConversation(int currentUserId, int otherUserId);
       Task MarkConversationAsRead(int conversationId);


    }
}
