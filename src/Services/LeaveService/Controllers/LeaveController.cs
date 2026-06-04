using LeaveService.DTOs;
using LeaveService.Models;
using LeaveService.Repositories.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Enums;
using System.Security.Claims;
using Shared.DTOs;
using Shared.Common;
using LeaveService.Client;

namespace LeaveService.Controllers;


[ApiController]
[Route("api/leaves")]
[Authorize]
public class LeaveController : ControllerBase
{
    private readonly ILeaveRepository _leaveRepo;
    private readonly ILeaveTypeRepository _typeRepo;
    private readonly EmployeeClient _employeeClient;

    public LeaveController(ILeaveRepository leaveRepository,
    ILeaveTypeRepository typeRepository,
    EmployeeClient employeeClient)
    {
        _leaveRepo = leaveRepository;
        _typeRepo = typeRepository;
        _employeeClient = employeeClient;
    }



    [HttpPost]
    public async Task<IActionResult> SubmitLeave([FromBody] CreateLeaveDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdClaim, out var employeeId))
            {
                return Unauthorized("Invalid user id in token");
            }
            var name = User.FindFirst(ClaimTypes.Name)?.Value ?? "";
            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";

            var totalDays = (dto.EndDate - dto.StartDate).Days + 1;

            if (totalDays <= 0)
                return BadRequest("Invalid date range");

            var leave = new LeaveRequest
            {
                EmployeeId = employeeId,
                EmployeeName = name,
                EmployeeEmail = email,
                LeaveTypeId = dto.LeaveTypeId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                TotalDays = totalDays,
                Reason = dto.Reason,
                Status = LeaveStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _leaveRepo.CreateAsync(leave);

            return Ok(new
            {
                result.Id,
                result.Status
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SubmitLeave: {ex.Message}");
            return StatusCode(500, "An error occurred while submitting the leave request.");
        }

    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyLeaves()
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdClaim, out var employeeId))
        {
            return Unauthorized("Invalid user id in token");
        }
        var name = User.FindFirst(ClaimTypes.Name)?.Value ?? "";
        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";

        var leaves = await _leaveRepo.GetByEmployeeIdAsync(employeeId);

        var response = leaves.Select(x => new
        {
            x.Id,
            x.EmployeeId,
            x.EmployeeName,
            x.EmployeeEmail,
            x.LeaveTypeId,
            LeaveTypeName = x.LeaveType.Name,
            x.StartDate,
            x.EndDate,
            x.TotalDays,
            x.Reason,
            Status = x.Status.ToString(),
            x.CreatedAt
        });

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var leave = await _leaveRepo.GetByIdAsync(id);

        if (leave == null)
            return NotFound();

        return Ok(new
        {
            leave.Id,
            leave.EmployeeId,
            leave.EmployeeName,
            leave.EmployeeEmail,
            leave.LeaveTypeId,
            LeaveTypeName = leave.LeaveType.Name,
            leave.StartDate,
            leave.EndDate,
            leave.TotalDays,
            leave.Reason,
            Status = leave.Status.ToString(),
            leave.CreatedAt
        });
    }

    [HttpGet("/api/leave-types")]
    public async Task<IActionResult> GetLeaveTypes()
    {
        var types = await _typeRepo.GetAllAsync();

        var response = types.Select(x => new
        {
            x.Id,
            x.Name,
            x.Description,
            x.DefaultDays
        });

        return Ok(response);
    }

    [HttpGet("me/balance")]
    public async Task<IActionResult> GetBalance()
    {
        var employeeId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(employeeId, out var id))
            return Unauthorized("Invalid token");

        var result = await _leaveRepo.GetLeaveBalanceAsync(id);

        return Ok(result);
    }

    [HttpPost("approve")]
    public async Task<IActionResult> ApproveLeave([FromBody] ApproveLeaveDto dto)
    {
        var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(managerId, out var mgrId))
            return Unauthorized();

        var leave = await _leaveRepo.GetByIdAsync(dto.LeaveRequestId);

        if (leave == null)
            return NotFound("Leave request not found");

        var token = Request.Headers["Authorization"]
    .ToString()
    .Replace("Bearer ", "");

        var actualManagerId = await _employeeClient
            .GetManagerId(leave.EmployeeId, token);

        if (actualManagerId == null)
            return BadRequest("Employee has no manager");

        // 🔐 VALIDATION
        if (User.IsInRole("Manager") && actualManagerId != mgrId)
            return Forbid("Not your employee");

        // approve/reject
        leave.Status = dto.Approve
            ? LeaveStatus.Approved
            : LeaveStatus.Rejected;

        if (!dto.Approve)
            leave.RejectionReason = dto.Comment;

        leave.UpdatedAt = DateTime.UtcNow;

        await _leaveRepo.UpdateAsync(leave);

        return Ok(ApiResponseFactory.Success<object>(null, $"Leave request {(dto.Approve ? "approved" : "rejected")}"));
    }

}


