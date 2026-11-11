using HRMS.Service.Base;
using HRMS.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Service.Interface
{
    public interface ILeaveTypeService
    {
        Task<BaseResponse<List<LeaveTypeList>>> LeaveTypeListAsync(BasePaging paging);

        Task<BaseResponse<bool>> CreateLeaveTypeAsync(CreateLeaveType request);

        Task<BaseResponse<bool>> UpdateLeaveTypeAsync(UpdateLeaveType request);

        Task<BaseResponse<bool>> DeleteLeaveTypeAsync(int leaveTypeId);
    }
}
