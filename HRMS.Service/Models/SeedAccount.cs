namespace HRMS.Service.Models
{
    public class SeedAccount
    {
        public required string Username { get; set; }

        public required string RoleName { get; set; }

        public required string EmployeeNumber { get; set; }

        public required string FullName { get; set; }

        public required string Email { get; set; }

        public int DepartmentId { get; set; }

        public int? ManagerId { get; set; }
    }
}
