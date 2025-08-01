using IgniteLifeApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace IgniteLifeApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BookingsController(BookingService service) : ControllerBase
{
    private readonly BookingService _service = service;

}