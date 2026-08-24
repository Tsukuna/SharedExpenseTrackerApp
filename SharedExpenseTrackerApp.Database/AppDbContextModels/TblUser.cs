using System;
using System.Collections.Generic;

namespace SharedExpenseTrackerApp.Database.AppDbContextModels;

public partial class TblUser
{
    public long Id { get; set; }

    public string FullName { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string Password { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<TblExpenseDetail> TblExpenseDetails { get; set; } = new List<TblExpenseDetail>();

    public virtual ICollection<TblExpense> TblExpenses { get; set; } = new List<TblExpense>();

    public virtual ICollection<TblGroupMember> TblGroupMembers { get; set; } = new List<TblGroupMember>();

    public virtual ICollection<TblGroup> TblGroups { get; set; } = new List<TblGroup>();
}
