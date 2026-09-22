using SharedExpenseTrackerApp.Domain.Models.Expense;

namespace SharedExpenseTrackerApp.Domain.Models.ExpenseList;

public class ExpenseListRespModel
{
    public long Id { get; set; }
    public long GroupId { get; set; }
    public string Name { get; set; } = null!;
    public bool IsDefault { get; set; }
    public long CreatedUserId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ExpenseListDetailRespModel
{
    public long Id { get; set; }
    public long GroupId { get; set; }
    public string Name { get; set; } = null!;
    public bool IsDefault { get; set; }
    public long CreatedUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ExpenseRespModel> Expenses { get; set; } = new();
}
