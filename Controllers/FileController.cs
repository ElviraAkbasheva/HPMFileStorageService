using HPMFileStorageService.Data;
using HPMFileStorageService.Models;
using HPMFileStorageService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HPMFileStorageService.Controllers
{
    [Route("api/files")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IMinIOService _minioService;
        private readonly long _maxFileSizeBytes;
        public FileController(ApplicationDBContext context, IMinIOService minioService, IOptions<FileUploadSettings> fileUploadSettings)
        {
           _context = context;
           _minioService = minioService;
           _maxFileSizeBytes = fileUploadSettings.Value.MaxFileSizeMB * 1024L * 1024L; // МБ переводим в байты
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Файл обязателен и не может быть пустым.");

            if (file.Length > _maxFileSizeBytes)
                return BadRequest($"Размер файла превышает допустимый лимит в {_maxFileSizeBytes / (1024 * 1024)} МБ.");

            try
            {
                string bucketName = FileBucketResolver.GetBucketForFile(file.FileName);
                string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";

                using var stream = file.OpenReadStream();
                await _minioService.UploadFileAsync(bucketName, uniqueFileName, stream, file.ContentType);

                var fileMetadata = new FileMetadata
                {
                    FileName = uniqueFileName,
                    OriginalFileName = file.FileName,
                    BucketName = bucketName,
                    FileSize = file.Length,
                    UploadDate = DateTime.UtcNow,
                    ContentType = file.ContentType
                };

                _context.Files.Add(fileMetadata);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Id = fileMetadata.Id,
                    Bucket = bucketName,
                    Message = "Файл успешно загружен",
                    OriginalFileName = fileMetadata.OriginalFileName
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest($"Недопустимый тип файла: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при загрузке файла: {ex.Message}");
            }
        }

        // Эндпоинт для получения метаданных файла
        [HttpGet("{id}")]
        public async Task<ActionResult<FileMetadata>> GetFileMetadata(int id)
        {
            var fileMetadata = await _context.Files.FindAsync(id);

            if (fileMetadata == null)
            {
                return NotFound($"Метаданные файла с ID {id} не найдены.");
            }

            return fileMetadata;
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadFile(int id)
        {
            // Найти метаданные
            var metadata = await _context.Files.FindAsync(id);
            if (metadata == null)
            {
                return NotFound($"Файл с ID {id} не найден в базе данных.");
            }

            try
            {
                // Скачать файл из MinIO
                var fileStream = await _minioService.DownloadFileAsync(metadata.BucketName, metadata.FileName);

                // Вернуть файл клиенту
                return File(
                    fileStream,
                    contentType: metadata.ContentType,
                    fileDownloadName: metadata.OriginalFileName // это имя будет в "Сохранить как..."
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при получении файла из хранилища: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFile(int id)
        {
            // Найти метаданные
            var metadata = await _context.Files.FindAsync(id);
            if (metadata == null)
            {
                return NotFound($"Файл с ID {id} не найден.");
            }

            try
            {
                // Удалить из MinIO
                await _minioService.DeleteFileAsync(metadata.BucketName, metadata.FileName);

                // Удалить из БД
                _context.Files.Remove(metadata);
                await _context.SaveChangesAsync();


                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при удалении файла: {ex.Message}");
            }
        }
    }
}
