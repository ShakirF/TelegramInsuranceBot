using System.Security.Cryptography;

namespace Shared.Utilities
{
    public static class FileHashHelper
    {
        public static string ComputeSHA256(Stream stream)
        {
            using var sha = SHA256.Create();
            stream.Position = 0;
            var hash = sha.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
