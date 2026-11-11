using HRMS.Data.Context;
using HRMS.LIB;
using HRMS.Service.Base;
using HRMS.Service.Interface;
using HRMS.Service.Models;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Azure;
using HRMS.Data.Entity;

namespace HRMS.Service.Service
{
    public class LeaveTypeService(HRMSContext context) : ILeaveTypeService
    {
        private readonly HRMSContext _context = context;

        public async Task<BaseResponse<List<LeaveTypeList>>> LeaveTypeListAsync(BasePaging paging)
        {
            var response = new BaseResponse<List<LeaveTypeList>>();

            var leaveTypes = _context.MstLeaveTypes.Where(leaveType => !leaveType.IsDeleted)
                .Select(leaveType => new LeaveTypeList
                {
                    Id = leaveType.Id,
                    TypeName = leaveType.TypeName,
                    RequiresAttachment = leaveType.RequiresAttachment,
                    Created = leaveType.Created,
                    CreatedBy = leaveType.CreatedBy
                });

            var columnMappings = new Dictionary<string, Expression<Func<LeaveTypeList, object?>>>
            {
                ["Id"] = leaveType => leaveType.Id,
                ["TypeName"] = leaveType => leaveType.TypeName,
                ["RequiresAttachment"] = leaveType => leaveType.RequiresAttachment,
                ["Created"] = leaveType => leaveType.Created,
                ["CreatedBy"] = leaveType => leaveType.CreatedBy
            };

            // Apply sorting based on the provided column and sort direction
            if (paging.Column is not null && columnMappings.TryGetValue(paging.Column, out var keySelector))
            {
                leaveTypes = paging.SortBy == Const.PAGING_SORT_ASC ? leaveTypes.OrderBy(keySelector) : leaveTypes.OrderByDescending(keySelector);
            }
            else
            {
                // Default order so paging is stable
                leaveTypes = leaveTypes.OrderBy(e => e.Id);
            }

            // Get the total count of leavetype before applying pagination
            var total = await leaveTypes.CountAsync();

            // Apply pagination using Skip and Take
            var result = await leaveTypes.Skip(paging.Start).Take(paging.Length).ToListAsync();

            response.Total = total;
            response.Result = result;
            return response;
        }

        public async Task<BaseResponse<bool>> CreateLeaveTypeAsync(CreateLeaveType request)
        {
            var response = new BaseResponse<bool>();

            if (string.IsNullOrWhiteSpace(request.TypeName))
            {
                response.Result = false;
                response.Message = "Type name is required.";
                return response;
            }

            var typeName = request.TypeName.Trim();

            if (typeName.Length > 50)
            {
                response.Result = false;
                response.Message = "Type name cannot exceed 50 characters.";
                return response;
            }

            if (await _context.MstLeaveTypes.AnyAsync(e => e.TypeName == typeName && !e.IsDeleted))
            {
                response.Result = false;
                response.Message = "Type Name already exists.";
                return response;
            }

            var newLeaveType = new MstLeaveType
            {
                TypeName = typeName,
                RequiresAttachment = request.RequiresAttachment,
                Created = DateTime.UtcNow,
                CreatedBy = "System"
            };

            _context.MstLeaveTypes.Add(newLeaveType);
            await _context.SaveChangesAsync();

            response.Result = true;
            response.Message = "LeaveType created successfully";
            return response;
        }

        public async Task<BaseResponse<bool>> UpdateLeaveTypeAsync(UpdateLeaveType request)
        {
            var response = new BaseResponse<bool>();

            var leaveType = await _context.MstLeaveTypes.FindAsync(request.Id);
            if (leaveType == null || leaveType.IsDeleted)
            {
                response.Result = false;
                response.Message = "Leave type not found.";
                return response;
            }

            if (string.IsNullOrWhiteSpace(request.TypeName))
            {
                response.Result = false;
                response.Message = "Type name is required.";
                return response;
            }

            var typeName = request.TypeName.Trim();

            if (typeName.Length > 50)
            {
                response.Result = false;
                response.Message = "Type name cannot exceed 50 characters.";
                return response;
            }

            if (await _context.MstLeaveTypes.AnyAsync(e => e.Id != request.Id && e.TypeName == typeName && !e.IsDeleted))
            {
                response.Result = false;
                response.Message = "Type Name already exists.";
                return response;
            }

            leaveType.TypeName = typeName;
            leaveType.RequiresAttachment = request.RequiresAttachment;
            leaveType.Updated = DateTime.UtcNow;
            leaveType.UpdatedBy = "System";

            await _context.SaveChangesAsync();

            response.Result = true;
            response.Message = "LeaveType updated successfully";
            return response;
        }

        public async Task<BaseResponse<bool>> DeleteLeaveTypeAsync(int leaveTypeId)
        {
            var response = new BaseResponse<bool>();

            var leaveType = await _context.MstLeaveTypes.FindAsync(leaveTypeId);
            if (leaveType == null || leaveType.IsDeleted)  
            {
                response.Result = false;
                response.Message = "Leave type not found.";
                return response;
            }

            // Block deleting a leave type that is used by existing leave requests
            var isUsed = await _context.TrxLeaveRequests.AnyAsync(r => r.LeaveTypeId == leaveTypeId && !r.IsDeleted);
            if (isUsed)
            {
                response.Result = false;
                response.Message = "Cannot delete: this leave type is used by existing leave requests.";
                return response;
            }

            leaveType.IsDeleted = true;
            leaveType.Updated = DateTime.UtcNow;
            leaveType.UpdatedBy = "System";

            await _context.SaveChangesAsync();

            response.Result = true;
            response.Message = "Leave type deleted successfully";
            return response;
        }
    }
}
