using IdentityService.Models;

namespace IdentityService.Repositories.Interfaces
{
    public interface IAuditLogRepository : IRepository<AuditLog>
    {
        Task<IEnumerable<AuditLog>> GetAuditLogsByUserIdAsync(Guid userId);
    }
}
