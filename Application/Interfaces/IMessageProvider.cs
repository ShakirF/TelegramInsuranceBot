using Domain.Enums;

namespace Application.Interfaces
{
    public interface IMessageProvider
    {
        string GetStartMessage(string? firstName);
        string GetDocumentSavedMessage(string fileName);
        string GetUnexpectedDocumentMessage();
        string GetOcrDoneMessage();
        string GetNoExtractedFieldsMessage();
        string GetStepMismatchMessage(UserStep step);
        string GetConfirmationPrompt();
        string GetConfirmationThankYouMessage();
        string GetUnknownCommandMessage();
        string GetPassportPromptMessage();
        string GetCarDocPromptMessage(string fileName);
        string GetOcrErrorMessage();
        string GetUnexpectedErrorMessage();
        string GetNoFieldsExtractedMessage();
        string GetPriceQuoteMessage(string price);
        string GetPolicyConfirmedMessage();
        string GetPolicyCancelMessage();
        string GetPolicyFixPriceMessage();
    }
}
