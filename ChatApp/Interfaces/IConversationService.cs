using ChatApp.DTOs.Chat;
using ChatApp.DTOs.Helper;

namespace ChatApp.Interfaces
{
    public interface IConversationService
    {
       Task<int> CreateOrGetConversation(int currentUserId, int otherUserId);
       Task MarkConversationAsRead(int conversationId);
       Task<PagedResponse<GetMessageResponse>> GetConversationMessages(int conversationId, int pageNumber, int pageSize);

    }
}
