namespace Shared.Contracts;

public class FlightSegmentDto
{
    public string DepartureId { get; set; }
    public string AircompanyCode { get; set; }
    public string FlightNumber { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public string DeparturePortCode { get; set; }
    public string ArrivalPortCode { get; set; }
    public DateTime WebCheckInOpening { get; set; } = DateTime.UtcNow.AddHours(-24);
    public DateTime WebCheckInClosing { get; set; } = DateTime.UtcNow.AddHours(-1);
    public string FlightStatus { get; set; } = "scheduled";
}