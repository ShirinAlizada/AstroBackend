using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace AstroBackend.Infrastructure.Services
{
    /// <summary>
    /// IFileStorageService-in yerli diskə (wwwroot/uploads) yazan tətbiqi. Bulud
    /// provayderinə (Azure Blob, S3 və s.) keçiddə yalnız bu sinif əvəzlənəcək —
    /// Application qatındakı IFileStorageService interfeysi dəyişmir.
    ///
    /// IWebHostEnvironment-ə birbaşa asılılıq yaratmamaq üçün (Infrastructure
    /// ASP.NET Core hosting abstraksiyalarına istinad etmir) contentRootPath sadə
    /// bir string olaraq Program.cs-dən ServiceRegistration vasitəsilə ötürülür.
    /// </summary>
    public class LocalFileStorageService : IFileStorageService
    {
        private static readonly HashSet<string> AllowedAvatarExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp", ".gif"
        };

        private readonly string _contentRootPath;

        public LocalFileStorageService(string contentRootPath)
        {
            _contentRootPath = contentRootPath;
        }

        public async Task<string> SaveAvatarAsync(Guid userId, Stream content, string fileExtension, CancellationToken ct = default)
        {
            var extension = fileExtension.StartsWith('.') ? fileExtension : $".{fileExtension}";
            if (!AllowedAvatarExtensions.Contains(extension))
            {
                throw new BadRequestException("Dəstəklənməyən şəkil formatı. İcazə verilən formatlar: jpg, jpeg, png, webp, gif.");
            }
            var fileName = $"{userId}{extension}";

            var uploadsDir = Path.Combine(_contentRootPath, "wwwroot", "uploads", "avatars");
            Directory.CreateDirectory(uploadsDir);

            var filePath = Path.Combine(uploadsDir, fileName);

            await using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                content.Position = 0;
                await content.CopyToAsync(fileStream, ct);
            }

            return $"/uploads/avatars/{fileName}";
        }
    }

}
