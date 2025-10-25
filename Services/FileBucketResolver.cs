using System.IO;

namespace HPMFileStorageService.Services
{
    public static class FileBucketResolver
    {
        // Явный список РАЗРЕШЁННЫХ расширений
        private static readonly HashSet<string> _allowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            // Images
            "jpg", "jpeg", "png", "webp", "svg", "gif",

            // Videos
            "mp4", "mpeg", "wmv", "avi", "mov",

            // Documents
            "doc", "docx", "xls", "xlsx", "pdf",

            // Others (безопасные)
            "txt", "csv", "json", "xml", "log", "md"
        };

        public static string GetBucketForFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("Имя файла не может быть пустым.");

            var extension = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();

            if (!_allowedExtensions.Contains(extension))
                throw new ArgumentException($"Тип файла '{extension}' не поддерживается.");

            return extension switch
            {
                "jpg" or "jpeg" or "png" or "webp" or "svg" or "gif" => "images",
                "mp4" or "mpeg" or "wmv" or "avi" or "mov" => "videos",
                "doc" or "docx" or "xls" or "xlsx" or "pdf" => "documents",
                _ => "others"
            };
        }
    }
}