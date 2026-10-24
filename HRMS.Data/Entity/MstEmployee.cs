using System;
using System.Collections.Generic;

namespace HRMS.Data.Entity;

public partial class MstEmployee
{
    public int Id { get; set; }

    public string EmployeeNumber { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int DepartmentId { get; set; }

    public int? ManagerId { get; set; }

    public int AnnualLeaveBalance { get; set; }

    public bool IsDeleted { get; set; }

    public virtual MstDepartment Department { get; set; } = null!;

    public virtual ICollection<MstEmployee> InverseManager { get; set; } = new List<MstEmployee>();

    public virtual MstEmployee? Manager { get; set; }

    public virtual ICollection<TrxLeaveApproval> TrxLeaveApprovals { get; set; } = new List<TrxLeaveApproval>();

    public virtual ICollection<TrxLeaveRequest> TrxLeaveRequests { get; set; } = new List<TrxLeaveRequest>();
}
