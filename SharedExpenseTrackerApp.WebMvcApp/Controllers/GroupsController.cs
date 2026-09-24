using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SharedExpenseTrackerApp.Domain.Features.Group;
using SharedExpenseTrackerApp.Domain.Models.Group;
using SharedExpenseTrackerApp.WebMvcApp.Filters;
using SharedExpenseTrackerApp.WebMvcApp.Helpers;
using SharedExpenseTrackerApp.WebMvcApp.Hubs;

namespace SharedExpenseTrackerApp.WebMvcApp.Controllers;

[RequireLogin]
public class GroupsController : Controller
{
    private readonly IGroupService _groupService;

    public GroupsController(IGroupService groupService)
    {
        _groupService = groupService;
    }

    private long CurrentUserId => HttpContext.Session.GetUserId()!.Value;

    // ─── GET /Groups ─────────────────────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        var result = await _groupService.GetUserGroupsAsync(CurrentUserId);

        ViewData["Title"] = "My Groups";
        ViewData["FullName"] = HttpContext.Session.GetFullName();
        ViewData["Groups"] = result.IsSuccess ? result.Data : new List<GroupRespModel>();
        ViewData["Error"] = result.IsError ? result.Message : null;

        return View();
    }

    // ─── GET /Groups/Create ──────────────────────────────────────────────────
    public IActionResult Create()
    {
        ViewData["Title"] = "Create Group";
        ViewData["FullName"] = HttpContext.Session.GetFullName();
        return View();
    }

    // ─── POST /Groups/Create ─────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name)
    {
        var result = await _groupService.CreateGroupAsync(CurrentUserId, new CreateGroupReqModel { Name = name });

        if (!result.IsSuccess)
        {
            ViewData["Error"] = result.Message;
            ViewData["Name"] = name;
            return View();
        }

        // After creating the group, redirect to create the first (default) list
        return RedirectToAction("Create", "Lists", new { groupId = result.Data!.Id, isFirst = true });
    }

    // ─── GET /Groups/Join ────────────────────────────────────────────────────
    public IActionResult Join()
    {
        ViewData["Title"] = "Join Group";
        ViewData["FullName"] = HttpContext.Session.GetFullName();
        return View();
    }

    // ─── POST /Groups/Join ───────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Join(string shareCode)
    {
        var result = await _groupService.JoinGroupAsync(CurrentUserId, new JoinGroupReqModel { ShareCode = shareCode });

        if (!result.IsSuccess)
        {
            ViewData["Error"] = result.Message;
            ViewData["ShareCode"] = shareCode;
            return View();
        }

        return RedirectToAction("Index");
    }

    // ─── GET /Groups/Details/{groupId} ───────────────────────────────────────
    public async Task<IActionResult> Details(long id)
    {
        var result = await _groupService.GetGroupDetailAsync(id, CurrentUserId);

        if (!result.IsSuccess)
        {
            ViewData["Error"] = result.Message;
            return RedirectToAction("Index");
        }

        ViewData["Title"] = result.Data!.Name;
        ViewData["FullName"] = HttpContext.Session.GetFullName();
        ViewData["CurrentUserId"] = CurrentUserId;
        ViewData["Group"] = result.Data;
        ViewData["GroupId"] = result.Data.Id;
        ViewData["GroupName"] = result.Data.Name;
        ViewData["ShareCode"] = result.Data.ShareCode;
        ViewData["Members"] = result.Data.Members;
        ViewData["IsCreator"] = result.Data.CreatedUserId == CurrentUserId;

        return View();
    }
}
