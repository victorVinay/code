namespace Shared.DTOs;

public class UpdateTeamDto
{
    public string Name { get; set; }
    public Guid? ManagerId { get; set; }
}