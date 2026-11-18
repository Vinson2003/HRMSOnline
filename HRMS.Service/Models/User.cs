using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Service.Models
{
    public class UserList
    {
        public int Id { get; set; }

        public string Username { get; set; } = null!;

        public string Password { get; set; } = null!;

        public int EmployeeId { get; set; } 

        public string FullName { get; set; } = null!;

        public int RoleId { get; set; }

        public string RoleName { get; set; } = null!;

        public bool IsActive { get; set; }

        public DateTime? Created { get; set; }

        public string? CreatedBy { get; set; }
    }

    public class CreateUser
    {
        public string Username { get; set; } = null!;

        public string Password { get; set; } = null!;

        public string ConfirmPassword { get; set; } = null!;

        public int EmployeeId { get; set; }

        public int RoleId { get; set; }

        public bool IsActive { get; set; }
    }

    public class UpdateUser
    {
        public int Id { get; set; }

        public string Username { get; set; } = null!;

        public int RoleId { get; set; }

        public bool IsActive { get; set; }
    }
}
