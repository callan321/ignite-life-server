using IgniteLifeApi.Api.Controllers.Common;
using IgniteLifeApi.Application.Dtos.BookingRuleBlockedPeriod;
using IgniteLifeApi.Application.Dtos.BookingRules;
using IgniteLifeApi.Application.Services.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
[Route("api/booking-rules")]
[Authorize(Policy = "AdminUser")]
public class BookingRulesController : ControllerBase
{
    private readonly BookingRuleService _bookingRuleService;

    public BookingRulesController(BookingRuleService bookingRuleService)
    {
        _bookingRuleService = bookingRuleService;
    }

    // --- Singleton rules ---
    // GET /api/booking-rules
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken = default)
    {
        var result = await _bookingRuleService.GetBookingRulesAsync(cancellationToken);
        return ServiceResultToActionResult.ToActionResult(this, result);
    }

    // PATCH /api/booking-rules/{id}
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> UpdateBookingRules(
        [FromRoute] Guid id,
        [FromBody] UpdateBookingRulesDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await _bookingRuleService.UpdateBookingRulesAsync(id, dto, cancellationToken);
        return ServiceResultToActionResult.ToActionResult(this, result);
    }

    // --- Blocked periods ---

    // POST /api/booking-rules/blocked-periods
    [HttpPost("blocked-periods")]
    public async Task<IActionResult> CreateBlockedPeriod(
        [FromBody] CreateBookingRuleBlockedPeriodDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await _bookingRuleService.CreateBlockedPeriodAsync(dto, cancellationToken);
        return ServiceResultToActionResult.ToActionResult(this, result);
    }

    // PATCH /api/booking-rules/blocked-periods/{id}
    [HttpPatch("blocked-periods/{id:guid}")]
    public async Task<IActionResult> UpdateBlockedPeriod(
        [FromRoute] Guid id,
        [FromBody] UpdateBookingRuleBlockedPeriodDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await _bookingRuleService.UpdateBlockedPeriodAsync(id, dto, cancellationToken);
        return ServiceResultToActionResult.ToActionResult(this, result);
    }

    // DELETE /api/booking-rules/blocked-periods/{id}
    [HttpDelete("blocked-periods/{id:guid}")]
    public async Task<IActionResult> DeleteBlockedPeriod(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _bookingRuleService.DeleteBlockedPeriodAsync(id, cancellationToken);
        return ServiceResultToActionResult.ToActionResult(this, result);
    }
}
