namespace PassengerService.Models;

public class Booking
{
    public int BookingId { get; set; }
    public string Pnr { get; set; }
    public int FlightId { get; set; }
    public Flight Flight { get; set; }
    public decimal LuggageWeight { get; set; } = 20m;
    public bool PaidCheckin { get; set; } = false;
    public ICollection<Passenger> Passengers { get; set; } = new List<Passenger>();
}