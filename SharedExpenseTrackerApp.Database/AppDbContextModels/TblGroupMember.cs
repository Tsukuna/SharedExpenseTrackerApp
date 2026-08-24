using System;
using System.Collections.Generic;

namespace SharedExpenseTrackerApp.Database.AppDbContextModels;

public partial class TblGroupMember
{
    public long Id { get; set; }

    public long GroupId { get; set; }

    public long UserId { get; set; }

    public DateTime JoinedAt { get; set; }

    public string? Description { get; set; }

    public virtual TblGroup Group { get; set; } = null!;

    public virtual TblUser User { get; set; } = null!;
}
