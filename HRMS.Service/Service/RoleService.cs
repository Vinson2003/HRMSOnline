using HRMS.Data.Context;
using HRMS.Data.Entity;
using HRMS.LIB;
using HRMS.Service.Base;
using HRMS.Service.Interface;
using HRMS.Service.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HRMS.Service.Service
{
    public class RoleService(HRMSContext context, ICurrentUserService currentUser) : IRoleService
    {
        private readonly HRMSContext _context = context;
        private readonly ICurrentUserService _currentUser = currentUser;

        public async Task<BaseResponse<List<RoleList>>> RoleListAsync(BasePaging paging)
        {
            var response = new BaseResponse<List<RoleList>>();

            var users = _context.MstRoles
                .Where(x => !x.IsDeleted)
                .Select(x => new RoleList
                {
                    Id = x.Id,
                    RoleName = x.RoleName,
                    Created = x.Created,
                });

            // Define a dictionary to map column names to their corresponding expressions
            var columnMappings = new Dictionary<string, Expression<Func<RoleList, object?>>>
            {
                ["Id"] = users => users.Id,
                ["RoleName"] = users => users.RoleName,
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

        public async Task<BaseResponse<bool>> CreateRoleAsync(CreateRole request)
        {
            var response = new BaseResponse<bool>();

            if (string.IsNullOrWhiteSpace(request.RoleName))
            {
                response.Result = false;
                response.Message = "Role name is required.";
                return response;
            }

            var roleName = request.RoleName.Trim();

            if (roleName.Length > 50)
            {
                response.Result = false;
                response.Message = "Role name cannot exceed 50 characters.";
                return response;
            }

            // Check if the role name already exists
            var alreadyExists = await _context.MstRoles.AnyAsync(role => role.RoleName == roleName);

            if (alreadyExists)
            {
                response.Result = false;
                response.Message = "Role name already exists.";
                return response;
            }

            // Get username from login
            var getUsername = await _currentUser.GetUsernameAsync();

            var role = new MstRole
            {
                RoleName = roleName,
                Created = DateTime.UtcNow,
                CreatedBy = getUsername
            };

            _context.MstRoles.Add(role);
            await _context.SaveChangesAsync();

            response.Result = true;
            response.Message = "Role Added Succesfully";
            return response;
        }

        public async Task<BaseResponse<bool>> UpdateRoleAsync(UpdateRole request)
        {
            var response = new BaseResponse<bool>();

            if (string.IsNullOrWhiteSpace(request.RoleName))
            {
                response.Result = false;
                response.Message = "Role name is required.";
                return response;
            }

            var roleName = request.RoleName.Trim();

            if (roleName.Length > 50)
            {
                response.Result = false;
                response.Message = "Role name cannot exceed 50 characters.";
                return response;
            }

            var role = await _context.MstRoles.FirstOrDefaultAsync(item => item.Id == request.Id && !item.IsDeleted);
            if (role == null)
            {
                response.Result = false;
                response.Message = "Role not found";
                return response;
            }

            if (Const.PROTECTED_ROLES.Contains(role.RoleName, StringComparer.OrdinalIgnoreCase))
            {
                response.Result = false;
                response.Message = "Default roles cannot be modified.";
                return response;
            }

            var alreadyExists = await _context.MstRoles.AnyAsync(item => item.Id != request.Id && item.RoleName == roleName);

            if (alreadyExists)
            {
                response.Result = false;
                response.Message = "Role name already exists.";
                return response;
            }

            role.RoleName = roleName;
            role.Updated = DateTime.UtcNow;
            role.UpdatedBy = await _currentUser.GetUsernameAsync();

            await _context.SaveChangesAsync();

            response.Result = true;
            response.Message = "Role updated successfully.";
            return response;
        }

        public async Task<BaseResponse<bool>> DeleteRoleAsync(int id)
        {
            var response = new BaseResponse<bool>();

            // Check if the role exists and is not deleted
            var role = await _context.MstRoles.FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted);
            if (role == null)
            {
                response.Result = false;
                response.Message = "Role not found.";
                return response;
            }

            if (Const.PROTECTED_ROLES.Contains(role.RoleName, StringComparer.OrdinalIgnoreCase))
            {
                response.Result = false;
                response.Message = "Default roles cannot be deleted.";
                return response;
            }

            // Check whether the role is being used by users
            var isRoleInUse = await _context.MstUsers.AnyAsync(user => user.RoleId == id && !user.IsDeleted);

            if (isRoleInUse)
            {
                response.Result = false;
                response.Message = "Cannot delete role because it is currently assigned to one or more users.";
                return response;
            }

            role.IsDeleted = true;
            role.Updated = DateTime.UtcNow;
            role.UpdatedBy = await _currentUser.GetUsernameAsync();

            await _context.SaveChangesAsync();

            response.Result = true;
            response.Message = "Role deleted successfully.";
            return response;
        }
    }
}
