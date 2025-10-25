namespace HPMFileStorageService.Services
{
    public interface IMinIOService
    {
        Task<bool> BucketExistsAsync(string bucketName);
        Task CreateBucketAsync(string bucketName);
        Task UploadFileAsync(string bucketName, string objectName, Stream stream, string contentType);
        Task<Stream> DownloadFileAsync(string bucketName, string objectName);
        Task DeleteFileAsync(string bucketName, string objectName);
    }
}
