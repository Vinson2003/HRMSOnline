using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Service.Models
{
    public class EmployeeList
    {
        public int Id { get; set; }

        public string EmployeeNumber { get; set; } = null!;

        public string FullName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public int DepartmentId { get; set; } 

        public int? ManagerId { get; set; }

        public string? DepartmentName { get; set; }

        public string? ManagerName { get; set; }

        public int AnnualLeaveBalance { get; set; }

        public DateTime? Created { get; set; }

        public string? CreatedBy { get; set; }
    }

    public class CreateEmployee
    {
        public string EmployeeNumber { get; set; } = null!;

        public string FullName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public int DepartmentId { get; set; }

        public int? ManagerId { get; set; }

        public int AnnualLeaveBalance { get; set; }
    }

    public class UpdateEmployee
    {
        public int Id { get; set; }
        public string EmployeeNumber { get; set; } = null!;

        public string FullName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public int DepartmentId { get; set; }

        public int? ManagerId { get; set; }

        public int AnnualLeaveBalance { get; set; }
    }
}
