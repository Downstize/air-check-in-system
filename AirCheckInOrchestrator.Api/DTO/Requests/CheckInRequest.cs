namespace AirCheckInOrchestrator.Api.DTO.Requests;

public class CheckInRequest
{
    public string DynamicId { get; set; }
    public string LastName { get; set; } = default!;
    public string Pnr      { get; set; } = default!;
    public bool   PaidSeat { get; set; }
}