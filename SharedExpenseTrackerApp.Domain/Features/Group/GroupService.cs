using Microsoft.EntityFrameworkCore;
using SharedExpenseTrackerApp.Database.AppDbContextModels;
using SharedExpenseTrackerApp.Domain.Models.Group;
using SharedExpenseTrackerApp.Domain.Shared;

namespace SharedExpenseTrackerApp.Domain.Features.Group;

public class GroupService : IGroupService
{
    private readonly AppDbContext _dbContext;

    public GroupService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<GroupRespModel>> CreateGroupAsync(long userId, CreateGroupReqModel request, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Result<GroupRespModel>.ValidationError("Group name is required.");

            var shareCode = GenerateShareCode();

            var group = new TblGroup
            {
                Name = request.Name,
                ShareCode = shareCode,
                CreatedUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.TblGroups.Add(group);
            await _dbContext.SaveChangesAsync(ct);

            // Add creator as a member
            _dbContext.TblGroupMembers.Add(new TblGroupMember
            {
                GroupId = group.Id,
                UserId = userId,
                JoinedAt = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync(ct);

            return Result<GroupRespModel>.Success(new GroupRespModel
            {
                Id = group.Id,
                Name = group.Name,
                ShareCode = group.ShareCode,
                CreatedUserId = group.CreatedUserId,
                CreatedAt = group.CreatedAt
            }, "Group created successfully.");
        }
        catch (Exception ex)
        {
            return Result<GroupRespModel>.SystemError($"An error occurred while creating the group: {ex.Message}");
        }
    }

    public async Task<Result<List<GroupRespModel>>> GetUserGroupsAsync(long userId, CancellationToken ct = default)
    {
        try
        {
            var groups = await _dbContext.TblGroupMembers
                .AsNoTracking()
                .Where(m => m.UserId == userId)
                .Select(m => new GroupRespModel
                {
                    Id = m.Group.Id,
                    Name = m.Group.Name,
                    ShareCode = m.Group.ShareCode,
                    CreatedUserId = m.Group.CreatedUserId,
                    CreatedAt = m.Group.CreatedAt
                })
                .ToListAsync(ct);

            return Result<List<GroupRespModel>>.Success(groups);
        }
        catch (Exception ex)
        {
            return Result<List<GroupRespModel>>.SystemError($"An error occurred: {ex.Message}");
        }
    }

    public async Task<Result<GroupDetailRespModel>> GetGroupDetailAsync(long groupId, long userId, CancellationToken ct = default)
    {
        try
        {
            var isMember = await _dbContext.TblGroupMembers
                .AsNoTracking()
                .AnyAsync(m => m.GroupId == groupId && m.UserId == userId, ct);

            if (!isMember)
                return Result<GroupDetailRespModel>.ValidationError("You are not a member of this group.");

            var group = await _dbContext.TblGroups
                .AsNoTracking()
                .Include(g => g.TblGroupMembers)
                    .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(g => g.Id == groupId, ct);

            if (group is null)
                return Result<GroupDetailRespModel>.NotFound("Group not found.");

            var detail = new GroupDetailRespModel
            {
                Id = group.Id,
                Name = group.Name,
                ShareCode = group.ShareCode,
                CreatedUserId = group.CreatedUserId,
                CreatedAt = group.CreatedAt,
                Members = group.TblGroupMembers.Select(m => new GroupMemberRespModel
                {
                    Id = m.Id,
                    UserId = m.UserId,
                    FullName = m.User.FullName,
                    PhoneNumber = m.User.PhoneNumber,
                    JoinedAt = m.JoinedAt
                }).ToList()
            };

            return Result<GroupDetailRespModel>.Success(detail);
        }
        catch (Exception ex)
        {
            return Result<GroupDetailRespModel>.SystemError($"An error occurred: {ex.Message}");
        }
    }

    public async Task<Result<string>> JoinGroupAsync(long userId, JoinGroupReqModel request, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.ShareCode))
                return Result<string>.ValidationError("Share code is required.");

            var group = await _dbContext.TblGroups
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.ShareCode == request.ShareCode, ct);

            if (group is null)
                return Result<string>.ValidationError("Invalid share code.");

            var alreadyMember = await _dbContext.TblGroupMembers
                .AsNoTracking()
                .AnyAsync(m => m.GroupId == group.Id && m.UserId == userId, ct);

            if (alreadyMember)
                return Result<string>.ValidationError("You are already a member of this group.");

            _dbContext.TblGroupMembers.Add(new TblGroupMember
            {
                GroupId = group.Id,
                UserId = userId,
                JoinedAt = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync(ct);

            return Result<string>.Success("Joined group successfully.");
        }
        catch (Exception ex)
        {
            return Result<string>.SystemError($"An error occurred while joining the group: {ex.Message}");
        }
    }

    public async Task<Result<List<GroupMemberRespModel>>> GetGroupMembersAsync(long groupId, long userId, CancellationToken ct = default)
    {
        try
        {
            var isMember = await _dbContext.TblGroupMembers
                .AsNoTracking()
                .AnyAsync(m => m.GroupId == groupId && m.UserId == userId, ct);

            if (!isMember)
                return Result<List<GroupMemberRespModel>>.ValidationError("You are not a member of this group.");

            var members = await _dbContext.TblGroupMembers
                .AsNoTracking()
                .Where(m => m.GroupId == groupId)
                .Select(m => new GroupMemberRespModel
                {
                    Id = m.Id,
                    UserId = m.UserId,
                    FullName = m.User.FullName,
                    PhoneNumber = m.User.PhoneNumber,
                    JoinedAt = m.JoinedAt
                })
                .ToListAsync(ct);

            return Result<List<GroupMemberRespModel>>.Success(members);
        }
        catch (Exception ex)
        {
            return Result<List<GroupMemberRespModel>>.SystemError($"An error occurred: {ex.Message}");
        }
    }

    private static string GenerateShareCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
