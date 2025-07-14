using Application.Interfaces;
using Application.Telegram.Commands;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Storage;
using Infrastructure.Telegram.Interface;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Constants;

namespace Application.Telegram.Handlers
{
    public class GeneratePolicyCommandHandler : IRequestHandler<GeneratePolicyCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITelegramBotService _bot;
        private readonly IPolicyBuilder _builder;
        private readonly IOpenAIService _openAi;
        private readonly IFileStorageService _storage;
        private readonly IPromptProvider _promptProvider;

        public GeneratePolicyCommandHandler(IUnitOfWork unitOfWork, ITelegramBotService bot, IPolicyBuilder builder, IOpenAIService openAi, IFileStorageService storage, IPromptProvider promptProvider)
        {
            _unitOfWork = unitOfWork;
            _bot = bot;
            _builder = builder;
            _openAi = openAi;
            _storage = storage;
            _promptProvider = promptProvider;
        }

        public async Task<Unit> Handle(GeneratePolicyCommand request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users.Query()
                .FirstOrDefaultAsync(u => u.TelegramUserId == request.ChatId, cancellationToken);

            var extracted = await _unitOfWork.ExtractedFields.Query()
                .Include(f => f.Document)
                    .ThenInclude(d => d.User)
                .Where(f => f.Document.User.TelegramUserId == request.ChatId)
                .ToListAsync(cancellationToken);

            var summary = string.Join("\n", extracted.Select(x => $"{x.FieldName}: {x.FieldValue}"));

            var gptMessage = await _promptProvider.GetGenerateSummaryMessageAsync(summary,cancellationToken);

            var pdfBytes = await _builder
                .WithUser(user!)
                .WithSummary(summary)
                .WithGptText(gptMessage)
                .WithPrice(PolicyConstants.DefaultPolicyPrice)
                .BuildPdfAsync();

            var path = await _storage.SaveAsync(new MemoryStream(pdfBytes), $"{PolicyConstants.PolicyFilePrefix}{request.ChatId}.pdf", request.ChatId.ToString());

            var policy = new Policy
            {
                UserId = user!.Id,
                FilePath = path,
                DocumentSummary = summary,
                GptMessage = gptMessage,
                Price = PolicyConstants.DefaultPolicyPrice,
                IssuedAt = DateTime.UtcNow,
                ExpiryAt = DateTime.UtcNow.AddDays(PolicyConstants.PolicyValidityDays),
                IsSentToUser = true
            };

            await _unitOfWork.Policies.AddAsync(policy, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _bot.SendDocumentAsync(request.ChatId, path, "✅ Here is your insurance policy PDF!");

            return Unit.Value;
        }
    }
}
