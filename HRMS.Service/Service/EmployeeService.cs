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
                .Join(_context.MstDepartments, employee => employee.DepartmentId, department => department.Id, (employee, department) => new { Employee = employee, Department = department })
                .Where(x => !x.Employee.IsDeleted && (string.IsNullOrWhiteSpace(searchName) || x.Employee.FullName!.Contains(searchName)))
                .Select(x => new EmployeeList
                {
                    Id = x.Employee.Id,
                    EmployeeNumber = x.Employee.EmployeeNumber,
                    FullName = x.Employee.FullName,
                    Email = x.Employee.Email,
                    DepartmentId = x.Employee.DepartmentId,
                    ManagerId = x.Employee.ManagerId,
                    AnnualLeaveBalance = x.Employee.AnnualLeaveBalance,
                    Created = x.Employee.Created,
                    CreatedBy = x.Employee.CreatedBy
                });

            // Define a dictionary to map column names to their corresponding expressions
            var columnMappings = new Dictionary<string, Expression<Func<EmployeeList, object?>>>
            {
                ["Id"] = employee => employee.Id,
                ["EmployeeNumber"] = employee => employee.EmployeeNumber,
                ["FullName"] = employee => employee.FullName,
                ["Email"] = employee => employee.Email,
                ["DepartmentId"] = employee => employee.DepartmentId,
                ["ManagerId"] = employee => employee.ManagerId,
                ["AnnualLeaveBalance"] = employee => employee.AnnualLeaveBalance,
                ["Created"] = employee => employee.Created,
                ["CreatedBy"] = employee => employee.CreatedBy
            };

            // Apply sorting based on the provided column and sort direction
            if (paging.Column is not null && columnMappings.TryGetValue(paging.Column, out var keySelector))
            {
                employees = paging.SortBy == Const.PAGING_SORT_ASC ? employees.OrderBy(keySelector) : employees.OrderByDescending(keySelector);
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
            var employeeNumber = request.EmployeeNumber.Trim();

            // Check if the employee number already exists and is not deleted
            var alreadyExists = await _context.MstEmployees.AnyAsync(employee =>
                !employee.IsDeleted && employee.EmployeeNumber == employeeNumber);

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
                FullName = request.FullName,
                Email = request.Email,
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

            employee.EmployeeNumber = request.EmployeeNumber.Trim();
            employee.FullName = request.FullName;
            employee.Email = request.Email;
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
