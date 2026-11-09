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
    }
}
