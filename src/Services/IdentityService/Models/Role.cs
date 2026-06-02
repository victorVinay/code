using System;

namespace IdentityService.Models
{
    public class Role
    {
        public Guid UUID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
