using IgniteLifeApi.Dtos;
using Microsoft.AspNetCore.Mvc;
using Server.Dtos;
using Server.Models;
using Server.Services;

namespace IgniteLifeApi.Controllers
{
    [Route("api/[controller]")]

    [ApiController]
    public class BookingServiceTypeController(BookingServiceTypeService service) : ControllerBase
    {
        private readonly BookingServiceTypeService _service = service;

        // GET: api/BookingServices
        [HttpGet]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<ActionResult<ApiResult<IEnumerable<BookingServiceType>>>> GetBookingServices()
        {
            var result = await _service.GetAllBookingServiceAsync();
            return Ok(result);
        }

        // POST: api/BookingServices
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<BookingServiceType>> PostBookingService(CreateBookingServiceTypeRequest request)
        {
            var result = await _service.PostBookingServiceAsync(request);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchBookingService([FromRoute] Guid id, [FromBody] UpdateBookingServiceTypeRequest request)
        {
            var result = await _service.PatchBookingServiceAsync(id, request);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        // DELETE: api/BookingServices/5
        [HttpDelete("{id}")]
        [Produces("application/json")]
        public async Task<ActionResult<ApiResult<IEnumerable<BookingServiceType>>>> DeleteBookingService([FromRoute] Guid id)
        {
            var result = await _service.DeleteBookingServiceAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }
    }
}
