using EmployeeService.Data;
using EmployeeService.Models;
using EmployeeService.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace EmployeeService.Repositories.Implementations;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly EmployeeDbContext _context;

    public DepartmentRepository(EmployeeDbContext context)
    {
        _context = context;
    }

    public async Task<Department?> GetByIdAsync(Guid id)
    {
        return await _context.Departments
            .Include(d => d.Teams)
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<List<Department>> GetAllAsync()
    {
        return await _context.Departments.ToListAsync();
    }

    public async Task AddAsync(Department department)
    {
        await _context.Departments.AddAsync(department);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Department department)
    {
        _context.Departments.Update(department);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var dept = await _context.Departments.FindAsync(id);
        if (dept == null) return;

        _context.Departments.Remove(dept);
        await _context.SaveChangesAsync();
    }
}