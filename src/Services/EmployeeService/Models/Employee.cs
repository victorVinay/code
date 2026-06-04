

namespace EmployeeService.Models;

public class Employee
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string? Phone { get; set; }
    public string? JobTitle { get; set; }
    public string Role { get; set; }

    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public Guid? TeamId { get; set; }
    public Team? Team { get; set; }

    public bool IsActive { get; set; }
    public DateTime? HireDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; }
    public ICollection<AuditLog> AuditLogs { get; set; }

    public ICollection<Team> ManagedTeams { get; set; } // inverse for manager
}