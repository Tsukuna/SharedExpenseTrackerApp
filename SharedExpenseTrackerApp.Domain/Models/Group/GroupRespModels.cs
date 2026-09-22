namespace SharedExpenseTrackerApp.Domain.Models.Group;

public class GroupRespModel
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string ShareCode { get; set; } = null!;
    public long CreatedUserId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GroupMemberRespModel
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public DateTime JoinedAt { get; set; }
}

public class GroupDetailRespModel
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string ShareCode { get; set; } = null!;
    public long CreatedUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<GroupMemberRespModel> Members { get; set; } = new();
}
