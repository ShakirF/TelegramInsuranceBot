using Infrastructure.Telegram.Interface;
using Infrastructure.Telegram.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

namespace Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            var botToken = configuration["BOT_TOKEN"] ?? throw new InvalidOperationException("BOT_TOKEN is missing");

            services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(botToken));

            services.AddSingleton<ITelegramBotService, TelegramBotService>();
            return services;
        }
    }
}
