using Application.Interfaces;
using Domain.Enums;

namespace Infrastructure.Localization
{
    public class MessageProvider : IMessageProvider
    {
        public string GetStartMessage(string? firstName) => $"""
        👋 Hello {firstName}, I am here to help you get insured!

        📄 Please upload your passport (photo or PDF).
        📑 Then upload your car registration certificate.

        📌 After both are uploaded, I’ll extract and show you the details for confirmation.
        """;

        public string GetDocumentSavedMessage(string fileName) => $"📎 Document '{fileName}' received and saved.";
        public string GetUnexpectedDocumentMessage() => "⚠️ Unexpected document. Please follow the document upload sequence.";
        public string GetOcrDoneMessage() => "🧠 OCR processing complete.";
        public string GetNoExtractedFieldsMessage() => "⚠️ No extracted fields found.";

        public string GetStepMismatchMessage(UserStep step) => step switch
        {
            UserStep.AwaitingPassport => "⚠️ You're not in the confirmation step. Please upload your passport first.",
            UserStep.AwaitingCarDoc => "⚠️ You're not in the confirmation step. Please upload your car registration document first.",
            _ => "⚠️ You're not in the confirmation step. Please upload your documents first."
        };

        public string GetConfirmationPrompt() =>
            """
        ✅ If everything looks good, type <b>confirm</b>.
        🔁 Otherwise, re-upload the correct document.
        """;

        public string GetConfirmationThankYouMessage() =>
            "✅ Thank you. I will now generate your policy document.";

        public string GetUnknownCommandMessage() =>
            "Unknown command or message. Please send a document or type /start.";

        public string GetPassportPromptMessage() =>
            "✅ Passport received.\n📤 Now please upload your car registration document.";

        public string GetCarDocPromptMessage(string fileName) =>
            $"✅ Car document received.\n📎 Document '{fileName}' saved and OCR processed.";

        public string GetOcrErrorMessage() =>
            "❗ Failed to contact OCR service. Please try again later.";

        public string GetUnexpectedErrorMessage() =>
            "❗ An unexpected error occurred. Please try again later.";

        public string GetNoFieldsExtractedMessage() =>
             "⚠️ No fields extracted.";

        public string GetPriceQuoteMessage(string price) =>
             $"💰 The price for your car insurance is <b>{price}</b>.\n\n" +
             "✅ Type <b>confirm</b> to approve and generate the policy.\n" +
             "❌ Or type <b>cancel</b> to abort.";

        public string GetPolicyConfirmedMessage() =>
            "🎉 Policy confirmed!";

        public string GetPolicyCancelMessage() =>
            "❌ Your request has been cancelled. You can start again by uploading your passport.";

        public string GetPolicyFixPriceMessage() =>
           "⚠️ The price is fixed at $100. You can type 'confirm' to proceed or 'cancel' again to cancel and start over.";
    }
}
