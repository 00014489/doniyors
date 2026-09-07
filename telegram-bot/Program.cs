using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using telegram_bot.DAL;
using telegram_bot.DAL.Repositories.Sessions;
using telegram_bot.DAL.Repositories.Users;
using telegram_bot.Handlers;
using telegram_bot.Keyboards;
using telegram_bot.Midleware;
using telegram_bot.Models;
using telegram_bot.Services;
using telegram_bot.Services.Localization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.Configure<BotSettings>(
    builder.Configuration.GetSection("BotSettings"));

// Localization
builder.Services.AddSingleton<ILocalizationService, LocalizationService>();


// Telegram client
builder.Services.AddSingleton<ITelegramBotClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<BotSettings>>().Value;
    return new TelegramBotClient(settings.TELEGRAM_BOT_TOKEN);
});

builder.Services.AddDbContext<BotDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ISessionRepo, SessionRepo>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<SessionService>();


builder.Services.AddScoped<UpdateHandler>();
builder.Services.AddScoped<MessageHandler>();
builder.Services.AddScoped<CommandHandler>();
builder.Services.AddScoped<CallbackQueryHandler>();

builder.Services.AddScoped<ReplyKeyboards>();
builder.Services.AddScoped<InlineKeyboards>();



builder.Services.AddControllers();


var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var settings = scope.ServiceProvider.GetRequiredService<IOptions<BotSettings>>().Value;
    var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
    await botClient.SetWebhook(settings.WEBHOOK_BASE_URL);
}

app.UseMiddleware<ErrorHandlingMiddleware>();
app.MapControllers();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}


app.Run();

