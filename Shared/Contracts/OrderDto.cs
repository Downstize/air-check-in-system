namespace Shared.Contracts;

public class OrderDto
{
    public string OrderId         { get; set; }
    public List<FlightSegmentDto> Segments { get; set; }
    public List<PassengerDto>     Passengers { get; set; }
    public decimal LuggageWeight  { get; set; }
    public bool PaidCheckin       { get; set; }
}