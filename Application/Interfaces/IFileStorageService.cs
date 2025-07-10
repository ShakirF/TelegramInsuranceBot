namespace Infrastructure.Storage
{
    public interface IFileStorageService
    {
        Task<string> SaveAsync(Stream fileStream, string fileName, string subfolder = "");
    }
}
