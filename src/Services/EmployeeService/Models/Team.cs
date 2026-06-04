namespace EmployeeService.Models;

public class Team
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }

    public Guid DepartmentId { get; set; }
    public Department Department { get; set; }

    public Guid? ManagerId { get; set; } // optional
    public Employee Manager { get; set; }

    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Employee> Members { get; set; }
}