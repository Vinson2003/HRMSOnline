using System;
using System.Collections.Generic;

namespace HRMS.Data.Entity;

public partial class MstUser
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int EmployeeId { get; set; }

    public int RoleId { get; set; }

    public bool IsActive { get; set; }

    public DateTime? Created { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? Updated { get; set; }

    public string? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public virtual MstEmployee Employee { get; set; } = null!;

    public virtual MstRole Role { get; set; } = null!;
}
