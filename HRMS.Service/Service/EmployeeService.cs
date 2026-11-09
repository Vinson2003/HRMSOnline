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
    public class EmployeeService(HRMSContext context) : IEmployeeService
    {
        private readonly HRMSContext _context = context;

        public async Task<BaseResponse<List<EmployeeList>>> EmployeeListAsync(BasePaging paging, string searchName)
        {
            var employees = _context.MstEmployees
                .Where(x => !x.IsDeleted && (string.IsNullOrWhiteSpace(searchName) || x.FullName.Contains(searchName)))
                .Select(x => new EmployeeList
                {
                    Id = x.Id,
                    EmployeeNumber = x.EmployeeNumber,
                    FullName = x.FullName,
                    Email = x.Email,
                    DepartmentId = x.DepartmentId,
                    DepartmentName = x.Department.DepartmentName,
                    ManagerId = x.ManagerId,
                    ManagerName = x.Manager != null ? x.Manager.FullName : null,
                    AnnualLeaveBalance = x.AnnualLeaveBalance,
                    Created = x.Created,
                    CreatedBy = x.CreatedBy
                });

            // Define a dictionary to map column names to their corresponding expressions
            var columnMappings = new Dictionary<string, Expression<Func<EmployeeList, object?>>>
            {
                ["Id"] = employee => employee.Id,
                ["EmployeeNumber"] = employee => employee.EmployeeNumber,
                ["FullName"] = employee => employee.FullName,
                ["Email"] = employee => employee.Email,
                ["DepartmentId"] = employee => employee.DepartmentId,
                ["DepartmentName"] = employee => employee.DepartmentName,
                ["ManagerId"] = employee => employee.ManagerId,
                ["ManagerName"] = employee => employee.ManagerName,
                ["AnnualLeaveBalance"] = employee => employee.AnnualLeaveBalance,
                ["Created"] = employee => employee.Created,
                ["CreatedBy"] = employee => employee.CreatedBy
            };

            // Apply sorting based on the provided column and sort direction
            if (paging.Column is not null && columnMappings.TryGetValue(paging.Column, out var keySelector))
            {
                employees = paging.SortBy == Const.PAGING_SORT_ASC ? employees.OrderBy(keySelector) : employees.OrderByDescending(keySelector);
            }
            else
            {
                // Default order so paging is stable
                employees = employees.OrderBy(e => e.Id);
            }

            // Get the total count of employees before applying pagination
            var total = await employees.CountAsync();

            // Apply pagination using Skip and Take
            var result = await employees.Skip(paging.Start).Take(paging.Length).ToListAsync();

            return new BaseResponse<List<EmployeeList>>
            {
                Total = total,
                Result = result
            };
        }

        public async Task<BaseResponse<bool>> CreateEmployeeAsync(CreateEmployee request)
        {
            // Trim the input values to remove leading and trailing whitespace
            var email = request.Email.Trim();
            var fullName = request.FullName.Trim();

            // Check if the email already exists in the database
            if (await _context.MstEmployees.AnyAsync(e => e.Email == email))
            {
                return new BaseResponse<bool> { Result = false, Message = "Email already exists." };
            }

            var newEmployee = new MstEmployee
            {
                EmployeeNumber = await GenerateEmployeeNumberAsync(),
                FullName = fullName,
                Email = email,
                DepartmentId = request.DepartmentId,
                ManagerId = request.ManagerId,
                AnnualLeaveBalance = request.AnnualLeaveBalance,
                Created = DateTime.UtcNow,
                CreatedBy = "System"
            };

            _context.MstEmployees.Add(newEmployee);
            await _context.SaveChangesAsync();

            return new BaseResponse<bool>
            {
                Result = true,
                Message = "Employee added successfully."
            };
        }

        public async Task<BaseResponse<bool>> UpdateEmployeeAsync(UpdateEmployee request)
        {
            var employee = await _context.MstEmployees.FindAsync(request.Id);

            // Check if the employee exists and is not deleted
            if (employee == null || employee.IsDeleted)
            {
                return new BaseResponse<bool>
                {
                    Result = false,
                    Message = "Employee not found."
                };
            }

            var email = request.Email.Trim();
            var fullName = request.FullName.Trim();

            // Check if the email already exists for another employee
            if (await _context.MstEmployees.AnyAsync(e => e.Id != request.Id && e.Email == email))
            {
                return new BaseResponse<bool> { Result = false, Message = "Email already exists." };
            }

            // Check if the employee is trying to set themselves as their own manager
            if (request.ManagerId == request.Id)
            {
                return new BaseResponse<bool> { Result = false, Message = "Employee cannot be their own manager." };
            }

            employee.FullName = fullName;
            employee.Email = email;
            employee.DepartmentId = request.DepartmentId;
            employee.ManagerId = request.ManagerId;
            employee.AnnualLeaveBalance = request.AnnualLeaveBalance;
            employee.Updated = DateTime.UtcNow;
            employee.UpdatedBy = "System"; 

            await _context.SaveChangesAsync();

            return new BaseResponse<bool>
            {
                Result = true,
                Message = "Employee updated successfully."
            };
        }

        public async Task<BaseResponse<bool>> DeleteEmployeeAsync(int employeeId)
        {
            var employee = await _context.MstEmployees.FindAsync(employeeId);
            if (employee == null || employee.IsDeleted)
            {
                return new BaseResponse<bool>
                {
                    Result = false,
                    Message = "Employee not found."
                };
            }

            // Check if the employee is a manager of other employees
            if (await _context.MstEmployees.AnyAsync(e => !e.IsDeleted && e.ManagerId == employeeId))
            {
                return new BaseResponse<bool>
                {
                    Result = false,
                    Message = "Cannot delete: this employee is still a manager of other employees."
                };
            }

            employee.IsDeleted = true;
            employee.Updated = DateTime.UtcNow;
            employee.UpdatedBy = "System"; 

            await _context.SaveChangesAsync();

            return new BaseResponse<bool>
            {
                Result = true,
                Message = "Employee deleted successfully."
            };
        }

        private async Task<string> GenerateEmployeeNumberAsync()
        {
            var prefix = $"EMP-{DateTime.UtcNow.Year}-";

            // No IsDeleted filter: the UNIQUE constraint in the database also counts deleted rows
            var lastNumber = await _context.MstEmployees
                .Where(e => e.EmployeeNumber.StartsWith(prefix))
                .OrderByDescending(e => e.EmployeeNumber)
                .Select(e => e.EmployeeNumber)
                .FirstOrDefaultAsync();

            var next = 1;
            if (lastNumber is not null && int.TryParse(lastNumber[prefix.Length..], out var current))
            {
                next = current + 1;
            }

            return $"{prefix}{next:D4}";   // EMP-2026-0001
        }
    }
}
