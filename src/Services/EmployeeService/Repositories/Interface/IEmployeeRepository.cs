using EmployeeService.Models;

namespace EmployeeService.Repositories.Interface;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(Guid id);
    Task<List<Employee>> GetAllAsync();
    Task<List<Employee>> GetByDepartmentAsync(Guid departmentId);

    Task AddAsync(Employee employee);
    Task UpdateAsync(Employee employee);
    Task DeleteAsync(Guid id);
}