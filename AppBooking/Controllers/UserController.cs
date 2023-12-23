using AppBooking.Data;
using AppBooking.Model;
using AppBooking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace AppBooking.Controllers
{
    [Authorize]
    [Route("api/v1/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public UserController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<User> users = await _context.Users.ToListAsync();
            return StatusCode(200,users);
        }
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {

            var token = HttpContext.Request.Headers["Authorization"].ToString().Split(" ")[1];
            TokenService tokenService = new TokenService(_configuration,_context);
            int userId = int.Parse(tokenService.GetUserIdFromToken(token));
            User? user = await _context.Users.FirstOrDefaultAsync(m => m.UserId == userId);
            return StatusCode(200, user);
        }
        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            User? user = await _context.Users.FirstOrDefaultAsync(m => m.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] User user)
        {
            if (ModelState.IsValid)
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                _context.Add(user);
                await _context.SaveChangesAsync();
                return Ok(user);
            }

            return BadRequest(ModelState);
        }

        [HttpPut("edit/{id}")]
        public async Task<IActionResult> Edit(int id, [FromBody] User user)
        {
            if (id != user.UserId)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return Ok(user);
            }

            return BadRequest(ModelState);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            User? user = await _context.Users.FirstOrDefaultAsync(m => m.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }
        [HttpPost("book")]
        public async Task<ActionResult<Booking>> BookFlight([FromBody] Booking request)
        {
            if (ModelState.IsValid)
            {

                var token = HttpContext.Request.Headers["Authorization"].ToString().Split(" ")[1];
                TokenService tokenService = new TokenService(_configuration,_context);
                User user = await tokenService.GetUserAsync(token);
                if (user == null)
                {
                   return BadRequest("Token expires ");
                }

                var booking = new Booking
                {
                    UserId = user.UserId,
                    FlightId = request.FlightId,
                    SeatId = request.SeatId,
                    BaggageWeight = request.BaggageWeight,
                    Price = CalculateTotalPrice(request.FlightId, request.SeatId, request.BaggageWeight),
                    Status = "WAITING",
                    User = user,
                    Flight = await _context.Flights.FindAsync(request.FlightId),
                    Seat = await _context.Seats.FindAsync(request.SeatId),
                };
                _context.Bookings.Add(booking);
                _context.SaveChanges();

                return Ok(new { booking });
            }

            return BadRequest(ModelState);
        }

        [HttpDelete("cancel-booking/{bookingId}")]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            try
            {

                // 1. Kiểm tra xem có thể hủy vé hay không
                var bookingToCancel = await _context.Bookings.FindAsync(bookingId);

                if (bookingToCancel == null)
                {
                    return NotFound("Booking not found.");
                }
                var seat = await _context.Seats.FirstAsync(x => x.SeatId == bookingToCancel.SeatId);
                seat.IsAvailable = true;
                _context.Bookings.Remove(bookingToCancel);
                await _context.SaveChangesAsync();

                return Ok(new { mess = "Booking cancelled successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpPut("payment/{bookingId}")]
        public async Task<IActionResult> MakePayment(int bookingId)
        {
            try
            {
                var bookingToPay = await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Seat) 
                    .Include(b => b.Flight)
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId);
                if (bookingToPay == null)
                {
                    return NotFound("Booking not found.");
                }
                var distance = await _context.Distances.FirstOrDefaultAsync(b => b.DistanceId == bookingToPay.Flight.FlightDistanceId);
                bookingToPay.User.SkyMiles += distance.Miles;
                bookingToPay.Status = "DONE"; // (hoặc trạng thái bạn muốn)
                bookingToPay.Seat.IsAvailable = false;
                await _context.SaveChangesAsync();
                return Ok(new {mess =  "Payment successful." });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPost("reschedule/{bookingId}")]
        public async Task<IActionResult> RescheduleFlight(int bookingId)
        {
            try
            {
                // Xử lý logic thay đổi thời gian đi ở đây
                var bookingToReschedule = await _context.Bookings.FindAsync(bookingId);

                if (bookingToReschedule == null)
                {
                    return NotFound("Booking not found.");
                }

                // Kiểm tra xem có thể thay đổi thời gian đi hay không
                var today = DateTime.UtcNow.Date;
                var daysUntilDeparture = (bookingToReschedule.Flight.DepartureTime.Date - today).Days;

                if (daysUntilDeparture <= 5)
                {
                    var replacementFlights = await _context.Flights
                        .Where(f => f.FlightDistance.DepartureCity == bookingToReschedule.Flight.FlightDistance.DepartureCity
                                    && f.FlightDistance.DestinationCity == bookingToReschedule.Flight.FlightDistance.DestinationCity
                                    && f.FlightId != bookingToReschedule.FlightId
                                    && f.DepartureTime > today
                                    && f.Seats.Any(s => s.IsAvailable))
                        .ToListAsync();

                    if (replacementFlights != null && replacementFlights.Count > 0)
                    {
                        return Ok(replacementFlights);
                    }
                }
                return BadRequest("Cannot reschedule the flight.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpGet("FlightsByUser")]
        public async Task<ActionResult<IEnumerable<Booking>>> GetFlightsByUser()
        {

            var token = HttpContext.Request.Headers["Authorization"].ToString().Split(" ")[1];
            TokenService tokenService = new TokenService(_configuration, _context);
            User user = await tokenService.GetUserAsync(token);
            var booking = await _context.Bookings
                .Where(b => b.UserId == user.UserId)
                .Include(b =>b.Seat)
                .ToArrayAsync();
            if (booking == null )
            {
                return NotFound("No flights found for the user.");
            }

            return Ok(booking);
        }
        [HttpPut("reschedule/{bookingId}")]
        public async Task<IActionResult> RescheduleBooking(int bookingId, [FromBody] RescheduleRequest rescheduleRequest)
        {
            try
            {
                var bookingToReschedule = await _context.Bookings.FindAsync(bookingId);
                if (bookingToReschedule == null)
                {
                    return NotFound("Booking not found.");
                }
                bookingToReschedule.FlightId = rescheduleRequest.NewFlightId;
                bookingToReschedule.SeatId = rescheduleRequest.NewSeatId;
                _context.Update(bookingToReschedule);
                await _context.SaveChangesAsync();
                return Ok("Booking rescheduled successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
        public class BookingRequest
        {
            public int Id { get; set; }
        }
        public class RescheduleRequest
        {
            public int NewFlightId { get; set; }
            public int NewSeatId { get; set; }

        }
        private double CalculateTotalPrice(int flightId, int seatId, int baggageWeight)
        {
            var basePrice = 100.0; // Giá vé cơ bản
            var baggageFee = 2.0; // Phí hành lý (mỗi kg)
            var seatPrice = _context.Seats
                .Where(s => s.FlightId == flightId && s.SeatId == seatId)
                .Select(s => s.Price)
                .FirstOrDefault();
            if (seatPrice == 0)
            {
                seatPrice = 100; // Giá ghế mặc định
            }
            var totalSeatPrice = seatPrice; // Giả sử là giá ghế đã tìm được hoặc giá ghế mặc định
            return basePrice + totalSeatPrice + (baggageWeight * baggageFee);
        }
        private string GenerateToken(int bookingId)
        {
            var token = Guid.NewGuid().ToString();
            var booking = _context.Bookings.Find(bookingId);
            _context.SaveChanges();
            return token;
        }

        internal void CheckAndDeleteExpiredBookings()
        {
            throw new NotImplementedException();
        }
    }
}
