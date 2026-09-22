namespace SharedExpenseTrackerApp.Domain.Models.Auth;

public class LoginRespModel
{
    public long Id { get; set; }
    public string FullName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Token { get; set; } = null!;
}
