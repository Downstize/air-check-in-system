namespace PassengerService.Models;

public class Passenger
{
    public string PassengerId { get; set; }
    public string PnrId { get; set; }
    public int PaxNo { get; set; }
    public string LastName { get; set; }
    public string FirstName { get; set; }
    public DateTime BirthDate { get; set; }
    public string Category { get; set; }
    public string CheckInStatus { get; set; }
    public string? Reason { get; set; }
    public int SeatsOccupied { get; set; }
    public bool Eticket { get; set; }
    public PassengerDocument Document { get; set; }
    public PassengerVisaDocument VisaDocument { get; set; }
    public string SeatNumber { get; set; }
    public string SeatStatus { get; set; }
    public string SeatLayerType { get; set; }
    public List<string> Remarks { get; set; } = new();
    public int Apis { get; set; }
    public int BookingId { get; set; }
    public Booking Booking { get; set; }
}