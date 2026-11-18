using HRMS.Service.Base;
using HRMS.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Service.Interface
{
    public interface IUserService
    {
        Task<BaseResponse<List<UserList>>> UserListAsync(BasePaging paging, string searchName);

        Task<BaseResponse<bool>> CreateUserAsync(CreateUser request);

        Task<BaseResponse<bool>> UpdateUserAsync(UpdateUser request);

        Task<BaseResponse<bool>> DeleteUserAsync(int userId);
    }
}
