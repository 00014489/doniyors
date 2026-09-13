using System.Text;
using Doniyors.Data;
using backend.DAL.Repositories.MemberRepo;
using backend.DAL.Repositories.TransactionRepo;
using backend.DAL.Repositories.TravelRepo;
using backend.DAL.Repositories.UserRepo;
using backend.Infrastructure;
using backend.Options;
using backend.Services.AuthService;
using backend.Services.JwtService;
using backend.Services.MemberService;
using backend.Services.TelegramMenuButton;
using backend.Services.TelegramValidator;
using backend.Services.TransactionService;
using backend.Services.TravelService;
using backend.Services.UserAdminService;
using backend.Services.UserService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);


// ---------------------------------------------------------------- options
// ValidateOnStart turns a missing or too-short secret into a startup failure
// with a readable message, instead of a 500 at the first request that needs it.
builder.Services
    .AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<TelegramOptions>()
    .Bind(builder.Configuration.GetSection(TelegramOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<CorsOptions>()
    .Bind(builder.Configuration.GetSection(CorsOptions.SectionName));

builder.Services
    .AddOptions<MiniAppOptions>()
    .Bind(builder.Configuration.GetSection(MiniAppOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>() ?? new JwtOptions();

var corsOptions = builder.Configuration
    .GetSection(CorsOptions.SectionName)
    .Get<CorsOptions>() ?? new CorsOptions();


// ------------------------------------------------------------ web basics
builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// nginx terminates TLS, so the scheme and client IP arrive in headers. The
// proxy's address inside the compose network is not fixed, so trust the
// network rather than a pinned address.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});


// ------------------------------------------------------------- database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        // The model lives in the shared Doniyors.Data project, but the schema
        // is owned here: migrations are generated and applied from this
        // project alone, so EF must be told to look for them here rather than
        // beside the DbContext.
        npgsql => npgsql.MigrationsAssembly(typeof(Program).Assembly.FullName)));


// --------------------------------------------------- dependency injection
// Injected rather than calling DateTimeOffset.UtcNow, so "is this adventure
// upcoming?" and the season boundaries are testable.
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddSingleton<ITelegramValidator, TelegramValidator>();
builder.Services.AddSingleton<IJwt, Jwt>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITravelService, TravelService>();
builder.Services.AddScoped<IUserAdminService, UserAdminService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IMemberService, MemberService>();

// Relabels the bot's menu button when the API changes a member's language.
builder.Services.AddHttpClient<ITelegramMenuButton, TelegramMenuButton>(client =>
{
    client.BaseAddress = new Uri("https://api.telegram.org/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITravelRepository, TravelRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IMemberRepository, MemberRepository>();


// ------------------------------------------------------------------ CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("Angular", policy =>
    {
        policy
            .WithOrigins(corsOptions.AllowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


// -------------------------------------------------------- authentication
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Off, so a claim reads back under the name it was written with.
        // With it on, "role" would arrive as the long ClaimTypes.Role URI.
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.Key)),

            NameClaimType = "sub",
            RoleClaimType = ClaimNames.Role,

            // The default five-minute grace makes a 60-minute token live 65.
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

builder.Services
    .AddAuthorizationBuilder()
    .AddPolicy(AuthorizationPolicies.Administrative, policy =>
        policy
            .RequireAuthenticatedUser()
            .RequireRole(RoleNames.Admin, RoleNames.SuperAdmin));


var app = builder.Build();


// ------------------------------------------------------------- pipeline
app.UseForwardedHeaders();

// Writes problem responses for anything that escapes a controller.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// No HTTPS redirection here: nginx terminates TLS and already redirects
// http -> https. Kestrel listens on plain HTTP inside the network, so
// redirecting again would send clients to a port nothing is listening on.

app.UseCors("Angular");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
