using EmployeeService.Models;

namespace EmployeeService.Repositories.Interface;

public interface ITeamRepository
{
    Task<Team?> GetByIdAsync(Guid id);
    Task<List<Team>> GetAllAsync();

    Task AddAsync(Team team);
    Task UpdateAsync(Team team);
    Task DeleteAsync(Guid id);
    Task<Team?> GetTeamWithDetailsAsync(Guid teamId);
}