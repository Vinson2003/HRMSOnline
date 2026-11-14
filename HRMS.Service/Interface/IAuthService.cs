using HRMS.Data.Entity;
using HRMS.Service.Base;

namespace HRMS.Service.Interface
{
    public interface IAuthService
    {
        Task<BaseResponse<bool>> AuthenticateAsync(string Username, string Password);
    }
}
