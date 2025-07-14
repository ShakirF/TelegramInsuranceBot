using Domain.Entities;

namespace Application.Interfaces
{
    public interface IPdfGenerator
    {
        Task<byte[]> GeneratePolicyPdf(User user, string summary, string gptText, decimal price);
    }
}
