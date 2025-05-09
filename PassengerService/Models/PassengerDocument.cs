namespace PassengerService.Models;

public class PassengerDocument
{
    public string Type { get; set; }
    public string IssueCountryCode { get; set; }
    public string Number { get; set; }
    public string NationalityCode { get; set; }
    public DateTime BirthDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
}