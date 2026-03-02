namespace ChatApp.DTOs.Auth
{
    public class AuthResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public AuthResponse? AuthData { get; set; }
    }
}
