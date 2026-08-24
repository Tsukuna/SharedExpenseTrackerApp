using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SharedExpenseTrackerApp.Database.AppDbContextModels;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TblExpense> TblExpenses { get; set; }

    public virtual DbSet<TblExpenseDetail> TblExpenseDetails { get; set; }

    public virtual DbSet<TblGroup> TblGroups { get; set; }

    public virtual DbSet<TblGroupMember> TblGroupMembers { get; set; }

    public virtual DbSet<TblUser> TblUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TblExpense>(entity =>
        {
            entity.ToTable("TblExpense");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedUserId).HasColumnName("created_user_id");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.IsDefault).HasColumnName("is_default");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");

            entity.HasOne(d => d.CreatedUser).WithMany(p => p.TblExpenses)
                .HasForeignKey(d => d.CreatedUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblExpense_created_user");

            entity.HasOne(d => d.Group).WithMany(p => p.TblExpenses)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK_TblExpense_group");
        });

        modelBuilder.Entity<TblExpenseDetail>(entity =>
        {
            entity.ToTable("TblExpenseDetail");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedUserId).HasColumnName("created_user_id");
            entity.Property(e => e.ExpenseListId).HasColumnName("expense_list_id");
            entity.Property(e => e.IsPaid).HasColumnName("isPaid");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.Note)
                .HasMaxLength(500)
                .HasColumnName("note");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasColumnName("updated_at");

            entity.HasOne(d => d.CreatedUser).WithMany(p => p.TblExpenseDetails)
                .HasForeignKey(d => d.CreatedUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblExpenseDetail_created_user");

            entity.HasOne(d => d.ExpenseList).WithMany(p => p.TblExpenseDetails)
                .HasForeignKey(d => d.ExpenseListId)
                .HasConstraintName("FK_TblExpenseDetail_expense");
        });

        modelBuilder.Entity<TblGroup>(entity =>
        {
            entity.ToTable("TblGroup");

            entity.HasIndex(e => e.ShareCode, "UQ_TblGroup_share_code").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedUserId).HasColumnName("created_user_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.ShareCode)
                .HasMaxLength(20)
                .HasColumnName("share_code");

            entity.HasOne(d => d.CreatedUser).WithMany(p => p.TblGroups)
                .HasForeignKey(d => d.CreatedUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblGroup_created_user");
        });

        modelBuilder.Entity<TblGroupMember>(entity =>
        {
            entity.ToTable("TblGroupMember");

            entity.HasIndex(e => new { e.GroupId, e.UserId }, "UQ_TblGroupMember_group_user").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.JoinedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("joined_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Group).WithMany(p => p.TblGroupMembers)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK_TblGroupMember_group");

            entity.HasOne(d => d.User).WithMany(p => p.TblGroupMembers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_TblGroupMember_user");
        });

        modelBuilder.Entity<TblUser>(entity =>
        {
            entity.ToTable("TblUser");

            entity.HasIndex(e => e.PhoneNumber, "UQ_TblUser_phone_number").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("full_name");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("phone_number");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
