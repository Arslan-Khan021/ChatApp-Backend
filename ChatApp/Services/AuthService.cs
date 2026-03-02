using AutoMapper;
using ChatApp.DataContext;
using ChatApp.DTOs.Auth;
using ChatApp.Entities;
using ChatApp.Interfaces;
using ChatApp.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class AuthService : IAuthService
{
    private readonly ChatAppDbContext _context;
    private readonly JWT _jwtService;
    private readonly PasswordHasher<User> _passwordHasher;
    private readonly IMapper _mapper;

    public AuthService(
        ChatAppDbContext context,
        JWT jwtService,
        IMapper mapper)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordHasher = new PasswordHasher<User>();
        _mapper = mapper;   
    }

    public async Task<(bool IsSuccess, string Message, string? Token, AuthResponse? authService)> RegisterAsync(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName))
        {
            return (false, "All fields are required.", null,null);
        }

        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (existingUser != null)
        {
            return (false, "Email is already registered.", null,null);
        }

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim().ToLower()
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var token = _jwtService.GetToken(user);
     
        AuthResponse response = _mapper.Map<AuthResponse>(user);

        return (true, "User registered successfully.", token,response);
    }

    
    public async Task<(bool IsSuccess, string Message, string? Token, AuthResponse? authService)> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return (false, "Email and password are required.", null,null);
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());

        if (user == null)
        {
            return (false, "Invalid email or password.", null, null);
        }

        var result = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            return (false, "Invalid email or password.", null, null );
        }

        var token = _jwtService.GetToken(user);
        AuthResponse response = _mapper.Map<AuthResponse>(user);
        return (true, "Login successful.", token,response);
    }
}