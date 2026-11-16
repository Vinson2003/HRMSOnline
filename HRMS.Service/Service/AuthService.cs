
using HRMS.Data.Context;
using HRMS.Data.Entity;
using HRMS.Service.Base;
using HRMS.Service.Models;
using HRMS.Service.Interface;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Service.Service
{
    public class AuthService(HRMSContext context, IPasswordHasher<MstUser> passwordHasher) : IAuthService
    {
        private readonly HRMSContext _context = context;
        private readonly IPasswordHasher<MstUser> _passwordHasher = passwordHasher;

        public async Task<BaseResponse<AuthUser>> AuthenticateAsync(string username, string password)
        {
            var response = new BaseResponse<AuthUser>();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                response.Message = "Username and Password are required.";
                return response;
            }

            username = username.Trim();

            var user = await _context.MstUsers.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive && !u.IsDeleted);

            if (user == null || string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                response.Message = "Username or password is not valid.";
                return response;
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

            if (result == PasswordVerificationResult.Failed)
            {
                response.Message = "Username or password is not valid.";
                return response;
            }

            // Update password hash if a newer hashing format is required.
            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, password);

                await _context.SaveChangesAsync();
            }

            response.Result = new AuthUser
            {
                Id = user.Id,
                Username = user.Username,
                EmployeeId = user.EmployeeId,
                RoleId = user.RoleId,
                RoleName = user.Role?.RoleName ?? string.Empty
            };

            response.Message = "Login successful.";

            return response;
        }
    }
}