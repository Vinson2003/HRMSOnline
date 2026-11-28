using HRMS.Data.Context;
using HRMS.Service.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Service.Service
{
    public class LeaveService(HRMSContext context, ICurrentUserService currentUser) : ILeaveService
    {
        private readonly HRMSContext _context = context;
        private readonly ICurrentUserService _currentUser = currentUser;

    }
}
