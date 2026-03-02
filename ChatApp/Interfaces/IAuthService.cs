using ChatApp.DTOs.Auth;

namespace ChatApp.Interfaces
{
    public interface IAuthService
    {
        Task<(bool IsSuccess, string Message, string? Token)> RegisterAsync(RegisterRequest request);
        Task<(bool IsSuccess, string Message, string? Token)> LoginAsync(LoginRequest request);
    }
}
