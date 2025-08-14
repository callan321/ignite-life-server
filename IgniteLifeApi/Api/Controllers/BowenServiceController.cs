using IgniteLifeApi.Api.Controllers.Common;
using IgniteLifeApi.Application.Dtos.BowenServices;
using IgniteLifeApi.Application.Services.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IgniteLifeApi.Api.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("api/bowen-services")]
    [Authorize(Policy = "AdminUser")]
    public class BowenServiceController : ControllerBase
    {
        private readonly BowenServiceService _bowenService;

        public BowenServiceController(BowenServiceService bowenService)
        {
            _bowenService = bowenService;
        }

        // GET /api/bowen-services
        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken = default)
        {
            var result = await _bowenService.GetBowenServicesAsync(cancellationToken);
            return ServiceResultToActionResult.ToActionResult(this, result);
        }

        // POST /api/bowen-services
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateBowenServiceDto dto,
            CancellationToken cancellationToken = default)
        {
            var result = await _bowenService.CreateBowenServiceAsync(dto, cancellationToken);
            return ServiceResultToActionResult.ToActionResult(this, result);
        }

        // PATCH /api/bowen-services/{id}
        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> Update(
            [FromRoute] Guid id,
            [FromBody] UpdateBowenServiceDto dto,
            CancellationToken cancellationToken = default)
        {
            var result = await _bowenService.UpdateBowenServiceAsync(id, dto, cancellationToken);
            return ServiceResultToActionResult.ToActionResult(this, result);
        }

        // DELETE /api/bowen-services/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(
            [FromRoute] Guid id,
            CancellationToken cancellationToken = default)
        {
            var result = await _bowenService.DeleteBowenServiceAsync(id, cancellationToken);
            return ServiceResultToActionResult.ToActionResult(this, result);
        }
    }
}
