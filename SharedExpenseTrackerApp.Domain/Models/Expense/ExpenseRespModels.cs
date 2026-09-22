namespace SharedExpenseTrackerApp.Domain.Models.Expense;

public class ExpenseRespModel
{
    public long Id { get; set; }
    public long ExpenseListId { get; set; }
    public string Name { get; set; } = null!;
    public int Amount { get; set; }
    public bool IsPaid { get; set; }
    public string? Note { get; set; }
    public long CreatedUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ExpenseSummaryRespModel
{
    public int TotalUnpaidCount { get; set; }
    public int TotalUnpaidAmount { get; set; }
    public List<ExpenseRespModel> UnpaidExpenses { get; set; } = new();
}
