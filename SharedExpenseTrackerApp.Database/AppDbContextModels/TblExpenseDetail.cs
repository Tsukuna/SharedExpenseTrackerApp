using System;
using System.Collections.Generic;

namespace SharedExpenseTrackerApp.Database.AppDbContextModels;

public partial class TblExpenseDetail
{
    public long Id { get; set; }

    public long ExpenseListId { get; set; }

    public string Name { get; set; } = null!;

    public int Amount { get; set; }

    public bool IsPaid { get; set; }

    public string? Note { get; set; }

    public long CreatedUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual TblUser CreatedUser { get; set; } = null!;

    public virtual TblExpense ExpenseList { get; set; } = null!;
}
