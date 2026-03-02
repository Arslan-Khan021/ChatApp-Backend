using ChatApp.DTOs.Auth;
using ChatApp.Entities;

namespace ChatApp.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterRequest request);
       Task<AuthResult> LoginAsync(LoginRequest request);
    }
}
