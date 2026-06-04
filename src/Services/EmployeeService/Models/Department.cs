namespace EmployeeService.Models;

public class Department
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<Team> Teams { get; set; }
    public ICollection<Employee> Employees { get; set; }
}