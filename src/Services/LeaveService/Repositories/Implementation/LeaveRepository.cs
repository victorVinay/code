using LeaveService.Data;
using LeaveService.Models;
using LeaveService.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs;
using Shared.Enums;

namespace LeaveService.Repositories.Implementation;

public class LeaveRepository : ILeaveRepository
{
    private readonly LeaveDbContext _context;

    public LeaveRepository(LeaveDbContext context)
    {
        _context = context;
    }

    public async Task<LeaveRequest> CreateAsync(LeaveRequest request)
    {
        _context.LeaveRequests.Add(request);
        await _context.SaveChangesAsync();
        return request;
    }

    public async Task<LeaveRequest?> GetByIdAsync(Guid id)
    {
        return await _context.LeaveRequests
            .Include(x => x.LeaveType)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<LeaveRequest>> GetByEmployeeIdAsync(Guid employeeId)
    {
        return await _context.LeaveRequests
            .Include(x => x.LeaveType)
            .Where(x => x.EmployeeId == employeeId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<LeaveBalanceDto>> GetLeaveBalanceAsync(Guid employeeId)
    {
        var leaveTypes = await _context.LeaveTypes
            .Where(x => x.IsActive)
            .ToListAsync();

        var approvedLeaves = await _context.LeaveRequests
            .Where(x => x.EmployeeId == employeeId &&
                        x.Status == LeaveStatus.Approved)
            .ToListAsync();

        var result = new List<LeaveBalanceDto>();

        foreach (var type in leaveTypes)
        {
            var used = approvedLeaves
                .Where(x => x.LeaveTypeId == type.Id)
                .Sum(x => x.TotalDays);

            result.Add(new LeaveBalanceDto
            {
                LeaveType = type.Name,
                Allocated = type.DefaultDays,
                Used = used,
                Remaining = Math.Max(type.DefaultDays - used, 0)
            });
        }

        return result;
    }

    public async Task UpdateAsync(LeaveRequest request)
    {
        _context.LeaveRequests.Update(request);
        await _context.SaveChangesAsync();
    }
}