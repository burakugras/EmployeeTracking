using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonnelLeaveTracking.Data;
using PersonnelLeaveTracking.Enums;
using PersonnelLeaveTracking.Models;

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
        [Authorize(Roles = "Manager,HRManager")]
        public IActionResult GetAllLeaveRequests()
        {
            // Verileri sorgula
            var leaveRequests = _context.LeaveRequests
                .Include(lr => lr.Employee)
                .Select(lr => new
                {
                    lr.Id,
                    lr.StartDate,
                    lr.EndDate,
                    Status = lr.Status.ToString(),
                    lr.ApprovedBy,
                    Employee = new
                    {
                        lr.Employee.Id,
                        lr.Employee.FirstName,
                        lr.Employee.LastName,
                        lr.Employee.Email,
                        Title = lr.Employee.Title.ToString()
                    }
                }).ToList();


            var result = leaveRequests.Select(lr => new
            {
                lr.Id,
                lr.StartDate,
                lr.EndDate,
                lr.Status,
                ApprovedBy = !string.IsNullOrEmpty(lr.ApprovedBy)
                    ? $"{lr.ApprovedBy} ({GetApproverRole(lr.ApprovedBy)})"
                    : "Henüz onaylanmadı.",
                lr.Employee
            });

            return Ok(result);
        }

        private string GetApproverRole(string approvedByEmail)
        {
            var approver = _context.Employees.FirstOrDefault(e => e.Email == approvedByEmail);
            return approver != null ? approver.Title.ToString() : "Bilinmeyen Rol";
        }







        [HttpGet("employee/{employeeId}")]
        [Authorize]
        public IActionResult GetLeaveRequestsByEmployee(int employeeId)
        {
            var employee = _context.Employees.FirstOrDefault(e => e.Id == employeeId);

            if (employee == null)
                return NotFound("Çalışan bulunamadı.");

            var leaveRequests = _context.LeaveRequests
                .Where(lr => lr.EmployeeId == employeeId)
                .Select(lr => new
                {
                    lr.Id,
                    lr.StartDate,
                    lr.EndDate,
                    Status = lr.Status.ToString(),
                    ApprovedBy = string.IsNullOrEmpty(lr.ApprovedBy) ? "Henüz onaylanmadı." : lr.ApprovedBy
                }).ToList();

            return Ok(leaveRequests);
        }

        [HttpPost]
        [Authorize]
        public IActionResult CreateLeaveRequest(LeaveRequest leaveRequest)
        {
            var email = User.Identity?.Name;
            var employee = _context.Employees.FirstOrDefault(e => e.Email == email);

            if (employee == null)
                return Unauthorized("Çalışan bulunamadı.");

            if (leaveRequest.StartDate < DateTime.UtcNow)
                return BadRequest("Başlangıç tarihi geçmiş bir tarih olamaz.");

            if (leaveRequest.EndDate < leaveRequest.StartDate)
                return BadRequest("Bitiş tarihi, başlangıç tarihinden önce olamaz.");

            var overlappingRequest = _context.LeaveRequests.Any(lr =>
                lr.EmployeeId == employee.Id &&
                lr.Status == LeaveStatus.Approved &&
                lr.StartDate < leaveRequest.EndDate &&
                lr.EndDate > leaveRequest.StartDate);

            if (overlappingRequest)
                return BadRequest("Bu tarih aralığında zaten bir onaylanmış izin talebi mevcut.");

            leaveRequest.EmployeeId = employee.Id;
            leaveRequest.Status = LeaveStatus.Pending;

            _context.LeaveRequests.Add(leaveRequest);
            _context.SaveChanges();

            return Ok("İzin talebi başarıyla oluşturuldu.");
        }



        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Manager,HRManager")]
        public IActionResult ApproveLeaveRequest(int id)
        {
            var leaveRequest = _context.LeaveRequests
                                       .Include(lr => lr.Employee)
                                       .FirstOrDefault(lr => lr.Id == id);

            if (leaveRequest == null)
                return NotFound("İzin talebi bulunamadı.");

            if (leaveRequest.Status != LeaveStatus.Pending)
                return BadRequest("Bu izin talebi zaten işlem görmüş.");

            var totalLeaveDays = (leaveRequest.EndDate - leaveRequest.StartDate).Days + 1;

            if (leaveRequest.Employee.RemainingLeaves < totalLeaveDays)
                return BadRequest("Çalışanın yeterli izin hakkı bulunmuyor.");

            leaveRequest.Status = LeaveStatus.Approved;
            leaveRequest.ApprovedBy = User.Identity?.Name;
            leaveRequest.Employee.RemainingLeaves -= totalLeaveDays;

            _context.SaveChanges();
            return Ok("İzin talebi onaylandı.");
        }

        [HttpPut("{id}/reject")]
        [Authorize(Roles = "Manager,HRManager")]
        public IActionResult RejectLeaveRequest(int id)
        {
            var leaveRequest = _context.LeaveRequests
                                       .Include(lr => lr.Employee)
                                       .FirstOrDefault(lr => lr.Id == id);

            if (leaveRequest == null)
                return NotFound("İzin talebi bulunamadı.");

            if (leaveRequest.Status != LeaveStatus.Pending)
                return BadRequest("Bu izin talebi zaten işlem görmüş.");

            leaveRequest.Status = LeaveStatus.Rejected;
            leaveRequest.ApprovedBy = User.Identity?.Name;

            _context.SaveChanges();
            return Ok("İzin talebi reddedildi.");
        }
    }
}
