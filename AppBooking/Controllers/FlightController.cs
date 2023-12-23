using AppBooking.Data;
using AppBooking.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AppBooking.Controllers
{
    [Route("api/v1/flights")]
    [ApiController]
    public class FlightController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FlightController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/v1/flights
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Flight>>> GetFlights()
        {
            var flights = await _context.Flights
                        .Include(flight => flight.FlightDistance)
                        .Include(flight => flight.Seats)
                        .ToListAsync();

            return Ok(flights);
        }

        // GET: api/v1/flights/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Flight>> GetFlight(int id)
        {
            var flight = await _context.Flights
                                   .Include(flight => flight.FlightDistance)
                                   .Include(flight => flight.Seats)
                                   .FirstOrDefaultAsync(f => f.FlightId == id);
            if (flight == null)
            {
                return NotFound();
            }

            return Ok(flight);
        }

        // POST: api/v1/flights
        [HttpPost]
        public async Task<ActionResult<Flight>> PostFlight(Flight flight)
        {

            Random random = new Random();
            int randomNumber = random.Next(1, 3); // Change the range based on your requirements
            flight.ArrivalTime = flight.ArrivalTime.AddHours(randomNumber);
            flight.FlightDistance = await _context.Distances.FindAsync(flight.FlightDistanceId);
            flight.FlightNumber = $"VNA{flight.FlightDistance.DepartureCity.Substring(0, 2)}{flight.FlightDistance.DestinationCity.Substring(0, 2)}".ToUpper();
            _context.Flights.Add(flight);
            await _context.SaveChangesAsync();
            var seats = new List<Seat>();
            var businessClassSeats = 20;
            var economyClassSeats = 80;
            for (int i = 1; i <= businessClassSeats; i++)
            {
                string seatNumber = string.Concat("B", flight.FlightDistance.DepartureCity.AsSpan(0, 2), flight.FlightDistance.DestinationCity.AsSpan(0, 2), i.ToString("D3")); // Định dạng số thành chuỗi với ít nhất 3 chữ số
                var seat = new Seat
                {
                    SeatNumber = seatNumber,
                    SeatClass = "Business",
                    IsAvailable = true, // Bạn có thể đặt theo yêu cầu của bạn
                    Price = 500,
                    FlightId = flight.FlightId,
                    Flight = flight
                };

                seats.Add(seat);
            }
            for (int i = 1; i <= economyClassSeats; i++)
            {
                string seatNumber = string.Concat("E", flight.FlightDistance.DepartureCity.AsSpan(0, 2), flight.FlightDistance.DestinationCity.AsSpan(0, 2), i.ToString("D3")); // Định dạng số thành chuỗi với ít nhất 3 chữ số
                var seat = new Seat
                {
                    SeatNumber = seatNumber,
                    SeatClass = "Economy",
                    IsAvailable = true, // Bạn có thể đặt theo yêu cầu của bạn
                    Price = 200,
                    FlightId = flight.FlightId,
                    Flight = flight
                };

                seats.Add(seat);
            }
            _context.Seats.AddRange(seats);
            await _context.SaveChangesAsync();
            var response = new
            {
                flight.FlightId,
                flight.FlightNumber,
                Seats = seats.Select(s => new { s.SeatNumber,s.SeatClass })
            };

            return CreatedAtAction("GetFlight", new { id = flight.FlightId }, response);
        }

        private int GetRandomIndex(int count)
        {
            throw new NotImplementedException();
        }

        // PUT: api/v1/flights/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFlight(int id, Flight flight)
        {
            if (id != flight.FlightId)
            {
                return BadRequest();
            }

            _context.Entry(flight).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FlightExists(id))
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

        // DELETE: api/v1/flights/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFlight(int id)
        {
            var flight = await _context.Flights.FindAsync(id);
            if (flight == null)
            {
                return NotFound();
            }
            _context.Flights.Remove(flight);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/v1/flights/getbydate/{date}
        [HttpGet("getbydate/{date}")]
        public async Task<ActionResult<IEnumerable<Flight>>> GetFlightsByDate(DateTime date)
        {
            
            var flights = await _context.Flights
                .Include(flight => flight.FlightDistance)
                .Include(flight => flight.Seats).
                Where(f => f.DepartureTime.Date == date.Date).ToListAsync();
            if (flights == null || flights.Count == 0)
            {
                return NotFound();
            }
            return flights;
        }

        // GET: api/v1/flights/getbydaterange/{startDate}/{endDate}
        [HttpGet("getbydaterange")]
        public async Task<ActionResult<IEnumerable<Flight>>> GetFlightsByDateRange([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            var flights = await _context.Flights.Include(f => f.FlightDistance)
                .Where(f => f.DepartureTime.Date >= from.Date && f.DepartureTime.Date <= to.Date)
                .ToListAsync();

            if (flights == null || flights.Count == 0)
            {
                return NotFound();
            }
            return flights;
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Flight>>> SearchFlights(
            [FromQuery] int distanceId, 
            [FromQuery] DateTime date
         )
        {
            var flights = await _context.Flights
                .Include(flight => flight.FlightDistance)
                .Include(flight => flight.Seats)
                .Where(f => f.FlightDistanceId == distanceId
                && f.DepartureTime.Date == date.Date
                && date.Date > DateTime.Now
                )
                .ToListAsync();

            if (flights == null || flights.Count == 0)
            {
                return NotFound();
            }
            return flights;
        }

        private bool FlightExists(int id)
        {
            return _context.Flights.Any(e => e.FlightId == id);
        }

    }
}

