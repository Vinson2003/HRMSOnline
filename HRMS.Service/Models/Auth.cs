using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Service.Models
{
    public class AuthUser
    {
        public int Id { get; set; }

        public string Username { get; set; } = null!;

        public int EmployeeId { get; set; }

        public int RoleId { get; set; }

        public string RoleName { get; set; } = null!;
    }
}
