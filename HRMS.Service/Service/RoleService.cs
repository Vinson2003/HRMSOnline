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
    public class RoleService(HRMSContext context) : IRoleService
    {
        private readonly HRMSContext _context = context;

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
                response.Message = "Role name cannot be empty.";
                return response;
            }

            var roleName = request.RoleName.Trim();

            // Check if the role name already exists and is not deleted
            var alreadyExists = await _context.MstRoles.AnyAsync(role => !role.IsDeleted && role.RoleName == roleName);

            if (alreadyExists)
            {
                response.Result = false;
                response.Message = "Role name already exists.";
                return response;
            }

            var role = new MstRole
            {
                RoleName = roleName,
                Created = DateTime.UtcNow,
                CreatedBy = "System"
            };

            _context.MstRoles.Add(role);
            await _context.SaveChangesAsync();

            response.Result = true;
            response.Message = "Role Added Succesfull";
            return response;
        }

        public async Task<BaseResponse<bool>> UpdateRoleAsync(UpdateRole request)
        {
            var response = new BaseResponse<bool>();

            if (string.IsNullOrWhiteSpace(request.RoleName))
            {
                response.Result = false;
                response.Message = "Role name cannot be empty.";
                return response;
            }

            var roleName = request.RoleName.Trim();

            // Check if the role exists and is not deleted
            var role = await _context.MstRoles.FirstOrDefaultAsync(item => item.Id == request.Id && !item.IsDeleted);
            if (role == null)
            {
                response.Result = false;
                response.Message = "Role not found";
                return response;
            }

            // Check if the new department name already exists for a different department
            var alreadyExists = await _context.MstRoles.AnyAsync(item => item.Id != request.Id &&
                !item.IsDeleted && item.RoleName == roleName);

            if (alreadyExists)
            {
                response.Result = false;
                response.Message = "Role name already exists.";
                return response;
            }

            role.RoleName = roleName;
            role.Updated = DateTime.UtcNow;
            role.UpdatedBy = "System";

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
                response.Result = true;
                response.Message = "Role not found";
                return response;
            }

            role.IsDeleted = true;
            role.Updated = DateTime.UtcNow;
            role.UpdatedBy = "System";

            await _context.SaveChangesAsync();

            response.Result = true;
            response.Message = "Role deleted successfully.";
            return response;
        }
    }
}
