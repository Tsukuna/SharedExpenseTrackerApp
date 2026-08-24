using System;
using System.Collections.Generic;

namespace SharedExpenseTrackerApp.Database.AppDbContextModels;

public partial class TblGroup
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string ShareCode { get; set; } = null!;

    public long CreatedUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual TblUser CreatedUser { get; set; } = null!;

    public virtual ICollection<TblExpense> TblExpenses { get; set; } = new List<TblExpense>();

    public virtual ICollection<TblGroupMember> TblGroupMembers { get; set; } = new List<TblGroupMember>();
}
