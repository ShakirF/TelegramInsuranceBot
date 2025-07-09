using Telegram.Bot;
using Infrastructure.Telegram;

var builder = WebApplication.CreateBuilder(args);

var botToken = Environment.GetEnvironmentVariable("BOT_TOKEN") ?? throw new InvalidOperationException("BOT_TOKEN is missing");

builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(botToken));
builder.Services.AddSingleton<TelegramBotService>();

builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();
app.Run();
