using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedExpenseTrackerApp.Domain.Features.Expense;
using SharedExpenseTrackerApp.Domain.Models.Expense;
using SharedExpenseTrackerApp.Domain.Shared;
using System.Security.Claims;

namespace SharedExpenseTrackerApp.WebApi.Controllers;

[ApiController]
[Authorize]
public class ExpensesController : BaseController
{
    private readonly IExpenseService _expenseService;

    public ExpensesController(IExpenseService expenseService)
    {
        _expenseService = expenseService;
    }

    private long GetUserId() =>
        long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("lists/{listId}/expenses")]
    public async Task<IActionResult> AddExpense(long listId, [FromBody] CreateExpenseReqModel request, CancellationToken ct)
    {
        var result = await _expenseService.AddExpenseAsync(listId, GetUserId(), request, ct);
        return Execute(result);
    }

    [HttpGet("lists/{listId}/expenses")]
    public async Task<IActionResult> GetExpenses(long listId, CancellationToken ct)
    {
        var result = await _expenseService.GetExpensesByListAsync(listId, GetUserId(), ct);
        return Execute(result);
    }

    [HttpPatch("expenses/{expenseId}/paid")]
    public async Task<IActionResult> TogglePaid(long expenseId, [FromBody] TogglePaidReqModel request, CancellationToken ct)
    {
        var result = await _expenseService.TogglePaidAsync(expenseId, GetUserId(), request, ct);
        return Execute(result);
    }

    [HttpGet("lists/{listId}/expenses/summary")]
    public async Task<IActionResult> GetUnpaidSummary(long listId, CancellationToken ct)
    {
        var result = await _expenseService.GetUnpaidSummaryAsync(listId, GetUserId(), ct);
        return Execute(result);
    }
}
