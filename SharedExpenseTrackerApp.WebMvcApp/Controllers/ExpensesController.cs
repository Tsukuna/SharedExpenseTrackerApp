using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SharedExpenseTrackerApp.Domain.Features.Expense;
using SharedExpenseTrackerApp.Domain.Features.ExpenseList;
using SharedExpenseTrackerApp.Domain.Models.Expense;
using SharedExpenseTrackerApp.WebMvcApp.Filters;
using SharedExpenseTrackerApp.WebMvcApp.Helpers;
using SharedExpenseTrackerApp.WebMvcApp.Hubs;

namespace SharedExpenseTrackerApp.WebMvcApp.Controllers;

[RequireLogin]
public class ExpensesController : Controller
{
    private readonly IExpenseService _expenseService;
    private readonly IExpenseListService _listService;
    private readonly IHubContext<ExpenseHub> _hubContext;

    public ExpensesController(
        IExpenseService expenseService,
        IExpenseListService listService,
        IHubContext<ExpenseHub> hubContext)
    {
        _expenseService = expenseService;
        _listService = listService;
        _hubContext = hubContext;
    }

    private long CurrentUserId => HttpContext.Session.GetUserId()!.Value;

    // ─── GET /Expenses/Create/{id} ──────────────────────────────────────────
    public async Task<IActionResult> Create(long id = 0, long listId = 0)
    {
        var targetListId = id != 0 ? id : listId;
        var listResult = await _listService.GetExpenseListByIdAsync(targetListId, CurrentUserId);
        if (!listResult.IsSuccess)
            return RedirectToAction("Index", "Groups");

        ViewData["Title"] = "Add Expense";
        ViewData["FullName"] = HttpContext.Session.GetFullName();
        ViewData["ListId"] = targetListId;
        ViewData["ListName"] = listResult.Data!.Name;
        ViewData["GroupId"] = listResult.Data.GroupId;
        return View();
    }

    // ─── POST /Expenses/Create (Axios JSON) ──────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExpenseRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            return BadRequest(new { message = "Expense name is required." });
        if (req.Amount <= 0)
            return BadRequest(new { message = "Amount must be greater than zero." });

        var result = await _expenseService.AddExpenseAsync(req.ListId, CurrentUserId,
            new CreateExpenseReqModel
            {
                Name = req.Name,
                Amount = req.Amount,
                Note = req.Note
            });

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message });

        // Notify SignalR clients about updated unpaid count
        await BroadcastUnpaidCount(req.ListId, req.GroupId);

        return Ok(new
        {
            message = "Expense added.",
            expenseId = result.Data!.Id,
            listId = req.ListId
        });
    }

    // ─── POST /Expenses/TogglePaid (Axios JSON) ───────────────────────────────
    [HttpPost]
    public async Task<IActionResult> TogglePaid([FromBody] TogglePaidRequest req)
    {
        var result = await _expenseService.TogglePaidAsync(req.ExpenseId, CurrentUserId,
            new TogglePaidReqModel { IsPaid = req.IsPaid });

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message });

        // Notify SignalR clients
        await BroadcastUnpaidCount(req.ListId, req.GroupId);

        return Ok(new
        {
            message = result.Message,
            expenseId = req.ExpenseId,
            isPaid = req.IsPaid
        });
    }

    // ─── GET /Expenses/Summary/{id} ─────────────────────────────────────────
    public async Task<IActionResult> Summary(long id = 0, long listId = 0)
    {
        var targetListId = id != 0 ? id : listId;
        var listResult = await _listService.GetExpenseListByIdAsync(targetListId, CurrentUserId);
        if (!listResult.IsSuccess)
            return RedirectToAction("Index", "Groups");

        var summaryResult = await _expenseService.GetUnpaidSummaryAsync(targetListId, CurrentUserId);
        if (!summaryResult.IsSuccess)
            return RedirectToAction("Details", "Lists", new { id = targetListId });

        var summary = summaryResult.Data!;
        var list = listResult.Data!;

        // Compute paid count/amount for chart
        var allExpenses = list.Expenses;
        int paidCount = allExpenses.Count(e => e.IsPaid);
        int paidAmount = allExpenses.Where(e => e.IsPaid).Sum(e => e.Amount);
        int totalAmount = allExpenses.Sum(e => e.Amount);

        ViewData["Title"] = $"Summary — {list.Name}";
        ViewData["FullName"] = HttpContext.Session.GetFullName();
        ViewData["ListId"] = targetListId;
        ViewData["ListName"] = list.Name;
        ViewData["GroupId"] = list.GroupId;
        ViewData["UnpaidCount"] = summary.TotalUnpaidCount;
        ViewData["UnpaidAmount"] = summary.TotalUnpaidAmount;
        ViewData["PaidCount"] = paidCount;
        ViewData["PaidAmount"] = paidAmount;
        ViewData["TotalAmount"] = totalAmount;
        ViewData["UnpaidExpenses"] = summary.UnpaidExpenses;

        return View();
    }

    // ─── Private: Broadcast unpaid count via SignalR ──────────────────────────
    private async Task BroadcastUnpaidCount(long listId, long groupId)
    {
        var summaryResult = await _expenseService.GetUnpaidSummaryAsync(listId, CurrentUserId);
        if (!summaryResult.IsSuccess) return;

        var groupKey = ExpenseHub.ListGroupKey(groupId, listId);
        await _hubContext.Clients.Group(groupKey)
            .SendAsync("UnpaidCountChanged", groupId, listId, summaryResult.Data!.TotalUnpaidCount, summaryResult.Data.TotalUnpaidAmount);
    }
}

// ─── Request DTOs ─────────────────────────────────────────────────────────────
public record CreateExpenseRequest(long ListId, long GroupId, string Name, int Amount, string? Note);
public record TogglePaidRequest(long ExpenseId, long ListId, long GroupId, bool IsPaid);
