using HRMS.Data.Context;
using HRMS.Service.Base;
using HRMS.Service.Models;
using HRMS.Service.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using HRMS.Data.Entity;

namespace HRMS.Service.Service
{
    public class AuthService(HRMSContext context) : IAuthService
    {
        private readonly HRMSContext _context = context;
        private readonly PasswordHasher<MstUser> _passwordHasher = new();

        public async Task<BaseResponse<AuthUser>> AuthenticateAsync(string Username, string Password)
        {
            var response = new BaseResponse<AuthUser>();

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                response.Message = "Username and password are required.";
                return response;
            }

            Username = Username.Trim();

            var user = await _context.MstUsers.Include(u => u.Role).FirstOrDefaultAsync(u =>
                    u.Username == Username && u.IsActive && !u.IsDeleted);

            if (user == null || string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                response.Message = "Username or password is not valid.";
                return response;
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, Password);

            if (result == PasswordVerificationResult.Failed)
            {
                response.Message = "Username or password is not valid.";
                return response;
            }

            response.Result = new AuthUser
            {
                Id = user.Id,
                Username = user.Username,
                EmployeeId = user.EmployeeId,
                RoleId = user.RoleId,
                RoleName = user.Role.RoleName,
            };
            response.Message = "Login successful.";
            return response;
        }
    }
}
