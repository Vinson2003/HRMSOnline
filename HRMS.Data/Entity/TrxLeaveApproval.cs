using System;
using System.Collections.Generic;

namespace HRMS.Data.Entity;

public partial class TrxLeaveApproval
{
    public int Id { get; set; }

    public int LeaveRequestId { get; set; }

    public int ApproverId { get; set; }

    public string ActionStatus { get; set; } = null!;

    public DateTime ActionDate { get; set; }

    public string? Remarks { get; set; }

    public DateTime? Created { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? Updated { get; set; }

    public string? UpdatedBy { get; set; }

    public virtual MstEmployee Approver { get; set; } = null!;

    public virtual TrxLeaveRequest LeaveRequest { get; set; } = null!;
}
