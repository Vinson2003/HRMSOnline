using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Service.Interface
{
    public interface ICurrentUserService
    {
        Task<string> GetUsernameAsync();
    }
}
