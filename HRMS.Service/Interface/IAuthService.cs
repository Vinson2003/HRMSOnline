using HRMS.Data.Entity;
using HRMS.Service.Base;
using HRMS.Service.Models;

namespace HRMS.Service.Interface
{
    public interface IAuthService
    {
        Task<BaseResponse<AuthUser>> AuthenticateAsync(string Username, string Password);
    }
}
