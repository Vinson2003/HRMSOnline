using HRMS.Data.Context;
using HRMS.LIB;
using HRMS.Service.Base;
using HRMS.Service.Interface;
using HRMS.Service.Models;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

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
    }
}
