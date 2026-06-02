using IdentityService.Data;
using IdentityService.Models;
using IdentityService.Repositories.Interfaces;

namespace IdentityService.Repositories.Implementations
{
    public class RoleRepository : Repository<Role>, IRoleRepository
    {
        public RoleRepository(IdentityDbContext context) : base(context)
        {
        }
    }
}
