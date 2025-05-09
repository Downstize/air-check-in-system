namespace PassengerService.Models;

public class PassengerVisaDocument
{
    public string BirthPlace { get; set; }
    public string Number { get; set; }
    public string IssuePlace { get; set; }
    public DateTime? IssueDate { get; set; }
    public string ApplicCountryCode { get; set; }
}