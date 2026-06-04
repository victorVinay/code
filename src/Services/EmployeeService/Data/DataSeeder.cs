using EmployeeService.Data;
using EmployeeService.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Common;

namespace EmployeeService.Data;

public static class DbSeeder
{
    public static async Task SeedAdminAsync(EmployeeDbContext context)
    {
        await context.Database.MigrateAsync();

        // =========================
        // 1. Seed Departments
        // =========================
        var defaultDepartments = new List<Department>
        {
            new Department
            {
                Name = "Human Resources",
                Description = "HR department",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Department
            {
                Name = "Information Technology",
                Description = "IT department",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Department
            {
                Name = "Finance",
                Description = "Finance and accounting",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Department
            {
                Name = "Operations",
                Description = "Operations and logistics",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        foreach (var dept in defaultDepartments)
        {
            var exists = await context.Departments
                .AnyAsync(d => d.Name == dept.Name);

            if (!exists)
            {
                dept.Id = Guid.NewGuid();
                context.Departments.Add(dept);
            }
        }

        await context.SaveChangesAsync();

        if (await context.Employees.AnyAsync(x => x.Role == Roles.Admin))
            return;

        var email = Environment.GetEnvironmentVariable("Admin__Email");
        var password = Environment.GetEnvironmentVariable("Admin__Password");

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            throw new Exception("Admin env variables not configured");

        var admin = new Employee
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = "System",
            LastName = "Admin",
            Role = Roles.Admin,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Employees.Add(admin);
        await context.SaveChangesAsync();
    }
}