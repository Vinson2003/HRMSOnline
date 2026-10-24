using System;
using System.Collections.Generic;

namespace HRMS.Data.Entity;

public partial class MstDepartment
{
    public int Id { get; set; }

    public string DepartmentName { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public virtual ICollection<MstEmployee> MstEmployees { get; set; } = new List<MstEmployee>();
}
