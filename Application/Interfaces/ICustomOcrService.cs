namespace Application.Interfaces
{
    public interface ICustomOcrService
    {
        Task<string> ExtractPassportDataAsync(Stream stream, string fileName, CancellationToken cancellationToken = default);
        Task<string> ExtractVehicleDataAsync(Stream stream, string fileName, CancellationToken cancellationToken = default);
    }
}
