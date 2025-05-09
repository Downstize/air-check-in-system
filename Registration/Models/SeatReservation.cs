namespace Registration.Models;

public class SeatReservation
{
    public int SeatReservationId { get; set; }
    public string DynamicId      { get; set; }
    public string DepartureId    { get; set; }
    public string PassengerId    { get; set; }
    public string SeatNumber     { get; set; }
    public DateTime ReservedAt   { get; set; }
}