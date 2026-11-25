using HRMS.Data.Context;
using HRMS.Data.Entity;
using HRMS.LIB;
using HRMS.Service.Base;
using HRMS.Service.Interface;
using HRMS.Service.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HRMS.Service.Service
{
    public class UserService(HRMSContext context, IPasswordHasher<MstUser> passwordHasher, ICurrentUserService currentUser) : IUserService
    {
        private readonly HRMSContext _context = context;
        private readonly IPasswordHasher<MstUser> _passwordHasher = passwordHasher;
        private readonly ICurrentUserService _currentUser = currentUser;

        // Is there at least one OTHER active Admin besides this user?
        private async Task<bool> HasOtherActiveAdminAsync(int excludeUserId)
        {
            return await _context.MstUsers.AnyAsync(u => u.Id != excludeUserId && !u.IsDeleted && u.IsActive && u.Role.RoleName == Const.ADMIN_ROLE);
        }

        public async Task<BaseResponse<List<UserList>>> UserListAsync(BasePaging paging, string searchName)
        {
            var response = new BaseResponse<List<UserList>>();

            var users = _context.MstUsers
                .Where(x => !x.IsDeleted && (string.IsNullOrWhiteSpace(searchName) || x.Username.Contains(searchName)
                || string.IsNullOrEmpty(searchName) || x.Employee.FullName.Contains(searchName)))
                .Select(x => new UserList 
                {
                    Id = x.Id,
                    Username = x.Username,
                    EmployeeId = x.EmployeeId,
                    FullName = x.Employee.FullName,
                    RoleId = x.RoleId,
                    RoleName = x.Role.RoleName,
                    IsActive = x.IsActive,
                    Created = x.Created,
                });

            // Define a dictionary to map column names to their corresponding expressions
            var columnMappings = new Dictionary<string, Expression<Func<UserList, object?>>>
            {
                ["Id"] = users => users.Id,
                ["Username"] = users => users.Username,
                ["EmployeeId"] = users => users.EmployeeId,
                ["FullName"] = users => users.FullName,
                ["RoleId"] = users => users.RoleId,
                ["RoleName"] = users => users.RoleName,
                ["IsActive"] = users => users.IsActive,
                ["Created"] = users => users.Created,
            };

            // Apply sorting based on the provided column and sort direction
            if (paging.Column is not null && columnMappings.TryGetValue(paging.Column, out var keySelector))
            {
                users = paging.SortBy == Const.PAGING_SORT_ASC ? users.OrderBy(keySelector) : users.OrderByDescending(keySelector);
            }
            else
            {
                // Default order so paging is stable
                users = users.OrderBy(e => e.Id);
            }

            // Get the total count of users before applying pagination
            var total = await users.CountAsync();

            // Apply pagination using Skip and Take
            var result = await users.Skip(paging.Start).Take(paging.Length).ToListAsync();

            response.Total = total;
            response.Result = result;
            return response;
        }

        public async Task<BaseResponse<bool>> CreateUserAsync(CreateUser request)
        {
            var response = new BaseResponse<bool>();

            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                response.Result = false;
                response.Message = "Username and Password are required.";
                return response;
            }

            var username = request.Username.Trim();
            var password = request.Password;

            if (username.Length > 50)
            {
                response.Result = false;
                response.Message = "Username cannot exceed 50 characters.";
                return response;
            }

            if (await _context.MstUsers.AnyAsync(e => e.Username == username))
            {
                response.Result = false;
                response.Message = "Username already exists.";
                return response;
            }

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            {
                response.Message = "Password must be at least 8 characters.";
                return response;
            }

            if (request.Password != request.ConfirmPassword)
            {
                response.Message = "Password and confirmation do not match.";
                return response;
            }

            if (!await _context.MstEmployees.AnyAsync(d => d.Id == request.EmployeeId && !d.IsDeleted))
            {
                response.Result = false;
                response.Message = "Employee not found.";
                return response;
            }

            if (await _context.MstUsers.AnyAsync(d => d.EmployeeId == request.EmployeeId))
            {
                response.Result = false;
                response.Message = "This employee already has an account.";
                return response;
            }

            if (!await _context.MstRoles.AnyAsync(d => d.Id == request.RoleId))
            {
                response.Result = false;
                response.Message = "Role not found.";
                return response;
            }

            var getUsername = await _currentUser.GetUsernameAsync();

            var newUser = new MstUser
            {
                Username = username,
                EmployeeId = request.EmployeeId,
                RoleId = request.RoleId,
                IsActive = request.IsActive,
                Created = DateTime.UtcNow,
                CreatedBy = getUsername
            };

            // Hash the password: the plain password is never stored
            newUser.PasswordHash = _passwordHasher.HashPassword(newUser, request.Password);

            _context.MstUsers.Add(newUser);
            await _context.SaveChangesAsync();

            response.Result = true;
            response.Message = "Create User Successful.";

            return response;
        }

        public async Task<BaseResponse<bool>> UpdateUserAsync(UpdateUser request)
        {
            var response = new BaseResponse<bool>();

            var user = await _context.MstUsers.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == request.Id);

            if (user == null || user.IsDeleted)
            {
                response.Result = false;
                response.Message = "User not found";
                return response;
            }

            var getUserId = await _currentUser.GetUserIdAsync();

            if (request.Id == getUserId && user.IsActive == false || request.RoleId != user.RoleId)
            {
                response.Result = false;
                response.Message = "You cannot deactivate your own account or change your own role.";
                return response;
            }

            if (string.IsNullOrWhiteSpace(request.Username))
            {
                response.Result = false;
                response.Message = "Username is required.";
                return response;
            }

            var username = request.Username.Trim();

            if (username.Length > 50)
            {
                response.Result = false;
                response.Message = "Username cannot exceed 50 characters.";
                return response;
            }

            if (await _context.MstUsers.AnyAsync(e => e.Username == username && e.Id != request.Id))
            {
                response.Result = false;
                response.Message = "Username already exists.";
                return response;
            }

            var newRole = await _context.MstRoles.FirstOrDefaultAsync(r => r.Id == request.RoleId);
            if (newRole == null)
            {
                response.Result = false;
                response.Message = "Role not found.";
                return response;
            }

            // Last Admin rule
            var isActiveAdminNow = user.IsActive && user.Role.RoleName == Const.ADMIN_ROLE;
            var willStayActiveAdmin = request.IsActive && newRole.RoleName == Const.ADMIN_ROLE;

            if (isActiveAdminNow && !willStayActiveAdmin && !await HasOtherActiveAdminAsync(user.Id))
            {
                response.Result = false;
                response.Message = "Cannot change: this is the last active Admin.";
                return response;
            }

            user.Username = username;
            user.RoleId = request.RoleId;
            user.IsActive = request.IsActive;
            user.Updated = DateTime.UtcNow;
            user.UpdatedBy = await _currentUser.GetUsernameAsync();

            await _context.SaveChangesAsync();

            response.Result = true;
            response.Message = "Update User Successful";
            return response;
        }

        public async Task<BaseResponse<bool>> DeleteUserAsync(int userId)
        {
            var response = new BaseResponse<bool>();

            var user = await _context.MstUsers.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || user.IsDeleted)
            {
                response.Result = false;
                response.Message = "User not found.";
                return response;
            }

            // Get user id from login and check cannot delete own account.
            var getUserId = await _currentUser.GetUserIdAsync();

            if (userId == getUserId)
            {
                response.Result = false;
                response.Message = "You cannot delete your own account.";
                return response;
            }

            // Last Admin rule: an active Admin can only be deleted if another active Admin exists
            if (user.IsActive && user.Role.RoleName == Const.ADMIN_ROLE && !await HasOtherActiveAdminAsync(user.Id))
            {
                response.Result = false;
                response.Message = "Cannot delete: this is the last active Admin.";
                return response;
            }

            user.IsDeleted = true;
            user.Updated = DateTime.UtcNow;
            user.UpdatedBy = await _currentUser.GetUsernameAsync();

            await _context.SaveChangesAsync();

            response.Result = true;
            response.Message = "User deleted successful.";
            return response;
        }

        public async Task<BaseResponse<bool>> ChangePasswordAsync(ChangeUserPassword request)
        {
            var response = new BaseResponse<bool>();

            var user = await _context.MstUsers.FirstOrDefaultAsync(u => u.Id == request.Id);
            if (user == null || user.IsDeleted)
            {
                response.Result = false;
                response.Message = "User not found.";
                return response;
            }

            if (string.IsNullOrEmpty(request.NewPassword) || request.NewPassword.Length < 8)
            {
                response.Result = false;
                response.Message = "Password must be at least 8 characters.";
                return response;
            }

            if (request.NewPassword != request.ConfirmPassword)
            {
                response.Result = false;
                response.Message = "Password and confirmation do not match.";
                return response;
            }

            // Hash the new password and store it on the REAL user
            user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
            user.Updated = DateTime.UtcNow;
            user.UpdatedBy = await _currentUser.GetUsernameAsync();

            await _context.SaveChangesAsync();

            response.Result = true;
            response.Message = "Password changed successfully.";
            return response;
        }
    }
}
