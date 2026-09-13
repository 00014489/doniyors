using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Doniyors.Data;
using telegram_bot.DAL.Repositories.Sessions;
using telegram_bot.DAL.Repositories.Transactions;
using telegram_bot.DAL.Repositories.Travels;
using telegram_bot.DAL.Repositories.Users;
using telegram_bot.Handlers;
using telegram_bot.Infrastructure;
using telegram_bot.Keyboards;
using telegram_bot.Models;
using telegram_bot.Services;
using telegram_bot.Services.Localization;
using telegram_bot.Services.QrCode;

var builder = WebApplication.CreateBuilder(args);


// ---------------------------------------------------------------- options
// ValidateOnStart turns a missing token or webhook secret into a startup
// failure with a readable message, instead of a bot that silently accepts
// forged updates or cannot talk to Telegram.
builder.Services
    .AddOptions<TelegramOptions>()
    .Bind(builder.Configuration.GetSection(TelegramOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<WebhookOptions>()
    .Bind(builder.Configuration.GetSection(WebhookOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<MiniAppOptions>()
    .Bind(builder.Configuration.GetSection(MiniAppOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();


// ------------------------------------------------------------ web basics
builder.Services.AddControllers();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// nginx terminates TLS, so the scheme and client IP arrive in headers.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});


// ------------------------------------------------------------- Telegram
builder.Services.AddSingleton<ITelegramBotClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<TelegramOptions>>().Value;

    return new TelegramBotClient(options.BotToken);
});

builder.Services.AddHostedService<WebhookRegistration>();


// ------------------------------------------------------------- database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


// --------------------------------------------------- dependency injection
// Injected rather than calling DateTimeOffset.UtcNow directly, so the
// provisioning timestamps are testable — the API registers the same.
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddSingleton<ILocalizationService, LocalizationService>();
builder.Services.AddSingleton<IQrCodeReader, QrCodeReader>();
builder.Services.AddSingleton<MiniAppMenuButton>();

builder.Services.AddScoped<ISessionRepo, SessionRepo>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITravelRepository, TravelRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<QrScanService>();

builder.Services.AddScoped<UpdateHandler>();
builder.Services.AddScoped<MessageHandler>();
builder.Services.AddScoped<CommandHandler>();
builder.Services.AddScoped<CallbackQueryHandler>();
builder.Services.AddScoped<QrScanHandler>();

builder.Services.AddScoped<ReplyKeyboards>();
builder.Services.AddScoped<InlineKeyboards>();


var app = builder.Build();


// ------------------------------------------------------------- pipeline
app.UseForwardedHeaders();

app.UseExceptionHandler();

// No HTTPS redirection: nginx terminates TLS and Kestrel listens on plain
// HTTP inside the compose network. Redirecting would bounce Telegram to a
// port nothing is listening on.

app.MapControllers();

app.Run();
