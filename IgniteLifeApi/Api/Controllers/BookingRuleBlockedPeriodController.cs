using IgniteLifeApi.Api.Controllers.Common;
using IgniteLifeApi.Application.Dtos.BookingRuleBlockedPeriod;
using IgniteLifeApi.Application.Services.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IgniteLifeApi.Api.Controllers;

[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
[Route("api/[controller]")]
[Authorize(Policy = "AdminUser")]
public class BookingRuleBlockedPeriodController : ControllerBase
{
    private readonly BookingRuleBlockedPeriodService _service;

    public BookingRuleBlockedPeriodController(BookingRuleBlockedPeriodService service)
    {
        _service = service;
    }

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
