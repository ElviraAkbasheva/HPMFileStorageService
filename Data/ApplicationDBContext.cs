using HPMFileStorageService.Models;
using Microsoft.EntityFrameworkCore;

namespace HPMFileStorageService.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
        {
            
        }

        public DbSet<FileMetadata> Files { get; set; }
    }
}
