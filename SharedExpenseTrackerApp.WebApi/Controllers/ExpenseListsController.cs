using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedExpenseTrackerApp.Domain.Features.ExpenseList;
using SharedExpenseTrackerApp.Domain.Models.ExpenseList;
using SharedExpenseTrackerApp.Domain.Shared;
using System.Security.Claims;

namespace SharedExpenseTrackerApp.WebApi.Controllers;

[ApiController]
[Authorize]
public class ExpenseListsController : BaseController
{
    private readonly IExpenseListService _expenseListService;

    public ExpenseListsController(IExpenseListService expenseListService)
    {
        _expenseListService = expenseListService;
    }

    private long GetUserId() =>
        long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("groups/{groupId}/lists")]
    public async Task<IActionResult> GetGroupLists(long groupId, CancellationToken ct)
    {
        var result = await _expenseListService.GetGroupListsAsync(groupId, GetUserId(), ct);
        return Execute(result);
    }

    [HttpPost("groups/{groupId}/lists")]
    public async Task<IActionResult> CreateExpenseList(long groupId, [FromBody] CreateExpenseListReqModel request, CancellationToken ct)
    {
        var result = await _expenseListService.CreateExpenseListAsync(groupId, GetUserId(), request, ct);
        return Execute(result);
    }

    [HttpGet("lists/{listId}")]
    public async Task<IActionResult> GetExpenseListById(long listId, CancellationToken ct)
    {
        var result = await _expenseListService.GetExpenseListByIdAsync(listId, GetUserId(), ct);
        return Execute(result);
    }

    [HttpPut("lists/{listId}")]
    public async Task<IActionResult> UpdateExpenseList(long listId, [FromBody] UpdateExpenseListReqModel request, CancellationToken ct)
    {
        var result = await _expenseListService.UpdateExpenseListAsync(listId, GetUserId(), request, ct);
        return Execute(result);
    }

    [HttpDelete("lists/{listId}")]
    public async Task<IActionResult> DeleteExpenseList(long listId, CancellationToken ct)
    {
        var result = await _expenseListService.DeleteExpenseListAsync(listId, GetUserId(), ct);
        return Execute(result);
    }
}
