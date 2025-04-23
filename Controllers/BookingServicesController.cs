using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Dtos;
using Server.Models;
using Server.Services;

namespace Server.Controllers
{
    [Route("api/[controller]")]

    [ApiController]
    public class BookingServicesController(ApplicationDbContext context, BookingServicesService service) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;
        private readonly BookingServicesService _service = service;

        // GET: api/BookingServices
        [HttpGet]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<ActionResult<ApiResult<IEnumerable<BookingService>>>> GetBookingServices()
        {
            var result = await _service.GetAllBookingServiceAsync();
            return Ok(result);
        }

        // PUT: api/BookingServices/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBookingService(Guid id, BookingService bookingService)
        {
            if (id != bookingService.Id)
            {
                return BadRequest();
            }

            _context.Entry(bookingService).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookingServiceExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/BookingServices
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<BookingService>> PostBookingService(BookingService bookingService)
        {
            _context.BookingServices.Add(bookingService);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBookingService", new { id = bookingService.Id }, bookingService);
        }

        // DELETE: api/BookingServices/5
        [HttpDelete("{id}")]
        [Produces("application/json")]
        public async Task<ActionResult<ApiResult<IEnumerable<BookingService>>>> DeleteBookingService([FromRoute] Guid id)
        {
            var result = await _service.DeleteBookingServiceAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        private bool BookingServiceExists(Guid id)
        {
            return _context.BookingServices.Any(e => e.Id == id);
        }
    }
}
