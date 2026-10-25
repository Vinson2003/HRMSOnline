using HRMS.Data.Entity;
using HRMS.Service.Base;
using HRMS.Service.Models;

namespace HRMS.Service.Interface
{
    public interface IDepartmentService
    {
        BaseResponse<List<DepartmentList>> GetAllDepartments(BasePaging paging, string searchName);

        BaseResponse<bool> CreateDepartment(CreateDepartment request);
    }
}
