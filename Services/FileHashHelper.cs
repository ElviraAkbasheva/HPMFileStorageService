using System.Security.Cryptography;

namespace HPMFileStorageService.Services
{
    public static class FileHashHelper
    {
        public static async Task<string> ComputeSha256HashAsync(Stream stream)
        {
            stream.Position = 0;
            using var sha256 = SHA256.Create();
            var hashBytes = await sha256.ComputeHashAsync(stream);
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }
    }
}
