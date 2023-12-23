using AppBooking.Data;
using AppBooking.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppBooking.Controllers
{
    [ApiController]
    [Route("api/v1/distances")]
    public class DistanceController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DistanceController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Distance>>> GetFlights()
        {
            return await _context.Distances.ToListAsync();
        }
        [HttpGet("departure-cities")]
        public IActionResult GetDepartureCities()
        {
            var departureCities = _context.Distances.Select(d => d.DepartureCity).Distinct().ToList();
            return StatusCode(200, new { cities = departureCities });
        }
        [HttpGet("get-distance")]
        public IActionResult GetDistance([FromQuery] string departureCity, [FromQuery] string destinationCity)
        {
            var distance = _context.Distances
                .FirstOrDefault(d => d.DepartureCity == departureCity && d.DestinationCity == destinationCity);
            if (distance == null)
            {
                return NotFound("Distance not found for the specified cities.");
            }
            return Ok(distance);
        }
    }
}
