using Domain.Enums;

namespace Application.Interfaces
{
    public interface IPromptProvider
    {
        Task<string> GetStartMessageAsync(string? firstName);
        Task<string> GetUnexpectedDocumentMessageAsync();
        Task<string> GetDocumentSavedMessageAsync(string fileName);
        Task<string> GetOcrDoneMessageAsync();
        Task<string> GetNoExtractedFieldsMessageAsync();
        Task<string> GetStepMismatchMessageAsync(UserStep step);
        Task<string> GetConfirmationPromptMessageAsync();
        Task<string> GetConfirmationThankYouMessageAsync();
        Task<string> GetUnknownCommandMessageAsync();
        Task<string> GetPassportPromptMessageAsync();
        Task<string> GetCarDocPromptMessageAsync(string fileName);
        Task<string> GetOcrErrorMessageAsync();
        Task<string> GetUnexpectedErrorMessageAsync();
        Task<string> GetPriceQuoteMessageAsync(string price);
        Task<string> GetPolicyCancelMessageAsync();
        Task<string> GetPolicyConfirmedMessageAsync();
        Task<string> GetPolicyFixPriceMessageAsync();
        Task<string> GetGenerateSummaryMessageAsync();
    }
}
