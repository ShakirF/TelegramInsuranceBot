using Application.Interfaces;
using Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Shared.Constants;
using Document = QuestPDF.Fluent.Document;

namespace Infrastructure.Policy
{
    public class QuestPdfGenerator : IPdfGenerator
    {
        public Task<byte[]> GeneratePolicyPdf(User user, string summary, string gptText, decimal price)
        {
            var now = DateTime.UtcNow;
            var expiry = now.AddDays(PolicyConstants.PolicyValidityDays);

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);
                    page.Header().Text("Insurance Policy").FontSize(20).SemiBold().AlignCenter();

                    page.Content().Column(col =>
                    {
                        col.Item().Text($" Name: {user.FirstName} {user.LastName}");
                        col.Item().Text($" Issued: {now:yyyy-MM-dd}   |   Expiry: {expiry:yyyy-MM-dd}");
                        col.Item().PaddingBottom(10).Text($" Price: {price} USD");
                        col.Item().LineHorizontal(1);
                        col.Item().Text(" Summary").SemiBold();
                        col.Item().Text(summary).Italic();
                        col.Item().PaddingTop(10).Text(" Message").SemiBold();
                        col.Item().Text(gptText);
                    });

                    page.Footer().AlignCenter().Text("Car Insurance Bot – Policy Document");
                });
            });

            using var ms = new MemoryStream();
            doc.GeneratePdf(ms);
            return Task.FromResult(ms.ToArray());
        }
    }
}
