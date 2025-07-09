using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.DbContext;


namespace Persistence
{
    public static class PersistenceServiceRegistration
     {
         public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
         {
             services.AddDbContext<AppDbContext>(options =>
                 options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

             return services;
         }
     }
}
