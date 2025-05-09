namespace AirCheckInOrchestrator.Api.DTO.Requests;

public class BaggageRequest
{
    public string DynamicId { get; set; }
    public string Pnr         { get; set; } = default!;
    public string PassengerId { get; set; } = default!;
    public int    Pieces      { get; set; }
    public decimal Weight     { get; set; }
}