using HRMS.Data.Context;
using HRMS.LIB;
using HRMS.Service.Base;
using HRMS.Service.Interface;
using HRMS.Service.Models;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using HRMS.Data.Entity;
using Microsoft.AspNetCore.Identity;

namespace HRMS.Service.Service
{
    public class UserService(HRMSContext context, IPasswordHasher<MstUser> passwordHasher) : IUserService
    {
        private readonly HRMSContext _context = context;
        private readonly IPasswordHasher<MstUser> _passwordHasher = passwordHasher;

        public async Task<BaseResponse<List<UserList>>> UserListAsync(BasePaging paging, string searchName)
        {
            var response = new BaseResponse<List<UserList>>();

            var users = _context.MstUsers
                .Where(x => !x.IsDeleted && (string.IsNullOrWhiteSpace(searchName) || x.Username.Contains(searchName)))
                .Select(x => new UserList
                {
                    Id = x.Id,
                    Username = x.Username,
                    EmployeeId = x.EmployeeId,
                    FullName = x.Employee.FullName,
                    RoleId = x.RoleId,
                    RoleName = x.Role.RoleName,
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
                ["ManagerName"] = users => users.RoleName,
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

            if (!await _context.MstRoles.AnyAsync(d => d.Id == request.RoleId))
            {
                response.Result = false;
                response.Message = "Role not found.";
                return response;
            }

            var newUser = new MstUser
            {
                Username = username,
                EmployeeId = request.EmployeeId,
                RoleId = request.RoleId,
                IsActive = request.IsActive,
                Created = DateTime.UtcNow,
                CreatedBy = "System"
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

            var user = await _context.MstUsers.FindAsync(request.Id);

            if (user == null || user.IsDeleted)
            {
                response.Result = false;
                response.Message = "User not found";
                return response;
            }

            if (string.IsNullOrWhiteSpace(request.Username))
            {
                response.Result = false;
                response.Message = "Username are required.";
                return response;
            }

            var username = request.Username.Trim();

            if (username.Length > 50)
            {
                response.Result = false;
                response.Message = "Username cannot exceed 50 characters.";
                return response;
            }

            if (await _context.MstUsers.AnyAsync(e => e.Username == username || e.Id != request.Id))
            {
                response.Result = false;
                response.Message = "Username already exists.";
                return response;
            }

            if (!await _context.MstRoles.AnyAsync(d => d.Id == request.RoleId))
            {
                response.Result = false;
                response.Message = "Role not found.";
                return response;
            }

            user.Username = username;
            user.RoleId = request.RoleId;
            user.IsActive = request.IsActive;
            user.Updated = DateTime.UtcNow;
            user.UpdatedBy = "System";

            await _context.SaveChangesAsync();

            response.Result = true;
            response.Message = "Update User Successful";
            return response;
        }

        public async Task<BaseResponse<bool>> DeleteUserAsync(int userId)
        {
            var response = new BaseResponse<bool>();

            var user = await _context.MstUsers.FindAsync(userId);
            if (user == null || user.IsDeleted)
            {
                response.Result = false;
                response.Message = "User not found.";
                return response;
            }

            user.IsDeleted = true;
            user.Updated = DateTime.UtcNow;
            user.UpdatedBy = "System";

            await _context.SaveChangesAsync();

            response.Result = true;
            response.Message = "User deleted successful.";
            return response;
        }
    }
}
