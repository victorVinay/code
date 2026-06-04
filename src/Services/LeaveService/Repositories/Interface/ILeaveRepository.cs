using LeaveService.Models;
using Shared.DTOs;

namespace LeaveService.Repositories.Interface
{
    public interface ILeaveRepository
    {
        Task<LeaveRequest> CreateAsync(LeaveRequest request);
        Task<LeaveRequest?> GetByIdAsync(Guid id);
        Task<List<LeaveRequest>> GetByEmployeeIdAsync(Guid employeeId);
        Task<List<LeaveBalanceDto>> GetLeaveBalanceAsync(Guid employeeId);

        Task UpdateAsync(LeaveRequest request);
    }
}