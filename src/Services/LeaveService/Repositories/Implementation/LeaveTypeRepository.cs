using LeaveService.Data;
using LeaveService.Models;
using LeaveService.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace LeaveService.Repositories.Implementation;

public class LeaveTypeRepository : ILeaveTypeRepository
{
    private readonly LeaveDbContext _context;

    public LeaveTypeRepository(LeaveDbContext context)
    {
        _context = context;
    }

    public async Task<List<LeaveType>> GetAllAsync()
    {
        return await _context.LeaveTypes
            .Where(x => x.IsActive)
            .ToListAsync();
    }

    public async Task<LeaveType?> GetByIdAsync(Guid id)
    {
        return await _context.LeaveTypes
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}