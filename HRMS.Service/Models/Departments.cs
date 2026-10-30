using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Service.Models
{
    public class DepartmentList
    {
        public int Id { get; set; }

        public string? DepartmentName { get; set; }

        public DateTime? Created { get; set; }
        
        public string? CreatedBy { get; set; }
    }

    public class CreateDepartment
    {
        public string DepartmentName { get; set; } = string.Empty;
    }
}
