
using LeaveService.Models;

namespace LeaveService.Repositories.Interface;

public interface ILeaveTypeRepository
{
    Task<List<LeaveType>> GetAllAsync();
    Task<LeaveType?> GetByIdAsync(Guid id);
}