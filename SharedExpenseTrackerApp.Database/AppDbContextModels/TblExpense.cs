using System;
using System.Collections.Generic;

namespace SharedExpenseTrackerApp.Database.AppDbContextModels;

public partial class TblExpense
{
    public long Id { get; set; }

    public long GroupId { get; set; }

    public string Name { get; set; } = null!;

    public bool IsDefault { get; set; }

    public long CreatedUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual TblUser CreatedUser { get; set; } = null!;

    public virtual TblGroup Group { get; set; } = null!;

    public virtual ICollection<TblExpenseDetail> TblExpenseDetails { get; set; } = new List<TblExpenseDetail>();
}
