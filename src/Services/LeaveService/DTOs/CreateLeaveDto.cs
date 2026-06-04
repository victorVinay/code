using System.ComponentModel.DataAnnotations;

namespace LeaveService.DTOs;

public class CreateLeaveDto
{
    [Required]
    public Guid LeaveTypeId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Reason { get; set; } = string.Empty;
}