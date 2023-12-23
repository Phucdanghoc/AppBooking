using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AppBooking.Model
{
    // MyAirlineReservation.Api.Models
    public class Flight
    {
        public int FlightId { get; set; }
        public string? FlightNumber { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string? Airplane { get; set; }

        [ForeignKey("FlightDistanceId")]
        public int? FlightDistanceId { get; set; }

        [JsonIgnore]
        public Distance? FlightDistance { get; set; }
        // Thuộc tính Distance
        [NotMapped]
        public Distance FlightDistanceDetails => FlightDistance ?? new Distance();

        public ICollection<Seat>? Seats { get; set; } = new List<Seat>();
    }
}