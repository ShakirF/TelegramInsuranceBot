using Application.Interfaces;
using Domain.Enums;

namespace Infrastructure.Localization
{
    public class PromptProvider : IPromptProvider
    {
        private readonly IOpenAIService _openAi;

        public PromptProvider(IOpenAIService openAi)
        {
            _openAi = openAi;
        }

        public Task<string> GetStartMessageAsync(string? firstName) =>
            _openAi.GetBotReplyAsync($"Greet the user named {firstName} and explain how to get insured. Ask to upload passport and car registration document.");

        public Task<string> GetUnexpectedDocumentMessageAsync() =>
            _openAi.GetBotReplyAsync("The user uploaded an unexpected document. Explain that they need to follow the document upload order.");

        public Task<string> GetDocumentSavedMessageAsync(string fileName) =>
            _openAi.GetBotReplyAsync($"Inform user that document '{fileName}' was received and saved successfully.");

        public Task<string> GetOcrDoneMessageAsync() =>
            _openAi.GetBotReplyAsync("Inform user that OCR processing is complete.");

        public Task<string> GetNoExtractedFieldsMessageAsync() =>
            _openAi.GetBotReplyAsync("Inform user that no extracted fields were found in the document.");

        public Task<string> GetStepMismatchMessageAsync(UserStep step) =>
            _openAi.GetBotReplyAsync($"User tried to confirm but is currently in step: {step}. Politely guide them to the correct step.");

        public Task<string> GetConfirmationPromptMessageAsync() =>
            _openAi.GetBotReplyAsync("Ask user to type 'confirm' if data is correct, or re-upload if wrong.");

        public Task<string> GetConfirmationThankYouMessageAsync() =>
            _openAi.GetBotReplyAsync("Thank the user for confirming. Inform them that policy generation will begin.");

        public Task<string> GetUnknownCommandMessageAsync() =>
            _openAi.GetBotReplyAsync("Inform the user that the command is unknown. Ask them to upload document or use /start.");

        public Task<string> GetPassportPromptMessageAsync() =>
            _openAi.GetBotReplyAsync("Acknowledge passport uploaded. Ask user to upload car registration.");

        public Task<string> GetCarDocPromptMessageAsync(string fileName) =>
            _openAi.GetBotReplyAsync($"User uploaded '{fileName}' car document. Confirm OCR done and ask for confirmation.");

        public Task<string> GetOcrErrorMessageAsync() =>
            _openAi.GetBotReplyAsync("Inform user that there was an error contacting OCR service.");

        public Task<string> GetUnexpectedErrorMessageAsync() =>
            _openAi.GetBotReplyAsync("Inform user that an unexpected error occurred. Ask them to try again later.");

        public Task<string> GetPriceQuoteMessageAsync(string price) =>
            _openAi.GetBotReplyAsync($"Inform user that insurance price is {price}. Ask them to confirm or cancel.");

        public Task<string> GetPolicyCancelMessageAsync() =>
            _openAi.GetBotReplyAsync("Acknowledge user cancelled policy generation. Reset the flow.");

        public Task<string> GetPolicyConfirmedMessageAsync() =>
            _openAi.GetBotReplyAsync("Acknowledge user accepted the price. Begin policy generation.");

        public Task<string> GetPolicyFixPriceMessageAsync() =>
            _openAi.GetBotReplyAsync("Inform user that the price is fixed and cannot be changed. Ask again 'confirm' to proceed or 'cancel' again to cancel and start over..");
        public Task<string> GetGenerateSummaryMessageAsync(string summary, CancellationToken cancellationToken) =>
                        _openAi.GetBotReplyAsync($"Generate a summary of the policy with the following details: {summary}. Please ensure it is clear and concise.", cancellationToken);
    }
}