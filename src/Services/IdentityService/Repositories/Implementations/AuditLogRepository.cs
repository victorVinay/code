using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using IdentityService.Data;
using IdentityService.Models;
using IdentityService.Repositories.Interfaces;

namespace IdentityService.Repositories.Implementations
{
    public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(IdentityDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Where(al => al.UserId == userId)
                .OrderByDescending(al => al.CreatedAt)
                .ToListAsync();
        }
    }
}
