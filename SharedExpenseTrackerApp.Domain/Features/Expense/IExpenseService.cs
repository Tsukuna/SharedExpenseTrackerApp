using SharedExpenseTrackerApp.Domain.Models.Expense;
using SharedExpenseTrackerApp.Domain.Shared;

namespace SharedExpenseTrackerApp.Domain.Features.Expense;

public interface IExpenseService
{
    Task<Result<ExpenseRespModel>> AddExpenseAsync(long listId, long userId, CreateExpenseReqModel request, CancellationToken ct = default);
    Task<Result<List<ExpenseRespModel>>> GetExpensesByListAsync(long listId, long userId, CancellationToken ct = default);
    Task<Result<ExpenseRespModel>> TogglePaidAsync(long expenseId, long userId, TogglePaidReqModel request, CancellationToken ct = default);
    Task<Result<ExpenseSummaryRespModel>> GetUnpaidSummaryAsync(long listId, long userId, CancellationToken ct = default);
}
