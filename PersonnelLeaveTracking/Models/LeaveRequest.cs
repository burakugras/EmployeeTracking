using PersonnelLeaveTracking.Enums;
using PersonnelLeaveTracking.Models;

public class LeaveRequest
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public LeaveStatus Status { get; set; }
    public string? ApprovedByManager { get; set; }
    public string? ApprovedByHRManager { get; set; }
}
