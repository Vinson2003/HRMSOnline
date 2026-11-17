using HRMS.Data.Context;
using HRMS.Data.Entity;
using HRMS.Service.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Service.Service
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(HRMSContext context, string? demoPassword)
        {
            // Without a password we cannot create accounts
            if (string.IsNullOrWhiteSpace(demoPassword))
            {
                return;
            }

            var departmentId = await EnsureDepartmentAsync( context, "Administration");

            // Admin account
            await EnsureAccountAsync(context, new SeedAccount
            {
                Username = "admin",
                RoleName = "Admin",
                EmployeeNumber = "EMP-DEMO-0001",
                FullName = "Admin Demo",
                Email = "admin@demo.local",
                DepartmentId = departmentId
            }, demoPassword);

            // HR account
            await EnsureAccountAsync(context, new SeedAccount
            {
                Username = "hr",
                RoleName = "HR",
                EmployeeNumber = "EMP-DEMO-0002",
                FullName = "HR Demo",
                Email = "hr@demo.local",
                DepartmentId = departmentId
            }, demoPassword);

            // Manager account
            var managerEmployeeId = await EnsureAccountAsync(
                context, new SeedAccount
                {
                    Username = "manager",
                    RoleName = "Manager",
                    EmployeeNumber = "EMP-DEMO-0003",
                    FullName = "Manager Demo",
                    Email = "manager@demo.local",
                    DepartmentId = departmentId
                }, demoPassword);

            // Employee account
            await EnsureAccountAsync(context, new SeedAccount
            {
                Username = "employee",
                RoleName = "Employee",
                EmployeeNumber = "EMP-DEMO-0004",
                FullName = "Employee Demo",
                Email = "employee@demo.local",
                DepartmentId = departmentId,
                ManagerId = managerEmployeeId
            }, demoPassword);
        }

        // Find the department by name, or create it
        private static async Task<int> EnsureDepartmentAsync(HRMSContext context, string name)
        {
            var department = await context.MstDepartments.FirstOrDefaultAsync(d => d.DepartmentName == name && !d.IsDeleted);

            if (department != null)
            {
                return department.Id;
            }

            department = new MstDepartment
            {
                DepartmentName = name,
                Created = DateTime.UtcNow,
                CreatedBy = "System"
            };

            context.MstDepartments.Add(department);
            await context.SaveChangesAsync();

            return department.Id;
        }

        // Make sure one account (and its employee) exists.
        // Returns the employee id.
        private static async Task<int?> EnsureAccountAsync(HRMSContext context, SeedAccount account, string password)
        {
            // 1. The account already exists: nothing to create
            var existingUser = await context.MstUsers.FirstOrDefaultAsync(u => u.Username == account.Username);

            if (existingUser != null)
            {
                return existingUser.EmployeeId;
            }

            // 2. Find the role
            var role = await context.MstRoles.FirstOrDefaultAsync(r => r.RoleName == account.RoleName);

            if (role == null)
            {
                return null;
            }

            // 3. Find the employee by email, or create it
            var employee = await context.MstEmployees.FirstOrDefaultAsync(e => e.Email == account.Email);

            if (employee == null)
            {
                employee = new MstEmployee
                {
                    EmployeeNumber = account.EmployeeNumber,
                    FullName = account.FullName,
                    Email = account.Email,
                    DepartmentId = account.DepartmentId,
                    ManagerId = account.ManagerId,
                    AnnualLeaveBalance = 12,
                    Created = DateTime.UtcNow,
                    CreatedBy = "System"
                };

                context.MstEmployees.Add(employee);
                await context.SaveChangesAsync();
            }

            // 4. Create the account and hash the password
            var user = new MstUser
            {
                Username = account.Username,
                EmployeeId = employee.Id,
                RoleId = role.Id,
                IsActive = true,
                Created = DateTime.UtcNow,
                CreatedBy = "System"
            };

            user.PasswordHash = new PasswordHasher<MstUser>().HashPassword(user, password);

            context.MstUsers.Add(user);
            await context.SaveChangesAsync();

            return employee.Id;
        }
    }
}