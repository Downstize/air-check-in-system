namespace Shared.Contracts;

public class PassengerDto
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
    public PassengerDocumentDto? Document { get; set; }
    public PassengerVisaDocumentDto? VisaDocument { get; set; }
    public string SeatNumber { get; set; }
    public string SeatStatus { get; set; }
    public string SeatLayerType { get; set; }
    public List<string>? Remarks { get; set; }
    public int Apis { get; set; }
    public int BookingId { get; set; }
}