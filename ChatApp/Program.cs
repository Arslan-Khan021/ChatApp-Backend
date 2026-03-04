using ChatApp.DataContext;
using ChatApp.Exceptions;
using ChatApp.Interfaces;
using ChatApp.Services;
using ChatApp.Utils;
using ChatApp.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Clear default claim mapping to keep JWT claim names (like "UserId")
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

// Add services to the container.
builder.Services.AddSignalR()
    .AddJsonProtocol(options => {
        options.PayloadSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

var config = builder.Configuration;
builder.Services.AddDbContext<ChatAppDbContext>
    (op => op.UseNpgsql(config.GetConnectionString("ChatAppConnectionString")));



builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<JWT>();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendCors", policy =>
        policy.WithOrigins("http://localhost:5173", "http://localhost:4173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/chatHub"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("FrontendCors");
app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapHub<ChatHub>("/chatHub");
app.MapControllers();

app.Run();
