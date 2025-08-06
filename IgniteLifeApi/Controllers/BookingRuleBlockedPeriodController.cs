using IgniteLifeApi.Application.Dtos.BookingRuleBlockedPeriod;
using IgniteLifeApi.Application.Services.Implementations;
using IgniteLifeApi.Controllers.Common;
using Microsoft.AspNetCore.Mvc;

namespace IgniteLifeApi.Controllers;

[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
[Route("api/[controller]")]
public class BookingRuleBlockedPeriodController(BookingRuleBlockedPeriodService service) : ControllerBase
{
    private readonly BookingRuleBlockedPeriodService _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllBookingRuleBlockedPeriodsAsync();
        return ServiceResultToActionResult.ToActionResult(this, result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingRuleBlockedPeriodDto dto)
    {
        var result = await _service.CreateBlockedPeriodAsync(dto);
        return ServiceResultToActionResult.ToActionResult(this, result);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBookingRuleBlockedPeriodDto dto)
    {
        var result = await _service.UpdateBlockedPeriodAsync(id, dto);
        return ServiceResultToActionResult.ToActionResult(this, result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteBlockedPeriodAsync(id);
        return ServiceResultToActionResult.ToActionResult(this, result);
    }
}
