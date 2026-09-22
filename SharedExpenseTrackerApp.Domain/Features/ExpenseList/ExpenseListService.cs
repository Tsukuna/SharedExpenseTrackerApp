using Microsoft.EntityFrameworkCore;
using SharedExpenseTrackerApp.Database.AppDbContextModels;
using SharedExpenseTrackerApp.Domain.Models.Expense;
using SharedExpenseTrackerApp.Domain.Models.ExpenseList;
using SharedExpenseTrackerApp.Domain.Shared;

namespace SharedExpenseTrackerApp.Domain.Features.ExpenseList;

public class ExpenseListService : IExpenseListService
{
    private readonly AppDbContext _dbContext;

    public ExpenseListService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ExpenseListRespModel>> CreateExpenseListAsync(long groupId, long userId, CreateExpenseListReqModel request, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Result<ExpenseListRespModel>.ValidationError("List name is required.");

            var isMember = await _dbContext.TblGroupMembers
                .AsNoTracking()
                .AnyAsync(m => m.GroupId == groupId && m.UserId == userId, ct);

            if (!isMember)
                return Result<ExpenseListRespModel>.ValidationError("You are not a member of this group.");

            var existingCount = await _dbContext.TblExpenses
                .AsNoTracking()
                .CountAsync(e => e.GroupId == groupId, ct);

            var isDefault = existingCount == 0;

            var list = new TblExpense
            {
                GroupId = groupId,
                Name = request.Name,
                IsDefault = isDefault,
                CreatedUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.TblExpenses.Add(list);
            await _dbContext.SaveChangesAsync(ct);

            return Result<ExpenseListRespModel>.Success(new ExpenseListRespModel
            {
                Id = list.Id,
                GroupId = list.GroupId,
                Name = list.Name,
                IsDefault = list.IsDefault,
                CreatedUserId = list.CreatedUserId,
                CreatedAt = list.CreatedAt
            }, "Expense list created successfully.");
        }
        catch (Exception ex)
        {
            return Result<ExpenseListRespModel>.SystemError($"An error occurred: {ex.Message}");
        }
    }

    public async Task<Result<List<ExpenseListRespModel>>> GetGroupListsAsync(long groupId, long userId, CancellationToken ct = default)
    {
        try
        {
            var isMember = await _dbContext.TblGroupMembers
                .AsNoTracking()
                .AnyAsync(m => m.GroupId == groupId && m.UserId == userId, ct);

            if (!isMember)
                return Result<List<ExpenseListRespModel>>.ValidationError("You are not a member of this group.");

            var lists = await _dbContext.TblExpenses
                .AsNoTracking()
                .Where(e => e.GroupId == groupId)
                .OrderByDescending(e => e.IsDefault)
                .ThenBy(e => e.CreatedAt)
                .Select(e => new ExpenseListRespModel
                {
                    Id = e.Id,
                    GroupId = e.GroupId,
                    Name = e.Name,
                    IsDefault = e.IsDefault,
                    CreatedUserId = e.CreatedUserId,
                    CreatedAt = e.CreatedAt
                })
                .ToListAsync(ct);

            return Result<List<ExpenseListRespModel>>.Success(lists);
        }
        catch (Exception ex)
        {
            return Result<List<ExpenseListRespModel>>.SystemError($"An error occurred: {ex.Message}");
        }
    }

    public async Task<Result<ExpenseListDetailRespModel>> GetExpenseListByIdAsync(long listId, long userId, CancellationToken ct = default)
    {
        try
        {
            var list = await _dbContext.TblExpenses
                .AsNoTracking()
                .Include(e => e.TblExpenseDetails)
                .FirstOrDefaultAsync(e => e.Id == listId, ct);

            if (list is null)
                return Result<ExpenseListDetailRespModel>.NotFound("Expense list not found.");

            var isMember = await _dbContext.TblGroupMembers
                .AsNoTracking()
                .AnyAsync(m => m.GroupId == list.GroupId && m.UserId == userId, ct);

            if (!isMember)
                return Result<ExpenseListDetailRespModel>.ValidationError("You are not a member of this group.");

            var detail = new ExpenseListDetailRespModel
            {
                Id = list.Id,
                GroupId = list.GroupId,
                Name = list.Name,
                IsDefault = list.IsDefault,
                CreatedUserId = list.CreatedUserId,
                CreatedAt = list.CreatedAt,
                Expenses = list.TblExpenseDetails
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
                    }).ToList()
            };

            return Result<ExpenseListDetailRespModel>.Success(detail);
        }
        catch (Exception ex)
        {
            return Result<ExpenseListDetailRespModel>.SystemError($"An error occurred: {ex.Message}");
        }
    }

    public async Task<Result<ExpenseListRespModel>> UpdateExpenseListAsync(long listId, long userId, UpdateExpenseListReqModel request, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Result<ExpenseListRespModel>.ValidationError("List name is required.");

            var list = await _dbContext.TblExpenses
                .FirstOrDefaultAsync(e => e.Id == listId, ct);

            if (list is null)
                return Result<ExpenseListRespModel>.NotFound("Expense list not found.");

            var isMember = await _dbContext.TblGroupMembers
                .AsNoTracking()
                .AnyAsync(m => m.GroupId == list.GroupId && m.UserId == userId, ct);

            if (!isMember)
                return Result<ExpenseListRespModel>.ValidationError("You are not a member of this group.");

            list.Name = request.Name;
            await _dbContext.SaveChangesAsync(ct);

            return Result<ExpenseListRespModel>.Success(new ExpenseListRespModel
            {
                Id = list.Id,
                GroupId = list.GroupId,
                Name = list.Name,
                IsDefault = list.IsDefault,
                CreatedUserId = list.CreatedUserId,
                CreatedAt = list.CreatedAt
            }, "Expense list updated successfully.");
        }
        catch (Exception ex)
        {
            return Result<ExpenseListRespModel>.SystemError($"An error occurred: {ex.Message}");
        }
    }

    public async Task<Result<string>> DeleteExpenseListAsync(long listId, long userId, CancellationToken ct = default)
    {
        try
        {
            var list = await _dbContext.TblExpenses
                .FirstOrDefaultAsync(e => e.Id == listId, ct);

            if (list is null)
                return Result<string>.NotFound("Expense list not found.");

            if (list.IsDefault)
                return Result<string>.ValidationError("The default list cannot be deleted.");

            var isMember = await _dbContext.TblGroupMembers
                .AsNoTracking()
                .AnyAsync(m => m.GroupId == list.GroupId && m.UserId == userId, ct);

            if (!isMember)
                return Result<string>.ValidationError("You are not a member of this group.");

            _dbContext.TblExpenses.Remove(list);
            await _dbContext.SaveChangesAsync(ct);

            return Result<string>.DeleteSuccess("Expense list deleted successfully.");
        }
        catch (Exception ex)
        {
            return Result<string>.SystemError($"An error occurred: {ex.Message}");
        }
    }
}
