using EmployeeService.Models;
using EmployeeService.Repositories.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Common;
using Shared.DTOs;

namespace EmployeeService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Manager,Admin")]
public class TeamController : ControllerBase
{
    private readonly ITeamRepository _repo;

    public TeamController(ITeamRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var teams = await _repo.GetAllAsync();

        var dtos = teams.Select(t => new TeamResponseDto
        {
            Id = t.Id,
            Name = t.Name,
            DepartmentId = t.DepartmentId,
            ManagerId = t.ManagerId
        });

        return Ok(ApiResponseFactory.Success(dtos));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTeamDto dto)
    {
        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            DepartmentId = dto.DepartmentId,
            ManagerId = dto.ManagerId
        };

        await _repo.AddAsync(team);

        var response = new TeamResponseDto
        {
            Id = team.Id,
            Name = team.Name,
            DepartmentId = team.DepartmentId,
            ManagerId = team.ManagerId
        };

        return Ok(ApiResponseFactory.Success(response, "Created", 201));
    }

    [HttpGet("teams/{teamId}")]
    [Authorize]
    public async Task<IActionResult> GetTeam(Guid teamId)
    {
        var team = await _repo.GetTeamWithDetailsAsync(teamId);

        if (team == null)
            return NotFound("Team not found");

        return Ok(ApiResponseFactory.Success(new
        {
            team.Id,
            team.Name,
            team.DepartmentId,

            Manager = team.Manager == null ? null : new
            {
                team.Manager.Id,
                team.Manager.FirstName,
                team.Manager.LastName
            },

            Employees = team.Members == null
                ? new List<EmployeeResponseDto>()
                : team.Members.Select(e => new EmployeeResponseDto
                {
                    Id = e.Id,
                    FirstName = e.FirstName,
                    LastName = e.LastName
                })
        }));
    }


}