using HRMS.Data.Context;
using HRMS.Data.Entity;
using HRMS.LIB;
using HRMS.Service.Base;
using HRMS.Service.Interface;
using HRMS.Service.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Net.NetworkInformation;


namespace HRMS.Service.Service
{
    public class DepartmentService(HRMSContext context) : IDepartmentService
    {
        private readonly HRMSContext _context = context;

        public async Task<BaseResponse<List<DepartmentList>>> DepartmentsListAsync(BasePaging paging, string searchName)
        {
            var departments = _context.MstDepartments
                .Where(department => !department.IsDeleted && (string.IsNullOrWhiteSpace(searchName) || department.DepartmentName!.Contains(searchName)))
                .Select(department => new DepartmentList
                {
                    Id = department.Id,
                    DepartmentName = department.DepartmentName,
                    Created = department.Created,
                    CreatedBy = department.CreatedBy
                });

            // Define a dictionary to map column names to their corresponding expressions
            var columnMappings = new Dictionary<string, Expression<Func<DepartmentList, object?>>>
            {
                ["Id"] = department => department.Id,
                ["DepartmentName"] = department => department.DepartmentName,
                ["Created"] = department => department.Created,
                ["CreatedBy"] = department => department.CreatedBy
            };

            // Apply sorting based on the provided column and sort direction
            if (paging.Column is not null && columnMappings.TryGetValue(paging.Column, out var keySelector))
            {
                departments = paging.SortBy == Const.PAGING_SORT_ASC ? departments.OrderBy(keySelector) : departments.OrderByDescending(keySelector);
            }

            // Get the total count of departments before applying pagination
            var total = await departments.CountAsync();

            // Apply pagination using Skip and Take
            var result = await departments.Skip(paging.Start).Take(paging.Length).ToListAsync();

            return new BaseResponse<List<DepartmentList>>
            {
                Total = total,
                Result = result
            };
        }

        public async Task<BaseResponse<bool>> CreateDepartmentAsync(CreateDepartment request)
        {
            // Trim the department name to remove any leading or trailing whitespace
            if (string.IsNullOrWhiteSpace(request.DepartmentName))
            {
                return new BaseResponse<bool> { Result = false, Message = "Department name cannot be empty." };
            }

            var departmentName = request.DepartmentName.Trim();

            // Check if the department name already exists and is not deleted
            var alreadyExists = await _context.MstDepartments.AnyAsync(department =>
                !department.IsDeleted && department.DepartmentName == departmentName);

            if (alreadyExists)
            {
                return new BaseResponse<bool>
                {
                    Result = false,
                    Message = "Department name already exists."
                };
            }

            var department = new MstDepartment
            {
                DepartmentName = departmentName,
                Created = DateTime.UtcNow,
                CreatedBy = "System" // You can replace "System" with the actual user who is creating the employee if available
            };

            _context.MstDepartments.Add(department);
            await _context.SaveChangesAsync();

            return new BaseResponse<bool>
            {
                Result = true,
                Message = "Department added successfully."
            };
        }

        public async Task<BaseResponse<bool>> UpdateDepartmentAsync(UpdateDepartment request)
        {
            // Trim the department name to remove any leading or trailing whitespace
            if (string.IsNullOrWhiteSpace(request.DepartmentName))
            {
                return new BaseResponse<bool> { Result = false, Message = "Department name cannot be empty." };
            }

            var departmentName = request.DepartmentName.Trim();

            // Check if the department exists and is not deleted
            var department = await _context.MstDepartments.FirstOrDefaultAsync(item =>
                item.Id == request.Id && !item.IsDeleted);

            if (department is null)
            {
                return new BaseResponse<bool>
                {
                    Result = false,
                    Message = "Department not found."
                };
            }

            // Check if the new department name already exists for a different department
            var alreadyExists = await _context.MstDepartments.AnyAsync(item =>
                item.Id != request.Id && !item.IsDeleted && item.DepartmentName == departmentName);

            if (alreadyExists)
            {
                return new BaseResponse<bool>
                {
                    Result = false,
                    Message = "Department name already exists."
                };
            }

            department.DepartmentName = departmentName;
            department.Updated = DateTime.UtcNow;
            department.UpdatedBy = "System"; // You can replace "System" with the actual user who is creating the employee if available

            await _context.SaveChangesAsync();

            return new BaseResponse<bool>
            {
                Result = true,
                Message = "Department updated successfully."
            };
        }

        public async Task<BaseResponse<bool>> DeleteDepartmentAsync(int id)
        {
            // Check if the department exists and is not deleted
            var department = await _context.MstDepartments.FirstOrDefaultAsync(item =>
                item.Id == id && !item.IsDeleted);

            if (department is null)
            {
                return new BaseResponse<bool>
                {
                    Result = false,
                    Message = "Department not found."
                };
            }

            // Block deleting a department that still has active employees
            var hasActiveEmployees = await _context.MstEmployees.AnyAsync(e =>
                !e.IsDeleted && e.DepartmentId == id);

            if (hasActiveEmployees)
            {
                return new BaseResponse<bool>
                {
                    Result = false,
                    Message = "Cannot delete: this department still has active employees."
                };
            }

            department.IsDeleted = true;
            department.Updated = DateTime.UtcNow;
            department.UpdatedBy = "System"; // You can replace "System" with the actual user who is creating the employee if available

            await _context.SaveChangesAsync();

            return new BaseResponse<bool>
            {
                Result = true,
                Message = "Department deleted successfully."
            };
        }
    }

}