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

    public async Task<AuthResult> RegisterAsync(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName))
        {
            return new AuthResult
            {
                IsSuccess = false,
                Message = "All fields are required."
            };
        }

        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());

        if (existingUser != null)
        {
            return new AuthResult
            {
                IsSuccess = false,
                Message = "Email is already registered."
            };
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
        var authResponse = _mapper.Map<AuthResponse>(user);

        return new AuthResult
        {
            IsSuccess = true,
            Message = "User registered successfully.",
            Token = token,
            AuthData = authResponse
        };
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return new AuthResult
            {
                IsSuccess = false,
                Message = "Email and password are required."
            };
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());

        if (user == null)
        {
            return new AuthResult
            {
                IsSuccess = false,
                Message = "Invalid email or password."
            };
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            return new AuthResult
            {
                IsSuccess = false,
                Message = "Invalid email or password."
            };
        }

        var token = _jwtService.GetToken(user);
        var authResponse = _mapper.Map<AuthResponse>(user);

        return new AuthResult
        {
            IsSuccess = true,
            Message = "Login successful.",
            Token = token,
            AuthData = authResponse
        };
    }
}