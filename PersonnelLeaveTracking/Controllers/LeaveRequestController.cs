using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonnelLeaveTracking.Data;
using PersonnelLeaveTracking.Models;
using PersonnelLeaveTracking.Enums;

namespace PersonnelLeaveTracking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveRequestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LeaveRequestController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAllLeaveRequests()
        {
            var leaveRequests = _context.LeaveRequests
                                        .Include(lr => lr.Employee)
                                        .ThenInclude(e => e.Department)
                                        .Select(lr => new
                                        {
                                            lr.Id,
                                            lr.StartDate,
                                            lr.EndDate,
                                            Status = lr.Status.ToString(),
                                            ApprovedBy = lr.ApprovedBy == "string" || string.IsNullOrEmpty(lr.ApprovedBy)
                                                         ? "Not approved yet"
                                                         : lr.ApprovedBy,
                                            Employee = new
                                            {
                                                lr.Employee.Id,
                                                lr.Employee.FirstName,
                                                lr.Employee.LastName,
                                                Title = lr.Employee.Title.ToString(),
                                                Department = new
                                                {
                                                    lr.Employee.Department.Id,
                                                    lr.Employee.Department.Name
                                                }
                                            }
                                        })
                                        .ToList();

            return Ok(leaveRequests);
        }



        [HttpGet("employee/{employeeId}")]
        public IActionResult GetLeaveRequestsByEmployee(int employeeId)
        {
            var leaveRequests = _context.LeaveRequests
                                        .Where(lr => lr.EmployeeId == employeeId)
                                        .Include(lr => lr.Employee)
                                        .ThenInclude(e => e.Department)
                                        .Select(lr => new
                                        {
                                            lr.Id,
                                            lr.StartDate,
                                            lr.EndDate,
                                            Status = lr.Status.ToString(),
                                            ApprovedBy = lr.ApprovedBy == "string" || string.IsNullOrEmpty(lr.ApprovedBy)
                                                         ? "Not approved yet"
                                                         : lr.ApprovedBy,
                                            Employee = new
                                            {
                                                lr.Employee.Id,
                                                lr.Employee.FirstName,
                                                lr.Employee.LastName,
                                                Title = lr.Employee.Title.ToString(),
                                                Department = new
                                                {
                                                    lr.Employee.Department.Id,
                                                    lr.Employee.Department.Name
                                                }
                                            }
                                        })
                                        .ToList();

            if (!leaveRequests.Any())
                return NotFound("Bu çalışanın izin talebi bulunamadı.");

            return Ok(leaveRequests);
        }



        [HttpPost]
        public IActionResult CreateLeaveRequest(LeaveRequest leaveRequest)
        {
            var employee = _context.Employees
                                   .Include(e => e.Department)
                                   .FirstOrDefault(e => e.Id == leaveRequest.EmployeeId);

            if (employee == null)
                return NotFound("Çalışan bulunamadı.");

            var totalLeaveDays = (leaveRequest.EndDate - leaveRequest.StartDate).Days + 1;
            if (employee.RemainingLeaves < totalLeaveDays)
                return BadRequest("Yeterli izin hakkı bulunmuyor.");

            leaveRequest.EmployeeId = employee.Id;
            leaveRequest.Employee = null;

            leaveRequest.Status = LeaveStatus.Pending;

            _context.LeaveRequests.Add(leaveRequest);
            employee.RemainingLeaves -= totalLeaveDays;

            _context.SaveChanges();

            return CreatedAtAction(nameof(GetAllLeaveRequests), new { id = leaveRequest.Id }, leaveRequest);
        }


        [HttpPut("{id}")]
        public IActionResult UpdateLeaveRequestStatus(int id, [FromBody] string status)
        {
            if (!Enum.TryParse<LeaveStatus>(status, true, out var parsedStatus))
                return BadRequest("Geçersiz durum.");

            var leaveRequest = _context.LeaveRequests.Find(id);
            if (leaveRequest == null)
                return NotFound("İzin talebi bulunamadı.");

            leaveRequest.Status = parsedStatus;
            leaveRequest.ApprovedBy = User.Identity?.Name ?? "Manager";

            _context.SaveChanges();

            return NoContent();
        }


        [HttpDelete("{id}")]
        public IActionResult DeleteLeaveRequest(int id)
        {
            var leaveRequest = _context.LeaveRequests.Find(id);
            if (leaveRequest == null)
                return NotFound("İzin talebi bulunamadı.");

            _context.LeaveRequests.Remove(leaveRequest);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
