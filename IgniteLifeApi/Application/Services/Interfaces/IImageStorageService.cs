namespace IgniteLifeApi.Application.Services.Interfaces
{
    public interface IImageStorageService
    {
        Task<string> UploadAsync(IFormFile file, string folder, CancellationToken ct = default);
        Task DeleteAsync(string imageUrl, CancellationToken ct = default);
    }
}
