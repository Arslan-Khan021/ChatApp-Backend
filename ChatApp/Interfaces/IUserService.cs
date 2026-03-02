using ChatApp.DTOs.Chat;
using ChatApp.DTOs.Helper;

namespace ChatApp.Interfaces
{
    public interface IUserService
    {
        Task<PagedResponse<ChatUserDto>> GetUsersAsync(int pageNumber,int pageSize);
    }
}
