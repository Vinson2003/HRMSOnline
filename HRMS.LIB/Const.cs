namespace HRMS.LIB
{
    public class Const
    {
        public static readonly string PAGING_SORT_ASC = "asc";
        public static readonly string PAGING_SORT_DESC = "desc";

        public static readonly string ADMIN_ROLE = "Admin";
        public static readonly string HR_ROLE = "HR";
        public static readonly string MANAGER_ROLE = "Manager";
        public static readonly string EMPLOYEE_ROLE = "Employee";
        public static readonly string[] PROTECTED_ROLES = { ADMIN_ROLE, HR_ROLE, MANAGER_ROLE, EMPLOYEE_ROLE };
    }
}