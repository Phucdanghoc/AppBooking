using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AppBooking.Model
{
    public class Booking
    {
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public int FlightId { get; set; }
        public int SeatId { get; set; }
        public int BaggageWeight { get; set; }
        public double Price { get; set; }
        public string? Status { get; set; }

        [JsonIgnore]
        public User? User { get; set; }

        [JsonIgnore]
        public Flight? Flight { get; set; }

        [JsonIgnore]
        public Seat? Seat { get; set; }

        [NotMapped]
        public Flight FlightDetail
        {
            get => Flight ?? new Flight();
            set => Flight = value;
        }

        [NotMapped]
        public Seat SeatDetail
        {
            get => Seat ?? new Seat();
            set => Seat = value;
        }
    }
}
