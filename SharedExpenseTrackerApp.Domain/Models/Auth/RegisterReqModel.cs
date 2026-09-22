namespace SharedExpenseTrackerApp.Domain.Models.Auth;

public class RegisterReqModel
{
    public string FullName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Password { get; set; } = null!;
}
