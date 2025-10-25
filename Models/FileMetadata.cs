using System.ComponentModel.DataAnnotations;

namespace HPMFileStorageService.Models
{
    public class FileMetadata
    {
        public int Id { get; set; }

        [Required] //Атрибут валидации из System.ComponentModel.DataAnnotations. Он проверяет, что поле не равно null и не является пустой строкой.
        public string FileName { get; set; } = string.Empty;

        [Required]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required]
        public string BucketName { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.Now;

        [Required]
        public string ContentType { get; set; } = string.Empty;
        public string FileHash { get; set; } = string.Empty;
    }
}
