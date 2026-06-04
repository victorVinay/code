namespace Shared.DTOs;

public class CreateEmployeeDto
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid? TeamId { get; set; }
}