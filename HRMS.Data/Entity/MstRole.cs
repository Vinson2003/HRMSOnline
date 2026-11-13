using System;
using System.Collections.Generic;

namespace HRMS.Data.Entity;

public partial class MstRole
{
    public int Id { get; set; }

    public string RoleName { get; set; } = null!;

    public DateTime? Created { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? Updated { get; set; }

    public string? UpdatedBy { get; set; }

    public virtual ICollection<MstUser> MstUsers { get; set; } = new List<MstUser>();
}
