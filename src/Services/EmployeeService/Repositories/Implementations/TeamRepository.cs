

using EmployeeService.Data;
using EmployeeService.Models;
using EmployeeService.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace EmployeeService.Repositories.Implementations;

public class TeamRepository : ITeamRepository
{
    private readonly EmployeeDbContext _context;

    public TeamRepository(EmployeeDbContext context)
    {
        _context = context;
    }

    public async Task<Team?> GetByIdAsync(Guid id)
    {
        return await _context.Teams
            .Include(t => t.Members)
            .Include(t => t.Manager)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<Team>> GetAllAsync()
    {
        return await _context.Teams.ToListAsync();
    }

    public async Task AddAsync(Team team)
    {
        await _context.Teams.AddAsync(team);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Team team)
    {
        _context.Teams.Update(team);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var team = await _context.Teams.FindAsync(id);
        if (team == null) return;

        _context.Teams.Remove(team);
        await _context.SaveChangesAsync();
    }

    public async Task<Team?> GetTeamWithDetailsAsync(Guid teamId)
    {
        return await _context.Teams
            .Include(x => x.Manager)
            .Include(x => x.Members)
            .FirstOrDefaultAsync(x => x.Id == teamId);
    }
}