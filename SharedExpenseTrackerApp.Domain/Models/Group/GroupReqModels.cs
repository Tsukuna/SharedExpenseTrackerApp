namespace SharedExpenseTrackerApp.Domain.Models.Group;

public class CreateGroupReqModel
{
    public string Name { get; set; } = null!;
}

public class JoinGroupReqModel
{
    public string ShareCode { get; set; } = null!;
}
