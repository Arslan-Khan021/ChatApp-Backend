using ChatApp.DTOs.Auth;
using ChatApp.Exceptions;
using ChatApp.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody]RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);

            if (!result.IsSuccess)
                throw new BadRequestException(result.Message);

            return Ok(new { Token = result.Token, UserDetails=result.AuthData });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody]LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);

            if (!result.IsSuccess)
                throw new UnauthorizedException(result.Message);

            return Ok(new { Token = result.Token, UserDetails = result.AuthData });
        }
    }
}
