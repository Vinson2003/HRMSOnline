using System;
using System.Collections.Generic;
using HRMS.Data.Entity;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Data.Context;

public partial class HRMSContext : DbContext
{
    public HRMSContext(DbContextOptions<HRMSContext> options)
        : base(options)
    {
    }

    public virtual DbSet<MstDepartment> MstDepartments { get; set; }

    public virtual DbSet<MstEmployee> MstEmployees { get; set; }

    public virtual DbSet<MstLeaveType> MstLeaveTypes { get; set; }

    public virtual DbSet<TrxLeaveApproval> TrxLeaveApprovals { get; set; }

    public virtual DbSet<TrxLeaveRequest> TrxLeaveRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MstDepartment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Mst_Depa__3214EC070A0446BD");

            entity.ToTable("Mst_Department");

            entity.Property(e => e.DepartmentName)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MstEmployee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Mst_Empl__3214EC07EE0897DA");

            entity.ToTable("Mst_Employee");

            entity.HasIndex(e => e.EmployeeNumber, "UQ__Mst_Empl__8D6635982D11A7C4").IsUnique();

            entity.Property(e => e.AnnualLeaveBalance).HasDefaultValue(12);
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.EmployeeNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.FullName)
                .HasMaxLength(150)
                .IsUnicode(false);

            entity.HasOne(d => d.Department).WithMany(p => p.MstEmployees)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Employee_Department");

            entity.HasOne(d => d.Manager).WithMany(p => p.InverseManager)
                .HasForeignKey(d => d.ManagerId)
                .HasConstraintName("FK_Employee_Manager");
        });

        modelBuilder.Entity<MstLeaveType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Mst_Leav__3214EC0786DE11E4");

            entity.ToTable("Mst_LeaveType");

            entity.Property(e => e.TypeName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TrxLeaveApproval>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Trx_Leav__3214EC079C1F63CA");

            entity.ToTable("Trx_LeaveApproval");

            entity.Property(e => e.ActionDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ActionStatus)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Remarks)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.Approver).WithMany(p => p.TrxLeaveApprovals)
                .HasForeignKey(d => d.ApproverId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LeaveApproval_Approver");

            entity.HasOne(d => d.LeaveRequest).WithMany(p => p.TrxLeaveApprovals)
                .HasForeignKey(d => d.LeaveRequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LeaveApproval_LeaveRequest");
        });

        modelBuilder.Entity<TrxLeaveRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Trx_Leav__3214EC07FACF2F89");

            entity.ToTable("Trx_LeaveRequest");

            entity.Property(e => e.AttachmentPath)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Reason)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.RequestDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.Employee).WithMany(p => p.TrxLeaveRequests)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LeaveRequest_Employee");

            entity.HasOne(d => d.LeaveType).WithMany(p => p.TrxLeaveRequests)
                .HasForeignKey(d => d.LeaveTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LeaveRequest_LeaveType");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
