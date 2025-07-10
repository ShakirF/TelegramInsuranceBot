using Application.Interfaces;
using Application.Telegram.Dispatcher;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationServiceRegistration).Assembly));
            services.AddScoped<IUpdateDispatcher, TelegramUpdateDispatcher>();

            return services;
        }
    }
}
