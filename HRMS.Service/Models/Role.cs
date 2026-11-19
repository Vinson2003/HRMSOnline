using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Service.Models
{
    public class RoleList
    {
        public int Id { get; set; }

        public string RoleName { get; set; } = null!;

        public DateTime? Created { get; set; }

        public string? CreatedBy { get; set; }
    }
}
