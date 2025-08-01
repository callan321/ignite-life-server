using IgniteLifeApi.Data;
using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace IgniteLifeApi.Services;

public class BookingService(ApplicationDbContext context)
{
    private readonly ApplicationDbContext _context = context;
}
