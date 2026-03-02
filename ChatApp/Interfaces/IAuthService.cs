using ChatApp.DTOs.Auth;
using ChatApp.Entities;

namespace ChatApp.Interfaces
{
    public interface IAuthService
    {
        Task<(bool IsSuccess, string Message, string? Token,AuthResponse? authService)> RegisterAsync(RegisterRequest request);
        Task<(bool IsSuccess, string Message, string? Token, AuthResponse? authService)> LoginAsync(LoginRequest request);
    }
}
