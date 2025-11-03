using Microsoft.AspNetCore.Hosting; // Para IWebHostEnvironment
using System.IO;
using System.Threading.Tasks;

namespace Spendnt.API.Helpers
{
    public class LocalFileStorage : IFileStorage
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LocalFileStorage(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task DeleteFileAsync(string filePath, string containerName)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            var fileName = Path.GetFileName(filePath); 
            var directoryPath = Path.Combine(_env.WebRootPath, containerName);
            var fullPath = Path.Combine(directoryPath, fileName);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        public async Task<string> SaveFileAsync(byte[] content, string extension, string containerName)
        {
            var fileName = $"{Guid.NewGuid()}{extension}";
            var folder = Path.Combine(_env.WebRootPath, containerName); 
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string savingPath = Path.Combine(folder, fileName);
            await File.WriteAllBytesAsync(savingPath, content);

            // Construir la URL relativa para acceder al archivo
            var request = _httpContextAccessor.HttpContext.Request;
            var url = $"{request.Scheme}://{request.Host}{request.PathBase}/{containerName}/{fileName}";
            return url;
        }
    }
}