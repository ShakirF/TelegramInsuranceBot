using Application.Telegram.Commands;
using Domain.Interfaces;
using Infrastructure.Telegram.Interface;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Telegram.Handlers
{
    public class AdminSummaryCommandHandler : IRequestHandler<AdminSummaryCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITelegramBotService _bot;

        public AdminSummaryCommandHandler(IUnitOfWork unitOfWork, ITelegramBotService bot)
        {
            _unitOfWork = unitOfWork;
            _bot = bot;
        }

        public async Task<Unit> Handle(AdminSummaryCommand request, CancellationToken cancellationToken)
        {
            var count = await _unitOfWork.Policies.Query().CountAsync(cancellationToken);
            var total = await _unitOfWork.Policies.Query().SumAsync(p => p.Price, cancellationToken);

            var text = $"📊 <b>Issued Policies Summary</b>\n" +
                       $"• Total Policies: {count}\n" +
                       $"• Total Revenue: ${total}";

            await _bot.SendTextAsync(request.ChatId, text);
            return Unit.Value;
        }
    }
}
