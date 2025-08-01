using IgniteLifeApi.Dtos;
using Microsoft.AspNetCore.Mvc;
using Server.Dtos;
using Server.Services;

namespace IgniteLifeApi.Controllers;

[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
[Route("api/[controller]")]
public class BookingRuleOpeningExceptionsController(BookingRuleOpeningExceptionService service) : ControllerBase
{
    private readonly BookingRuleOpeningExceptionService _service = service;

    [HttpPost]
    public async Task<ActionResult<ApiResult<ReadBookingRulesResponse>>> PostBookingRuleOpeningException(CreateBookingRuleOpeningExceptionRequest request)
    {
        var result = await _service.CreateOpeningExceptionAsync(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<ApiResult<ReadBookingRulesResponse>>> UpdateBookingRuleOpeningException(
        [FromRoute] Guid id,
        [FromBody] UpdateBookingRuleOpeningExceptionRequest request)
    {
        var result = await _service.UpdateOpeningExceptionAsync(id, request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResult<ReadBookingRulesResponse>>> DeleteBookingRuleOpeningException(
        [FromRoute] Guid id)
    {
        var result = await _service.DeleteOpeningExceptionAsync(id);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }
}
