using Application.Interfaces;
using Application.Services;
using Domain.Interfaces;
using Infrastructure.Localization;
using Infrastructure.OCR;
using Infrastructure.OpenAI;
using Infrastructure.Policy;
using Infrastructure.Repositories;
using Infrastructure.Storage;
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
            services.AddScoped<IFileStorageService, FileStorageService>();
            services.AddScoped<IUserStateService, UserStateService>();
            services.AddHttpClient<ICustomOcrService, CustomMindeeOcrService>().ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AllowAutoRedirect = false
            });
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IPromptProvider, PromptProvider>();
            services.AddHttpClient<IOpenAIService, OpenAIService>();
            services.AddScoped<IPdfGenerator, QuestPdfGenerator>();
            services.AddScoped<IPolicyBuilder, PdfPolicyBuilder>();
            services.AddScoped<IAdminService, AdminService>();

            return services;
        }
    }
}
