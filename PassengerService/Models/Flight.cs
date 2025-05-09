namespace PassengerService.Models;

public class Flight
{
    public int FlightId { get; set; }
    public string FlightNumber { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public string AircompanyCode { get; set; }
    public string DeparturePortCode { get; set; }
    public string ArrivalPortCode { get; set; }
    public string FlightStatus { get; set; }
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}