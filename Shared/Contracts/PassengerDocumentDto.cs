namespace Shared.Contracts;

public class PassengerDocumentDto
{
    public string Type { get; set; }
    public string IssueCountryCode { get; set; }
    public string Number { get; set; }
    public string NationalityCode { get; set; }
    public DateTime BirthDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
}
