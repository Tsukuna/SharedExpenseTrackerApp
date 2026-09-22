namespace SharedExpenseTrackerApp.Domain.Models.Auth;

public class UserProfileRespModel
{
    public long Id { get; set; }
    public string FullName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
