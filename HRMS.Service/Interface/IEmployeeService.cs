using HRMS.Service.Base;
using HRMS.Service.Models;

namespace HRMS.Service.Interface
{
    public interface IEmployeeService
    {
        Task<BaseResponse<List<EmployeeList>>> EmployeeListAsync(BasePaging paging, string searchName);
        Task<BaseResponse<bool>> CreateEmployeeAsync(CreateEmployee request);
        Task<BaseResponse<bool>> UpdateEmployeeAsync(UpdateEmployee request);
        Task<BaseResponse<bool>> DeleteEmployeeAsync(int employeeId);
    }
}
