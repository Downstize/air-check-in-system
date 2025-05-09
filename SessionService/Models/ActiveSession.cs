namespace SessionService.Models;

public class ActiveSession
{
    public int Id { get; set; }
    public string DynamicId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
