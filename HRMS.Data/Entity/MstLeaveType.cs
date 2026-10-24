using System;
using System.Collections.Generic;

namespace HRMS.Data.Entity;

public partial class MstLeaveType
{
    public int Id { get; set; }

    public string TypeName { get; set; } = null!;

    public bool RequiresAttachment { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<TrxLeaveRequest> TrxLeaveRequests { get; set; } = new List<TrxLeaveRequest>();
}
