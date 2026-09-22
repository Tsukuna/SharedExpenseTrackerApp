using SharedExpenseTrackerApp.Domain.Models.Group;
using SharedExpenseTrackerApp.Domain.Shared;

namespace SharedExpenseTrackerApp.Domain.Features.Group;

public interface IGroupService
{
    Task<Result<GroupRespModel>> CreateGroupAsync(long userId, CreateGroupReqModel request, CancellationToken ct = default);
    Task<Result<List<GroupRespModel>>> GetUserGroupsAsync(long userId, CancellationToken ct = default);
    Task<Result<GroupDetailRespModel>> GetGroupDetailAsync(long groupId, long userId, CancellationToken ct = default);
    Task<Result<string>> JoinGroupAsync(long userId, JoinGroupReqModel request, CancellationToken ct = default);
    Task<Result<List<GroupMemberRespModel>>> GetGroupMembersAsync(long groupId, long userId, CancellationToken ct = default);
}
