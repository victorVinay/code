
using Microsoft.AspNetCore.Mvc;
using EmployeeService.Models;
using EmployeeService.Repositories.Interface;
using Microsoft.AspNetCore.Authorization;
using Shared.Common;
using Shared.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace EmployeeService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeRepository _repo;
    private readonly ITeamRepository _teamRepo;

    public EmployeeController(
        IEmployeeRepository repo,
        ITeamRepository teamRepo)
    {
        _repo = repo;
        _teamRepo = teamRepo;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("Invalid token: user id missing");

        if (!Guid.TryParse(userId, out var id))
            return BadRequest("Invalid user id format in token");

        var employee = await _repo.GetByIdAsync(id);

        if (employee == null)
            return NotFound();

        var dto = new EmployeeResponseDto
        {
            Id = employee.Id,
            Email = employee.Email,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Role = employee.Role,
            JobTitle = employee.JobTitle,
            DepartmentName = employee.Department?.Name
        };

        return Ok(ApiResponseFactory.Success(dto));
    }

    [Authorize(Roles = "Manager,Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _repo.GetAllAsync();

        var dtos = users.Select(u => new EmployeeResponseDto
        {
            Id = u.Id,
            Email = u.Email,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Role = u.Role,
            JobTitle = u.JobTitle
        }).ToList();

        return Ok(ApiResponseFactory.Success(dtos));
    }

    [Authorize(Roles = "Manager,Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _repo.GetByIdAsync(id);

        var dto = new EmployeeResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            JobTitle = user.JobTitle
        };

        return Ok(ApiResponseFactory.Success(dto));
    }

    [Authorize(Roles = "Manager,Admin")]
    [HttpPost("/api/employees")]
    public async Task<IActionResult> CreateEmployee(CreateEmployeeDto dto)
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Role = "Employee",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            DepartmentId = dto.DepartmentId,
            TeamId = dto.TeamId
        };

        await _repo.AddAsync(employee);

        return Ok(ApiResponseFactory.Success(new
        {
            employee.Id,
            employee.Email,
            employee.Role
        }));
    }

    [Authorize(Roles = "Manager,Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateEmployeeDto dto)
    {
        var user = await _repo.GetByIdAsync(id);
        if (user == null)
            return NotFound(ApiResponseFactory.Fail<object>("Not found", 404));

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.JobTitle = dto.JobTitle;

        await _repo.UpdateAsync(user);

        return Ok(ApiResponseFactory.Success<object>(null, "Updated"));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _repo.DeleteAsync(id);
        return Ok(ApiResponseFactory.Success<object>(null, "Deleted"));
    }

    [HttpPost("teams/{teamId}/add-employee/{employeeId}")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> AddEmployeeToTeam(Guid teamId, Guid employeeId)
    {
        var team = await _teamRepo.GetByIdAsync(teamId);
        if (team == null)
            return NotFound("Team not found");

        var employee = await _repo.GetByIdAsync(employeeId);
        if (employee == null)
            return NotFound("Employee not found");

        employee.TeamId = teamId;

        await _repo.UpdateAsync(employee);

        return Ok(ApiResponseFactory.Success<object>(null, "Employee added to team"));
    }

    [HttpGet("employee/{employeeId}/manager")]
    public async Task<IActionResult> GetEmployeeManager(Guid employeeId)
    {

        var employee = await _repo.GetByIdAsync(employeeId);

        if (employee == null)
            return NotFound();

        if (employee.Team == null || employee.Team.ManagerId == null)
            return Ok(null);

        return Ok(new
        {
            ManagerId = employee.Team.ManagerId
        });
    }
}

