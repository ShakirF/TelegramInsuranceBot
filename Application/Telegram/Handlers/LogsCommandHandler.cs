using Application.Interfaces;
using Application.Telegram.Commands;
using Domain.Interfaces;
using Infrastructure.Telegram.Interface;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Application.Telegram.Handlers
{
    public class LogsCommandHandler : IRequestHandler<LogsCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITelegramBotService _bot;
        private readonly IAdminService _admin;

        public LogsCommandHandler(IUnitOfWork unitOfWork, ITelegramBotService bot, IAdminService admin)
        {
            _unitOfWork = unitOfWork;
            _bot = bot;
            _admin = admin;
        }

        public async Task<Unit> Handle(LogsCommand request, CancellationToken cancellationToken)
        {
            if (!_admin.IsAdmin(request.ChatId))
            {
                await _bot.SendTextAsync(request.ChatId, "⛔ Access denied.");
                return Unit.Value;
            }

            var logs = await _unitOfWork.Errors.Query()
                .OrderByDescending(e => e.CreatedAt)
                .Take(10)
                .ToListAsync(cancellationToken);

            if (!logs.Any())
            {
                await _bot.SendTextAsync(request.ChatId, "✅ No error logs found.");
                return Unit.Value;
            }

            var sb = new StringBuilder();
            sb.AppendLine("🛑 <b>Last 10 Error Logs</b>");

            foreach (var log in logs)
            {
                sb.AppendLine($"""
                📅 {log.CreatedAt:u}
                ⚠️ {log.Message}
                🔍 {log.StackTrace?.Substring(0, Math.Min(log.StackTrace.Length, 100))}...
                """);
                sb.AppendLine();
            }

            await _bot.SendTextAsync(request.ChatId, sb.ToString());
            return Unit.Value;
        }
    }

}
