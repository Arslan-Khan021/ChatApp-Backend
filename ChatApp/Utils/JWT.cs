using ChatApp.DataContext;
using ChatApp.Entities;
using ChatApp.Exceptions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Utils
{
    public class JWT
    {
        private readonly ChatAppDbContext context;
        private readonly IConfiguration configuration;
        private readonly IHttpContextAccessor httpContextAccessor;

        public JWT(ChatAppDbContext context, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            this.context = context;
            this.configuration = configuration;
            this.httpContextAccessor = httpContextAccessor;
        }

        public string GetToken(User user)
        {
            var claims = new[]
                   {
                        new Claim(JwtRegisteredClaimNames.Sub,configuration["Jwt:Subject"]),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim("UserId",user.UserId.ToString()),
                        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                        new Claim("Email", user.Email),
                        new Claim(ClaimTypes.Role,"User")
                    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
            var signIN = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                configuration["Jwt:Issuer"],
                configuration["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddDays(5),
                signingCredentials: signIN
                );
            string tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
            return tokenValue;
        }
        public async Task<int> GetUserID()
        {
            var httpContext = httpContextAccessor.HttpContext;
            var userId = httpContext?.User?.FindFirst("UserId")?.Value; 
            int Id= int.TryParse(userId, out var id) ? id : 0;
            if (Id != 0)
            {
                var user = await context.Users.FindAsync(Id);
                if(user is null)
                {
                    throw new NotFoundException("User not found!");
                }
            }
            else
            {
                throw new UnauthorizedException("Unauthorized!");
            }
            return Id;
        }

    }
}
