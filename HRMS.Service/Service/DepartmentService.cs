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
            var response = new BaseResponse<List<DepartmentList>>();

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

            response.Total = total;
            response.Result = result;
            return response;
        }

        public async Task<BaseResponse<bool>> CreateDepartmentAsync(CreateDepartment request)
        {
            var response = new BaseResponse<bool>();

            // Trim the department name to remove any leading or trailing whitespace
            if (string.IsNullOrWhiteSpace(request.DepartmentName))
            {
                response.Result = false;
                response.Message = "Department name cannot be empty.";
                return response;
            }

            var departmentName = request.DepartmentName.Trim();

            // Check if the department name already exists and is not deleted
            var alreadyExists = await _context.MstDepartments.AnyAsync(department => !department.IsDeleted 
                && department.DepartmentName == departmentName);

            if (alreadyExists)
            {
                response.Result = false;
                response.Message = "Department name already exists.";
                return response;
            }

            var department = new MstDepartment
            {
                DepartmentName = departmentName,
                Created = DateTime.UtcNow,
                CreatedBy = "System"
            };

            _context.MstDepartments.Add(department);
            await _context.SaveChangesAsync();

            response.Result = true;
            response.Message = "Department Added Succesfull";
            return response;
        }

        public async Task<BaseResponse<bool>> UpdateDepartmentAsync(UpdateDepartment request)
        {
            var response = new BaseResponse<bool>();

            if (string.IsNullOrWhiteSpace(request.DepartmentName))
            {
                response.Result = false;
                response.Message = "Department name cannot be empty.";
                return response;
            }

            var departmentName = request.DepartmentName.Trim();

            // Check if the department exists and is not deleted
            var department = await _context.MstDepartments.FirstOrDefaultAsync(item => item.Id == request.Id && !item.IsDeleted);
            if (department == null)
            {
                response.Result = false;
                response.Message = "Department not found";
                return response;
            }

            // Check if the new department name already exists for a different department
            var alreadyExists = await _context.MstDepartments.AnyAsync(item => item.Id != request.Id && 
                !item.IsDeleted && item.DepartmentName == departmentName);

            if (alreadyExists)
            {
                response.Result = false;
                response.Message = "Department name already exists.";
                return response;
            }

            department.DepartmentName = departmentName;
            department.Updated = DateTime.UtcNow;
            department.UpdatedBy = "System"; 

            await _context.SaveChangesAsync();

            response.Result = true;
            response.Message = "Department updated successfully.";
            return response;
        }

        public async Task<BaseResponse<bool>> DeleteDepartmentAsync(int id)
        {
            var response = new BaseResponse<bool>();

            // Check if the department exists and is not deleted
            var department = await _context.MstDepartments.FirstOrDefaultAsync(item =>
                item.Id == id && !item.IsDeleted);
            if (department == null)
            {
                response.Result = true;
                response.Message = "Department not found";
                return response;
            }

            // Block deleting a department that still has active employees
            var hasActiveEmployees = await _context.MstEmployees.AnyAsync(e =>
                !e.IsDeleted && e.DepartmentId == id);

            if (hasActiveEmployees)
            {
                response.Result = false;
                response.Message = "Cannot delete: this department still has active employees.";
                return response;
            }

            department.IsDeleted = true;
            department.Updated = DateTime.UtcNow;
            department.UpdatedBy = "System";

            await _context.SaveChangesAsync();

            response.Result = true;
            response.Message = "Department deleted successfully.";
            return response;
        }
    }

}