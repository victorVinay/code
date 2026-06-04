namespace Shared.DTOs;

public class LeaveBalanceDto
{
    public string LeaveType { get; set; }
    public int Allocated { get; set; }
    public int Used { get; set; }
    public int Remaining { get; set; }
}