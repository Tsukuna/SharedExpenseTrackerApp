namespace SharedExpenseTrackerApp.Domain.Models.Auth;

public class LoginReqModel
{
    public string PhoneNumber { get; set; } = null!;
    public string Password { get; set; } = null!;
}
