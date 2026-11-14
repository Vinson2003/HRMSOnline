using HRMS.Data.Context;
using HRMS.Data.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Service.Models
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(HRMSContext context, string? demoPassword)
        {
            // 1. Without a password we cannot create accounts
            if (string.IsNullOrWhiteSpace(demoPassword))
            {
                return;
            }

            // 2. Skip if the admin account already exists
            if (await context.MstUsers.AnyAsync(u => u.Username == "admin"))
            {
                return;
            }

            // 3. Find the Admin role
            var role = await context.MstRoles.FirstOrDefaultAsync(r => r.RoleName == "Admin");
            if (role == null)
            {
                return;
            }

            // 4. Use an existing employee as the owner of this account
            var employee = await context.MstEmployees.FirstOrDefaultAsync(e => !e.IsDeleted);
            if (employee == null)
            {
                return;
            }

            // 5. Create the user and hash the password
            var user = new MstUser
            {
                Username = "admin",
                EmployeeId = employee.Id,
                RoleId = role.Id,
                IsActive = true,
                Created = DateTime.UtcNow,
                CreatedBy = "System"
            };

            user.PasswordHash = new PasswordHasher<MstUser>().HashPassword(user, demoPassword);

            context.MstUsers.Add(user);
            await context.SaveChangesAsync();
        }
    }
}