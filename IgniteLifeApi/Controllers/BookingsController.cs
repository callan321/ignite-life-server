using Microsoft.AspNetCore.Mvc;
using Server.Services;

namespace IgniteLifeApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BookingsController(BookingService service) : ControllerBase
{
    private readonly BookingService _service = service;




}