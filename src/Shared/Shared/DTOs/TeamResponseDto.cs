namespace Shared.DTOs;

public class TeamResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid? ManagerId { get; set; }
}