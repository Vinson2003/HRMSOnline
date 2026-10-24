using System;
using System.Collections.Generic;

namespace HRMS.Data.Entity;

public partial class TrxLeaveRequest
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public int LeaveTypeId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public int TotalDays { get; set; }

    public string Reason { get; set; } = null!;

    public string? AttachmentPath { get; set; }

    public string Status { get; set; } = null!;

    public DateTime RequestDate { get; set; }

    public bool IsDeleted { get; set; }

    public virtual MstEmployee Employee { get; set; } = null!;

    public virtual MstLeaveType LeaveType { get; set; } = null!;

    public virtual ICollection<TrxLeaveApproval> TrxLeaveApprovals { get; set; } = new List<TrxLeaveApproval>();
}
