namespace Shared.DTOs;

public class CreateTeamDto
{
    public string Name { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid? ManagerId { get; set; }
}