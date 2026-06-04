namespace EmployeeService.Models;
public class AuditLog
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; }

    public string Action { get; set; }
    public string IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
}