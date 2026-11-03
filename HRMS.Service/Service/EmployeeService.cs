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
            // EmployeeService: ganti query awal
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

            // If no sorting is applied, ensure a default order to avoid issues with pagination
            if (!employees.Expression.ToString().Contains("OrderBy")) employees = employees.OrderBy(e => e.Id);

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
            var employeeNumber = request.EmployeeNumber.Trim();
            var email = request.Email.Trim();
            var fullName = request.FullName.Trim();

            // Check if the employee number already exists in the database
            var alreadyExists = await _context.MstEmployees.AnyAsync(e => e.EmployeeNumber == employeeNumber);
            if (alreadyExists)
            {
                return new BaseResponse<bool>
                {
                    Result = false,
                    Message = "Employee number already exists."
                };
            }

            var newEmployee = new MstEmployee
            {
                EmployeeNumber = employeeNumber,
                FullName = fullName,
                Email = email,
                DepartmentId = request.DepartmentId,
                ManagerId = request.ManagerId,
                AnnualLeaveBalance = request.AnnualLeaveBalance,
                Created = DateTime.UtcNow,
                CreatedBy = "System" // You can replace "System" with the actual user who is creating the employee if available
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
            if (employee == null || employee.IsDeleted)
            {
                return new BaseResponse<bool>
                {
                    Result = false,
                    Message = "Employee not found."
                };
            }

            var employeeNumber = request.EmployeeNumber.Trim();

            if (await _context.MstEmployees.AnyAsync(e => e.Id != request.Id && e.EmployeeNumber == employeeNumber))
            {
                return new BaseResponse<bool> { Result = false, Message = "Employee number already exists." };
            }

            if (request.ManagerId == request.Id)
            {
                return new BaseResponse<bool> { Result = false, Message = "Employee cannot be their own manager." };
            }

            employee.EmployeeNumber = request.EmployeeNumber.Trim();
            employee.FullName = request.FullName.Trim();
            employee.Email = request.Email.Trim();
            employee.DepartmentId = request.DepartmentId;
            employee.ManagerId = request.ManagerId;
            employee.AnnualLeaveBalance = request.AnnualLeaveBalance;
            employee.Updated = DateTime.UtcNow;
            employee.UpdatedBy = "System"; // You can replace "System" with the actual user who is updating the employee if available

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

            employee.IsDeleted = true;
            employee.Updated = DateTime.UtcNow;
            employee.UpdatedBy = "System"; // You can replace "System" with the actual user who is deleting the employee if available

            await _context.SaveChangesAsync();

            return new BaseResponse<bool>
            {
                Result = true,
                Message = "Employee deleted successfully."
            };
        }
    }
}
