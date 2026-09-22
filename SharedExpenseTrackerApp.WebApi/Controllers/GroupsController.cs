using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedExpenseTrackerApp.Domain.Features.Group;
using SharedExpenseTrackerApp.Domain.Models.Group;
using SharedExpenseTrackerApp.Domain.Shared;
using System.Security.Claims;

namespace SharedExpenseTrackerApp.WebApi.Controllers;

[ApiController]
[Route("api/v1/groups")]
[Authorize]
public class GroupsController : BaseController
{
    private readonly IGroupService _groupService;

    public GroupsController(IGroupService groupService)
    {
        _groupService = groupService;
    }

    private long GetUserId() =>
        long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupReqModel request, CancellationToken ct)
    {
        var result = await _groupService.CreateGroupAsync(GetUserId(), request, ct);
        return Execute(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetUserGroups(CancellationToken ct)
    {
        var result = await _groupService.GetUserGroupsAsync(GetUserId(), ct);
        return Execute(result);
    }

    [HttpGet("{groupId}")]
    public async Task<IActionResult> GetGroupDetail(long groupId, CancellationToken ct)
    {
        var result = await _groupService.GetGroupDetailAsync(groupId, GetUserId(), ct);
        return Execute(result);
    }

    [HttpPost("join")]
    public async Task<IActionResult> JoinGroup([FromBody] JoinGroupReqModel request, CancellationToken ct)
    {
        var result = await _groupService.JoinGroupAsync(GetUserId(), request, ct);
        return Execute(result);
    }

    [HttpGet("{groupId}/members")]
    public async Task<IActionResult> GetGroupMembers(long groupId, CancellationToken ct)
    {
        var result = await _groupService.GetGroupMembersAsync(groupId, GetUserId(), ct);
        return Execute(result);
    }
}
