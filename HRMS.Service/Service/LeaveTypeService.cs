using HRMS.Data.Context;
using HRMS.LIB;
using HRMS.Service.Base;
using HRMS.Service.Interface;
using HRMS.Service.Models;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Service.Service
{
    public class LeaveTypeService(HRMSContext context) : ILeaveTypeService
    {
        private readonly HRMSContext _context = context;

        public async Task<BaseResponse<List<LeaveTypeList>>> LeaveTypeListAsync(BasePaging paging)
        {
            var leaveType = _context.MstLeaveTypes.Where(leaveType => !leaveType.IsDeleted)
                .Select(leaveType => new LeaveTypeList
                {
                    Id = leaveType.Id,
                    TypeName = leaveType.TypeName,
                    Created = leaveType.Created,
                    CreatedBy = leaveType.CreatedBy
                });

            var columnMappings = new Dictionary<string, Expression<Func<LeaveTypeList, object?>>>
            {
                ["Id"] = department => department.Id,
                ["TypeName"] = department => department.TypeName,
                ["Created"] = department => department.Created,
                ["CreatedBy"] = department => department.CreatedBy
            };

            // Apply sorting based on the provided column and sort direction
            if (paging.Column is not null && columnMappings.TryGetValue(paging.Column, out var keySelector))
            {
                leaveType = paging.SortBy == Const.PAGING_SORT_ASC ? leaveType.OrderBy(keySelector) : leaveType.OrderByDescending(keySelector);
            }

            // Get the total count of departments before applying pagination
            var total = await leaveType.CountAsync();

            // Apply pagination using Skip and Take
            var result = await leaveType.Skip(paging.Start).Take(paging.Length).ToListAsync();

            return new BaseResponse<List<LeaveTypeList>>
            {
                Total = total,
                Result = result
            };
        }
    }
}
