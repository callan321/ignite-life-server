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
    public async Task<IActionResult> Create([FromBody] CreateBookingRuleBlockedPeriodRequest dto)
    {
        var result = await _service.CreateBlockedPeriodAsync(dto);
        return ServiceResultToActionResult.ToActionResult(this, result);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBookingRuleBlockedPeriodRequest dto)
    {
        if (id != dto.Id)
            return BadRequest(new { error = "ID in URL does not match ID in body." });

        var result = await _service.UpdateBlockedPeriodAsync(dto);
        return ServiceResultToActionResult.ToActionResult(this, result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteBlockedPeriodAsync(id);
        return ServiceResultToActionResult.ToActionResult(this, result);
    }
}
