using System;
using System.Collections.Generic;

namespace HRMS.Data.Entity;

public partial class MstDepartment
{
    public int Id { get; set; }

    public string DepartmentName { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public DateTime? Created { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? Updated { get; set; }

    public string? UpdatedBy { get; set; }

    public virtual ICollection<MstEmployee> MstEmployees { get; set; } = new List<MstEmployee>();
}
