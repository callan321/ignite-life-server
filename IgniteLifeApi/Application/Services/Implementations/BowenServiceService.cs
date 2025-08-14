using IgniteLifeApi.Application.Dtos.BowenServices;
using IgniteLifeApi.Application.Services.Common;
using IgniteLifeApi.Domain.Entities;
using IgniteLifeApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IgniteLifeApi.Application.Services.Implementations
{
    public class BowenServiceService
    {
        private readonly ApplicationDbContext _dbContext;

        public BowenServiceService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // ---------- Public Methods ----------

        public async Task<ServiceResult<List<BowenService>>> GetBowenServicesAsync(CancellationToken cancellationToken = default)
        {
            return await GetAllBowenServicesAsync(cancellationToken);
        }

        public async Task<ServiceResult<List<BowenService>>> CreateBowenServiceAsync(CreateBowenServiceDto dto, CancellationToken cancellationToken = default)
        {
            int? SessionCount = dto.IsMultiSession ? dto.SessionCount : null;
            int? MaxGroupSize = dto.IsGroupSession ? dto.MaxGroupSize : null;

            _dbContext.BowenServices.Add(new BowenService
            {
                Title = dto.Title,
                Price = dto.Price,
                DurationMinutes = dto.DurationMinutes,
                Description = dto.Description,
                IsMultiSession = dto.IsMultiSession,
                SessionCount = SessionCount,
                IsGroupSession = dto.IsGroupSession,
                MaxGroupSize = MaxGroupSize,
                IsActive = dto.IsActive,
                ImageUrl = dto.ImageUrl,
                ImageAltText = dto.ImageAltText
            });

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetAllBowenServicesAsync(cancellationToken);
        }

        public async Task<ServiceResult<List<BowenService>>> UpdateBowenServiceAsync(Guid id, UpdateBowenServiceDto dto, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.BowenServices
                .FindAsync([id], cancellationToken);

            if (entity == null)
                return ServiceResult<List<BowenService>>.NotFound("Bowen service not found.");

            // Pull current values
            var title = dto.Title?.Trim() ?? entity.Title;
            var price = dto.Price ?? entity.Price;
            var duration = dto.DurationMinutes ?? entity.DurationMinutes;
            var description = dto.Description?.Trim() ?? entity.Description;

            var isMulti = dto.IsMultiSession ?? entity.IsMultiSession;
            var isGroup = dto.IsGroupSession ?? entity.IsGroupSession;

            var sessionCount = isMulti ? (dto.SessionCount ?? entity.SessionCount) : null;
            var maxGroupSize = isGroup ? (dto.MaxGroupSize ?? entity.MaxGroupSize) : null;

            var isActive = dto.IsActive ?? entity.IsActive;
            var imageUrl = dto.ImageUrl?.Trim() ?? entity.ImageUrl;
            var imageAltText = dto.ImageAltText?.Trim() ?? entity.ImageAltText;

            if (isMulti)
            {
                if (sessionCount <= 0)
                    return ServiceResult<List<BowenService>>.BadRequest("Session count must be greater than zero for multi-session services.");
            }
            else
            {
                sessionCount = null;
            }
            if (isGroup)
            {
                if (maxGroupSize <= 0)
                    return ServiceResult<List<BowenService>>.BadRequest("Max group size must be greater than zero for group sessions.");
            }
            else
            {
                maxGroupSize = null;
            }

            // Apply updates
            entity.Title = title;
            entity.Price = price;
            entity.DurationMinutes = duration;
            entity.Description = description;

            entity.IsMultiSession = isMulti;
            entity.SessionCount = sessionCount;

            entity.IsGroupSession = isGroup;
            entity.MaxGroupSize = maxGroupSize;

            entity.IsActive = isActive;
            entity.ImageUrl = imageUrl;
            entity.ImageAltText = imageAltText;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetAllBowenServicesAsync(cancellationToken);
        }

        public async Task<ServiceResult<List<BowenService>>> DeleteBowenServiceAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var service = await _dbContext.BowenServices.FindAsync([id], cancellationToken);
            if (service == null)
                return ServiceResult<List<BowenService>>.NotFound("Bowen service not found.");
            _dbContext.BowenServices.Remove(service);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetAllBowenServicesAsync(cancellationToken);
        }

        // ---------- Private Methods ----------

        private async Task<ServiceResult<List<BowenService>>> GetAllBowenServicesAsync(CancellationToken cancellationToken = default)
        {
            var services = await _dbContext.BowenServices
            .AsNoTracking()
            .ToListAsync(cancellationToken);

            return ServiceResult<List<BowenService>>.SuccessResult(services, "Bowen services retrieved successfully.");
        }


    }
}
