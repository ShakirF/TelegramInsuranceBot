using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IPolicyBuilder
    {
        IPolicyBuilder WithUser(User user);
        IPolicyBuilder WithSummary(string extractedData);
        IPolicyBuilder WithGptText(string gptText);
        IPolicyBuilder WithPrice(decimal price);
        Task<byte[]> BuildPdfAsync();
    }
}
