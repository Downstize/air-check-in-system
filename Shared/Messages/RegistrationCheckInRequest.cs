namespace Shared.Messages;

public class RegistrationCheckInRequest
{
    public string DynamicId { get; set; }
    public string LastName { get; set; } = default!;
    public string Pnr { get; set; } = default!;
    public bool PaidSeat { get; set; }
}