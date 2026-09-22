using SharedExpenseTrackerApp.Domain.Models.ExpenseList;
using SharedExpenseTrackerApp.Domain.Shared;

namespace SharedExpenseTrackerApp.Domain.Features.ExpenseList;

public interface IExpenseListService
{
    Task<Result<ExpenseListRespModel>> CreateExpenseListAsync(long groupId, long userId, CreateExpenseListReqModel request, CancellationToken ct = default);
    Task<Result<List<ExpenseListRespModel>>> GetGroupListsAsync(long groupId, long userId, CancellationToken ct = default);
    Task<Result<ExpenseListDetailRespModel>> GetExpenseListByIdAsync(long listId, long userId, CancellationToken ct = default);
    Task<Result<ExpenseListRespModel>> UpdateExpenseListAsync(long listId, long userId, UpdateExpenseListReqModel request, CancellationToken ct = default);
    Task<Result<string>> DeleteExpenseListAsync(long listId, long userId, CancellationToken ct = default);
}
