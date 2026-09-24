using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SharedExpenseTrackerApp.Domain.Features.ExpenseList;
using SharedExpenseTrackerApp.Domain.Models.ExpenseList;
using SharedExpenseTrackerApp.WebMvcApp.Filters;
using SharedExpenseTrackerApp.WebMvcApp.Helpers;
using SharedExpenseTrackerApp.WebMvcApp.Hubs;

namespace SharedExpenseTrackerApp.WebMvcApp.Controllers;

[RequireLogin]
public class ListsController : Controller
{
    private readonly IExpenseListService _listService;

    public ListsController(IExpenseListService listService)
    {
        _listService = listService;
    }

    private long CurrentUserId => HttpContext.Session.GetUserId()!.Value;

    // ─── GET /Lists/Create/{groupId} ─────────────────────────────────────────
    public async Task<IActionResult> Create(long groupId, bool isFirst = false)
    {
        ViewData["Title"] = isFirst ? "Create Your First List" : "Create New List";
        ViewData["FullName"] = HttpContext.Session.GetFullName();
        ViewData["GroupId"] = groupId;
        ViewData["IsFirst"] = isFirst;
        return View();
    }

    // ─── POST /Lists/Create (Axios JSON) ────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateListRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            return BadRequest(new { message = "List name is required." });

        var result = await _listService.CreateExpenseListAsync(req.GroupId, CurrentUserId,
            new CreateExpenseListReqModel { Name = req.Name });

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message });

        return Ok(new
        {
            message = result.Message,
            listId = result.Data!.Id,
            groupId = result.Data.GroupId,
            isDefault = result.Data.IsDefault
        });
    }

    // ─── GET /Lists/Details/{listId} ─────────────────────────────────────────
    public async Task<IActionResult> Details(long id)
    {
        var result = await _listService.GetExpenseListByIdAsync(id, CurrentUserId);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Message;
            return RedirectToAction("Index", "Groups");
        }

        var list = result.Data!;
        int unpaidCount = list.Expenses.Count(e => !e.IsPaid);
        int unpaidTotal = list.Expenses.Where(e => !e.IsPaid).Sum(e => e.Amount);

        ViewData["Title"] = list.Name;
        ViewData["FullName"] = HttpContext.Session.GetFullName();
        ViewData["CurrentUserId"] = CurrentUserId;
        ViewData["ListId"] = list.Id;
        ViewData["ListName"] = list.Name;
        ViewData["GroupId"] = list.GroupId;
        ViewData["IsDefault"] = list.IsDefault;
        ViewData["Expenses"] = list.Expenses;
        ViewData["UnpaidCount"] = unpaidCount;
        ViewData["UnpaidTotal"] = unpaidTotal;

        return View();
    }

    // ─── GET /Lists/Edit/{listId} ────────────────────────────────────────────
    public async Task<IActionResult> Edit(long id)
    {
        var result = await _listService.GetExpenseListByIdAsync(id, CurrentUserId);
        if (!result.IsSuccess)
            return RedirectToAction("Index", "Groups");

        ViewData["Title"] = "Edit List";
        ViewData["FullName"] = HttpContext.Session.GetFullName();
        ViewData["ListId"] = result.Data!.Id;
        ViewData["ListName"] = result.Data.Name;
        ViewData["GroupId"] = result.Data.GroupId;
        ViewData["IsDefault"] = result.Data.IsDefault;
        return View();
    }

    // ─── POST /Lists/Edit (Axios JSON) ───────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Edit([FromBody] EditListRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            return BadRequest(new { message = "List name is required." });

        var result = await _listService.UpdateExpenseListAsync(req.ListId, CurrentUserId,
            new UpdateExpenseListReqModel { Name = req.Name });

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = "List updated.", listId = req.ListId });
    }

    // ─── POST /Lists/Delete (Axios JSON) ─────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Delete([FromBody] DeleteListRequest req)
    {
        var result = await _listService.DeleteExpenseListAsync(req.ListId, CurrentUserId);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = "List deleted.", groupId = req.GroupId });
    }

    // ─── GET /Lists/GroupLists?groupId={groupId} (Axios JSON) ──────────────────
    [HttpGet]
    public async Task<IActionResult> GroupLists([FromQuery] long groupId)
    {
        if (groupId <= 0)
            return BadRequest(new { message = "Invalid group ID." });

        var result = await _listService.GetGroupListsAsync(groupId, CurrentUserId);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message });

        return Ok(result.Data);
    }
}

// ─── Request DTOs ─────────────────────────────────────────────────────────────
public record CreateListRequest(long GroupId, string Name);
public record EditListRequest(long ListId, string Name);
public record DeleteListRequest(long ListId, long GroupId);
