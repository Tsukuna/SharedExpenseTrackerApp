using Microsoft.EntityFrameworkCore;
using SharedExpenseTrackerApp.Database.AppDbContextModels;
using SharedExpenseTrackerApp.Domain.Models.Expense;
using SharedExpenseTrackerApp.Domain.Shared;

namespace SharedExpenseTrackerApp.Domain.Features.Expense;

public class ExpenseService : IExpenseService
{
    private readonly AppDbContext _dbContext;

    public ExpenseService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ExpenseRespModel>> AddExpenseAsync(long listId, long userId, CreateExpenseReqModel request, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Result<ExpenseRespModel>.ValidationError("Expense name is required.");

            if (request.Amount <= 0)
                return Result<ExpenseRespModel>.ValidationError("Amount must be greater than zero.");

            var list = await _dbContext.TblExpenses
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == listId, ct);

            if (list is null)
                return Result<ExpenseRespModel>.NotFound("Expense list not found.");

            var isMember = await _dbContext.TblGroupMembers
                .AsNoTracking()
                .AnyAsync(m => m.GroupId == list.GroupId && m.UserId == userId, ct);

            if (!isMember)
                return Result<ExpenseRespModel>.ValidationError("You are not a member of this group.");

            var expense = new TblExpenseDetail
            {
                ExpenseListId = listId,
                Name = request.Name,
                Amount = request.Amount,
                IsPaid = false,
                Note = request.Note,
                CreatedUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.TblExpenseDetails.Add(expense);
            await _dbContext.SaveChangesAsync(ct);

            return Result<ExpenseRespModel>.Success(MapToRespModel(expense), "Expense added successfully.");
        }
        catch (Exception ex)
        {
            return Result<ExpenseRespModel>.SystemError($"An error occurred: {ex.Message}");
        }
    }

    public async Task<Result<List<ExpenseRespModel>>> GetExpensesByListAsync(long listId, long userId, CancellationToken ct = default)
    {
        try
        {
            var list = await _dbContext.TblExpenses
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == listId, ct);

            if (list is null)
                return Result<List<ExpenseRespModel>>.NotFound("Expense list not found.");

            var isMember = await _dbContext.TblGroupMembers
                .AsNoTracking()
                .AnyAsync(m => m.GroupId == list.GroupId && m.UserId == userId, ct);

            if (!isMember)
                return Result<List<ExpenseRespModel>>.ValidationError("You are not a member of this group.");

            var expenses = await _dbContext.TblExpenseDetails
                .AsNoTracking()
                .Where(e => e.ExpenseListId == listId)
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => new ExpenseRespModel
                {
                    Id = e.Id,
                    ExpenseListId = e.ExpenseListId,
                    Name = e.Name,
                    Amount = e.Amount,
                    IsPaid = e.IsPaid,
                    Note = e.Note,
                    CreatedUserId = e.CreatedUserId,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt
                })
                .ToListAsync(ct);

            return Result<List<ExpenseRespModel>>.Success(expenses);
        }
        catch (Exception ex)
        {
            return Result<List<ExpenseRespModel>>.SystemError($"An error occurred: {ex.Message}");
        }
    }

    public async Task<Result<ExpenseRespModel>> TogglePaidAsync(long expenseId, long userId, TogglePaidReqModel request, CancellationToken ct = default)
    {
        try
        {
            var expense = await _dbContext.TblExpenseDetails
                .Include(e => e.ExpenseList)
                .FirstOrDefaultAsync(e => e.Id == expenseId, ct);

            if (expense is null)
                return Result<ExpenseRespModel>.NotFound("Expense not found.");

            var isMember = await _dbContext.TblGroupMembers
                .AsNoTracking()
                .AnyAsync(m => m.GroupId == expense.ExpenseList.GroupId && m.UserId == userId, ct);

            if (!isMember)
                return Result<ExpenseRespModel>.ValidationError("You are not a member of this group.");

            expense.IsPaid = request.IsPaid;
            expense.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(ct);

            return Result<ExpenseRespModel>.Success(MapToRespModel(expense),
                request.IsPaid ? "Expense marked as paid." : "Expense marked as unpaid.");
        }
        catch (Exception ex)
        {
            return Result<ExpenseRespModel>.SystemError($"An error occurred: {ex.Message}");
        }
    }

    public async Task<Result<ExpenseSummaryRespModel>> GetUnpaidSummaryAsync(long listId, long userId, CancellationToken ct = default)
    {
        try
        {
            var list = await _dbContext.TblExpenses
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == listId, ct);

            if (list is null)
                return Result<ExpenseSummaryRespModel>.NotFound("Expense list not found.");

            var isMember = await _dbContext.TblGroupMembers
                .AsNoTracking()
                .AnyAsync(m => m.GroupId == list.GroupId && m.UserId == userId, ct);

            if (!isMember)
                return Result<ExpenseSummaryRespModel>.ValidationError("You are not a member of this group.");

            var unpaid = await _dbContext.TblExpenseDetails
                .AsNoTracking()
                .Where(e => e.ExpenseListId == listId && !e.IsPaid)
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => new ExpenseRespModel
                {
                    Id = e.Id,
                    ExpenseListId = e.ExpenseListId,
                    Name = e.Name,
                    Amount = e.Amount,
                    IsPaid = e.IsPaid,
                    Note = e.Note,
                    CreatedUserId = e.CreatedUserId,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt
                })
                .ToListAsync(ct);

            var summary = new ExpenseSummaryRespModel
            {
                TotalUnpaidCount = unpaid.Count,
                TotalUnpaidAmount = unpaid.Sum(e => e.Amount),
                UnpaidExpenses = unpaid
            };

            return Result<ExpenseSummaryRespModel>.Success(summary);
        }
        catch (Exception ex)
        {
            return Result<ExpenseSummaryRespModel>.SystemError($"An error occurred: {ex.Message}");
        }
    }

    private static ExpenseRespModel MapToRespModel(TblExpenseDetail e) => new()
    {
        Id = e.Id,
        ExpenseListId = e.ExpenseListId,
        Name = e.Name,
        Amount = e.Amount,
        IsPaid = e.IsPaid,
        Note = e.Note,
        CreatedUserId = e.CreatedUserId,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };
}
