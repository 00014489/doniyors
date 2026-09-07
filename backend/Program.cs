using System.Text;
using backend.DAL;
using backend.DAL.Repositories.TravelRepo;
using backend.DAL.Repositories.UserRepo;
using backend.Services.AuthService;
using backend.Services.JwtService;
using backend.Services.TelegramValidator;
using backend.Services.TravelService;
using backend.Services.UserService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);


// Controllers
builder.Services.AddControllers();

// PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Dependency Injection
builder.Services.AddSingleton<ITelegramValidator, TelegramValidator>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITravelService, TravelService>();


builder.Services.AddScoped<IJwt, Jwt>();


builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITravelRepository, TravelRepository>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("Angular", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:4200",      // Angular dev
                "https://your-domain.com",     // Production
                "https://pathwayed-chere-soppily.ngrok-free.dev"   // Ngrok
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// JWT Authentication
var jwt = builder.Configuration.GetSection("Jwt");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwt["Issuer"],
                ValidAudience = jwt["Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwt["Key"]!))
            };
    });

builder.Services.AddAuthorization();

var app = builder.Build();


app.UseHttpsRedirection();

// CORS MUST be before Authentication
app.UseCors("Angular");

// Authentication
app.UseAuthentication();

// Authorization
app.UseAuthorization();

// Controllers
app.MapControllers();

app.Run();