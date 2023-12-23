using AppBooking.Data;
using AppBooking.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppBooking.Controllers
{
    [Route("api/v1/seat")]
    public class SeatController : Controller
    {
        // GET: ReadController
        private readonly AppDbContext _context; // Replace YourDbContext with your actual DbContext

        public SeatController(AppDbContext context)
        {
            _context = context;
        }

        // GET api/v1/seats/{seatNumber}
        [HttpGet("{seatNumber}")]
        public async Task<ActionResult<Seat>> GetSeatBySeatNumber(string seatNumber)
        {
            var seat = await _context.Seats
                .Include(seat => seat.Flight)
                .Where(seat => seat.IsAvailable == false)
                .FirstOrDefaultAsync(s => s.SeatNumber == seatNumber);
            var distance = await _context.Distances.FirstOrDefaultAsync(d => d.DistanceId == seat.Flight.FlightDistanceId);
            if (seat == null)
            {
                return NotFound(); 
            }

            return Ok(new { seat,distance});
        }
    }
}
