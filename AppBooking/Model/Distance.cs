namespace AppBooking.Model
{
    public class Distance
    {
        public int DistanceId { get; set; }
        public string? DepartureCity { get; set; }
        public string? DestinationCity { get; set; }
        public double Miles { get; set; }
    }
}