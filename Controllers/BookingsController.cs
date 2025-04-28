using Microsoft.AspNetCore.Mvc;
using Server.Services;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BookingsController(BookingService service) : ControllerBase
{
    private readonly BookingService _service = service;




}