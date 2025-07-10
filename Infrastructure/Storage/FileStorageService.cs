using Microsoft.Extensions.Logging;

namespace Infrastructure.Storage
{
    public class FileStorageService : IFileStorageService
    {
        private readonly ILogger<FileStorageService> _logger;
        private readonly string _basePath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");

        public FileStorageService(ILogger<FileStorageService> logger)
        {
            _logger = logger;
        }

        public async Task<string> SaveAsync(Stream fileStream, string fileName, string subfolder = "")
        {
            try
            {
                var targetFolder = string.IsNullOrWhiteSpace(subfolder)
                    ? _basePath
                    : Path.Combine(_basePath, subfolder);

                Directory.CreateDirectory(targetFolder);

                var filePath = Path.Combine(targetFolder, fileName);

                using var fileStreamOutput = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                await fileStream.CopyToAsync(fileStreamOutput);

                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file");
                throw;
            }
        }
    }
}
