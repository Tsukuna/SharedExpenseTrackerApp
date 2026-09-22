namespace SharedExpenseTrackerApp.Domain.Models.Expense;

public class CreateExpenseReqModel
{
    public string Name { get; set; } = null!;
    public int Amount { get; set; }
    public string? Note { get; set; }
}

public class TogglePaidReqModel
{
    public bool IsPaid { get; set; }
}
