using HPMFileStorageService.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace HPMFileStorageService.Services
{
    public class MinIOService : IMinIOService
    {
        private readonly IMinioClient _minioClient;
        private readonly ILogger<MinIOService> _logger;

        public MinIOService(IOptions<MinIOSettings> minioOptions, ILogger<MinIOService> logger)
        {
            var settings = minioOptions.Value;
            _minioClient = new MinioClient()
                .WithEndpoint(settings.Endpoint)
                .WithCredentials(settings.AccessKey, settings.SecretKey)
                .WithRegion(settings.Region)
                .Build(); // Возвращает IMinioClient
            _logger = logger;
        }

        public async Task<bool> BucketExistsAsync(string bucketName)
        {
            try
            {
                return await _minioClient.BucketExistsAsync(
                    new BucketExistsArgs().WithBucket(bucketName));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке бакета {Bucket}", bucketName);
                throw;
            }
        }

        public async Task CreateBucketAsync(string bucketName)
        {
            try
            {
                await _minioClient.MakeBucketAsync(
                    new MakeBucketArgs().WithBucket(bucketName));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании бакета {Bucket}", bucketName);
                throw;
            }
        }

        public async Task UploadFileAsync(string bucketName, string objectName, Stream stream, string contentType)
        {
            try
            {
                if (!await BucketExistsAsync(bucketName))
                    await CreateBucketAsync(bucketName);

                stream.Position = 0;
                await _minioClient.PutObjectAsync(
                    new PutObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(objectName)
                        .WithStreamData(stream)
                        .WithObjectSize(stream.Length)
                        .WithContentType(contentType));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки файла {Object} в бакет {Bucket}", objectName, bucketName);
                throw;
            }
        }

        public async Task<Stream> DownloadFileAsync(string bucketName, string objectName)
        {
            try
            {
                var memory = new MemoryStream();
                await _minioClient.GetObjectAsync(
                    new GetObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(objectName)
                        .WithCallbackStream(stream => stream.CopyTo(memory))
                );
                memory.Position = 0;
                return memory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при скачивании файла {Object} из бакета {Bucket}", objectName, bucketName);
                throw;
            }
        }

        public async Task DeleteFileAsync(string bucketName, string objectName)
        {
            try
            {
                await _minioClient.RemoveObjectAsync(
                    new RemoveObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(objectName));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении файла {Object} из бакета {Bucket}", objectName, bucketName);
                throw;
            }
        }
    }
}
