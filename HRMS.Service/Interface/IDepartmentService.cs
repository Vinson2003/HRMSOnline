using HRMS.Data.Entity;
using HRMS.Service.Base;
using HRMS.Service.Models;

namespace HRMS.Service.Interface
{
    public interface IDepartmentService
    {
        Task<BaseResponse<List<DepartmentList>>> GetAllDepartmentsAsync(BasePaging paging, string searchName);

        Task<BaseResponse<bool>> CreateDepartmentAsync(CreateDepartment request);

        Task<BaseResponse<bool>> UpdateDepartmentAsync(UpdateDepartment request);

        Task<BaseResponse<bool>> DeleteDepartmentAsync(int Id);
    }
}
