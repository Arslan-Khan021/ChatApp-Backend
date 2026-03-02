using ChatApp.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatApp.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService userService;
        public UserController(IUserService userService)
        {
            this.userService = userService;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(int pageNumber = 1,int pageSize = 10)
        {
            var response = await userService
                .GetUsersAsync(pageNumber, pageSize);
            return Ok(response);
        }

    }
}
