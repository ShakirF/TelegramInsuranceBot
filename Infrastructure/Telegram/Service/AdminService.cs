using Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Telegram.Service
{
    public class AdminService : IAdminService
    {
        private readonly HashSet<long> _adminIds;

        public AdminService(IConfiguration config)
        {
            var section = config.GetSection("AdminSettings:TelegramIds");
            _adminIds = section.Get<long[]>()?.ToHashSet() ?? new();
        }

        public bool IsAdmin(long telegramUserId) => _adminIds.Contains(telegramUserId);
    }
}
