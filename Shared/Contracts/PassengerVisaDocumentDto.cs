namespace Shared.Contracts;

public class PassengerVisaDocumentDto
{
    public string BirthPlace { get; set; }
    public string Number { get; set; }
    public string IssuePlace { get; set; }
    public DateTime? IssueDate { get; set; }
    public string ApplicCountryCode { get; set; }
}