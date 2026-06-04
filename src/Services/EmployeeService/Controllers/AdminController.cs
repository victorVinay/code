using EmployeeService.Models;
using EmployeeService.Repositories.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Common;
using Shared.DTOs;

namespace EmployeeService.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IEmployeeRepository _repo;
    private readonly ITeamRepository _teamRepo;

    public AdminController(
        IEmployeeRepository repo,
        ITeamRepository teamRepo)
    {
        _repo = repo;
        _teamRepo = teamRepo;
    }

    [HttpPost("create-manager")]
    public async Task<IActionResult> CreateManager(CreateManagerDto dto)
    {
        var manager = new Employee
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Role = "Manager",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            DepartmentId = dto.DepartmentId,
            IsActive = true
        };

        await _repo.AddAsync(manager);

        return Ok(ApiResponseFactory.Success(
            new { manager.Id, manager.Email, manager.Role },
            "Manager created"
        ));
    }

    [HttpPost("teams/{teamId}/assign-manager/{managerId}")]
    public async Task<IActionResult> AssignManager(Guid teamId, Guid managerId)
    {
        var team = await _teamRepo.GetByIdAsync(teamId);
        if (team == null)
            return NotFound("Team not found");

        var manager = await _repo.GetByIdAsync(managerId);
        if (manager == null)
            return NotFound("Employee not found");

        if (manager.Role != Roles.Manager && manager.Role != Roles.Admin)
            return BadRequest("Employee is not eligible to be a manager");

        team.ManagerId = managerId;

        await _teamRepo.UpdateAsync(team);

        return Ok("Manager assigned to team");
    }
}