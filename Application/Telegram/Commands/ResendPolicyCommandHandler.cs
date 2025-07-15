using Domain.Interfaces;
using Infrastructure.Telegram.Interface;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Telegram.Commands
{
    public class ResendPolicyCommandHandler : IRequestHandler<ResendPolicyCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITelegramBotService _botService;
        private readonly ILogger<ResendPolicyCommandHandler> _logger;

        public ResendPolicyCommandHandler(
            IUnitOfWork unitOfWork,
            ITelegramBotService botService,
            ILogger<ResendPolicyCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _botService = botService;
            _logger = logger;
        }

        public async Task<Unit> Handle(ResendPolicyCommand request, CancellationToken cancellationToken)
        {
            var policy = await _unitOfWork.Policies.Query()
                .Include(p => p.User)
                .Where(p => p.User.TelegramUserId == request.ChatId)
                .OrderByDescending(p => p.IssuedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (policy == null || !File.Exists(policy.FilePath))
            {
                await _botService.SendTextAsync(request.ChatId, "⚠️ No policy found or file is missing.");
                return Unit.Value;
            }

            await _botService.SendDocumentAsync(request.ChatId, policy.FilePath, "📄 Here is your issued policy document.");

            _logger.LogInformation("Resent policy PDF to user {ChatId}.", request.ChatId);

            return Unit.Value;
        }
    }
}
