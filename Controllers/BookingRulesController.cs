using Microsoft.AspNetCore.Mvc;
using Server.Dtos;
using Server.Services;

namespace Server.Controllers;

[Route("api/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
[ApiController]
public class BookingRulesController(BookingRulesService service) : ControllerBase
{
    private readonly BookingRulesService _service = service;

    [HttpGet]
    public async Task<ActionResult<ApiResult<ReadBookingRulesResponse>>> GetBookingRules()
    {
        var result = await _service.GetDefaultBookingRulesAsync();
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    [HttpPatch]
    public async Task<ActionResult<ApiResult<ReadBookingRulesResponse>>> PatchDefaultBookingRules(
        [FromBody] UpdateBookingRulesRequest request)
    {
        var result = await _service.UpdateDefaultBookingRulesAsync(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
