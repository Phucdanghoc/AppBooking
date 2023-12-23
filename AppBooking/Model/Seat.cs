namespace AppBooking.Model
{
    public class Seat
    {
        public int SeatId { get; set; }
        public string? SeatNumber { get; set; }
        public string? SeatClass { get; set; }
        public bool IsAvailable { get; set; }
        public double Price { get; set; }
        // Khóa ngoại
        public int FlightId { get; set; }
        public Flight? Flight { get; set; }

    }
}
