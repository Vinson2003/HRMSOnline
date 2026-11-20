using HRMS.Service.Base;
using HRMS.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Service.Interface
{
    public interface IRoleService
    {
        Task<BaseResponse<List<RoleList>>> RoleListAsync(BasePaging paging);

        Task<BaseResponse<bool>> CreateRoleAsync(CreateRole request);

        Task<BaseResponse<bool>> UpdateRoleAsync(UpdateRole request);

        Task<BaseResponse<bool>> DeleteRoleAsync(int id);
    }
}
