using Microsoft.EntityFrameworkCore;
using IdentityService.Data;
using IdentityService.Models;
using IdentityService.Repositories.Interfaces;

namespace IdentityService.Repositories.Implementations
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(IdentityDbContext context) : base(context)
        {
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}
