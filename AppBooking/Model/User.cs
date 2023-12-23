namespace AppBooking.Model
{
    // MyAirlineReservation.Api.Models
    public class User
    {
        public int UserId { get; set; }
        public string? Password { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Sex { get; set; }
        public string? Role { get; set; } 
        public int Age { get; set; }
        public int Credit { get; set; }
        public double SkyMiles { get; set; }
        // Các thuộc tính khác nếu cần
    }

}
