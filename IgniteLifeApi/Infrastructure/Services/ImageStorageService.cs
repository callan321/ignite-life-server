using IgniteLifeApi.Application.Services.Interfaces;

namespace IgniteLifeApi.Infrastructure.Services
{
    public class ImageStorageService : IImageStorageService
    {
        public Task DeleteAsync(string imageUrl, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<string> UploadAsync(IFormFile file, string folder, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
