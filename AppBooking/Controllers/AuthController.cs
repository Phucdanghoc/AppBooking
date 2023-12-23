using AppBooking.Data;
using AppBooking.Model;
using AppBooking.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppBooking.Controllers
{
    [Route("api/v1/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserCredentials userCredentials)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == userCredentials.Email);

            if (user != null && BCrypt.Net.BCrypt.Verify(userCredentials.Password, user.Password))
            {
                TokenService tokenService = new TokenService(_configuration,_context);
                string token = tokenService.GenerateToken(user);
                // Return the token in a JSON format
                return Ok(new { token , user.Role});
            }

            // If authentication fails, return an error response
            return Unauthorized(new { message = "Invalid credentials" });
        }
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] User user)
        {
            try
            {
                // Validate the user input (you may want to add more validation)
                if (user == null)
                {
                    return BadRequest("Invalid user data");
                }
                if (ModelState.IsValid)
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                    _context.Add(user);
                    await _context.SaveChangesAsync();
                    return Ok(user);
                }
                else
                {
                    return BadRequest("Invalid user data");
                }
            }
            catch (Exception ex)
            {
                
                return StatusCode(500, "Internal server error" + ex.Message);
            }
        }
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok();
        }
    }

    public class UserCredentials
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}
