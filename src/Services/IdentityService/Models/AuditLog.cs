using System;

namespace IdentityService.Models
{
    public class AuditLog
    {
        public Guid UUID { get; set; }
        public Guid UserId { get; set; }
        public string Action { get; set; }
        public string IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation property
        public User User { get; set; }
    }
}
