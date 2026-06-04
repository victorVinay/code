namespace Shared.DTOs;

public class ApproveLeaveDto
{
    public Guid LeaveRequestId { get; set; }
    public bool Approve { get; set; }
    public string? Comment { get; set; }
}