using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonnelLeaveTracking.Data;
using PersonnelLeaveTracking.DTOs;
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
            var leaveRequests = _context.LeaveRequests
                .Include(lr => lr.Employee)
                .Select(lr => new
                {
                    lr.Id,
                    lr.StartDate,
                    lr.EndDate,
                    Status = lr.Status.ToString(),
                    ApprovedByManager = string.IsNullOrEmpty(lr.ApprovedByManager) ? "Henüz onaylanmadı." : lr.ApprovedByManager,
                    ApprovedByHRManager = string.IsNullOrEmpty(lr.ApprovedByHRManager) ? "Henüz onaylanmadı." : lr.ApprovedByHRManager,
                    Employee = new
                    {
                        lr.Employee.Id,
                        lr.Employee.FirstName,
                        lr.Employee.LastName,
                        lr.Employee.Email,
                        Title = lr.Employee.Title.ToString(),
                        RemainingLeaves = lr.Employee.RemainingLeaves
                    }
                }).ToList();

            return Ok(leaveRequests);
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
            ApprovedByManager = string.IsNullOrEmpty(lr.ApprovedByManager) ? "Henüz onaylanmadı." : lr.ApprovedByManager,
            ApprovedByHRManager = string.IsNullOrEmpty(lr.ApprovedByHRManager) ? "Henüz onaylanmadı." : lr.ApprovedByHRManager,
            Employee = new
            {
                lr.Employee.Id,
                lr.Employee.FirstName,
                lr.Employee.LastName,
                lr.Employee.Email,
                Title = lr.Employee.Title.ToString(),
                RemainingLeaves = lr.Employee.RemainingLeaves
            }
        }).ToList();

    return Ok(leaveRequests);
}

        [HttpPost]
        [Authorize]
        public IActionResult CreateLeaveRequest([FromBody] LeaveRequestDto leaveRequestDto)
        {
            var email = User.Identity?.Name;

            var employee = _context.Employees.FirstOrDefault(e => e.Email == email);
            if (employee == null)
            {
                return Unauthorized("Çalışan bulunamadı.");
            }

            if (leaveRequestDto.StartDate < DateTime.UtcNow)
            {
                return BadRequest(new { message = "Başlangıç tarihi geçmiş bir tarih olamaz." });
            }

            if (leaveRequestDto.EndDate < leaveRequestDto.StartDate)
            {
                return BadRequest(new { message = "Bitiş tarihi, başlangıç tarihinden önce olamaz." });
            }

            var overlappingRequest = _context.LeaveRequests.Any(lr =>
                lr.EmployeeId == employee.Id &&
                lr.Status == LeaveStatus.Approved &&
                lr.StartDate < leaveRequestDto.EndDate &&
                lr.EndDate > leaveRequestDto.StartDate);

            if (overlappingRequest)
            {
                return BadRequest(new { message = "Bu tarih aralığında zaten bir onaylanmış izin talebi mevcut." });
            }

            var leaveRequest = new LeaveRequest
            {
                EmployeeId = employee.Id,
                StartDate = leaveRequestDto.StartDate.ToUniversalTime(),
                EndDate = leaveRequestDto.EndDate.ToUniversalTime(),
                Status = LeaveStatus.Pending
            };

            _context.LeaveRequests.Add(leaveRequest);
            _context.SaveChanges();

            return Ok(new { message = "İzin talebi başarıyla oluşturuldu." });
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

            if (leaveRequest.Status == LeaveStatus.Approved)
                return BadRequest("Bu izin talebi zaten tamamen onaylanmış.");

            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var userEmail = User.Identity?.Name;

            var approver = _context.Employees.FirstOrDefault(e => e.Email == userEmail);
            if (approver == null)
                return Unauthorized("Onaylayan kişi bulunamadı.");

            var approverFullName = $"{approver.FirstName} {approver.LastName}";

            if (userEmail == leaveRequest.Employee.Email)
            {
                if (userRole == "Manager")
                {
                    leaveRequest.ApprovedByManager = approverFullName;
                }
                else if (userRole == "HRManager")
                {
                    leaveRequest.ApprovedByHRManager = approverFullName;
                    leaveRequest.Status = LeaveStatus.Approved;

                    var totalLeaveDays = (leaveRequest.EndDate - leaveRequest.StartDate).Days + 1;
                    if (leaveRequest.Employee.RemainingLeaves < totalLeaveDays)
                        return BadRequest("Çalışanın yeterli izin hakkı bulunmuyor.");

                    leaveRequest.Employee.RemainingLeaves -= totalLeaveDays;
                }
            }
            else
            {
                if (userRole == "Manager")
                {
                    if (leaveRequest.Employee.Title == EmployeeTitle.HRManager)
                        return BadRequest("Manager, HRManager taleplerini onaylayamaz.");

                    if (!string.IsNullOrEmpty(leaveRequest.ApprovedByManager))
                        return BadRequest("Bu izin talebi zaten bir Manager tarafından onaylandı.");

                    leaveRequest.ApprovedByManager = approverFullName;
                }
                else if (userRole == "HRManager")
                {
                    if (!string.IsNullOrEmpty(leaveRequest.ApprovedByHRManager))
                        return BadRequest("Bu izin talebi zaten bir HRManager tarafından onaylandı.");

                    leaveRequest.ApprovedByHRManager = approverFullName;

                    if (!string.IsNullOrEmpty(leaveRequest.ApprovedByManager))
                    {
                        leaveRequest.Status = LeaveStatus.Approved;

                        var totalLeaveDays = (leaveRequest.EndDate - leaveRequest.StartDate).Days + 1;
                        if (leaveRequest.Employee.RemainingLeaves < totalLeaveDays)
                            return BadRequest("Çalışanın yeterli izin hakkı bulunmuyor.");

                        leaveRequest.Employee.RemainingLeaves -= totalLeaveDays;
                    }
                }
            }

            _context.SaveChanges();
            return Ok(new { message = "İzin talebi güncellendi." });
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

            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var userEmail = User.Identity?.Name;

            var approver = _context.Employees.FirstOrDefault(e => e.Email == userEmail);
            if (approver == null)
                return Unauthorized("Reddeden kişi bulunamadı.");

            var approverFullName = $"{approver.FirstName} {approver.LastName}";

            if (userEmail == leaveRequest.Employee.Email)
            {
                if (userRole == "Manager")
                {
                    leaveRequest.ApprovedByManager = null;
                }
                else if (userRole == "HRManager")
                {
                    leaveRequest.ApprovedByHRManager = null;
                }
            }
            else
            {
                if (userRole == "Manager")
                {
                    if (leaveRequest.Employee.Title == EmployeeTitle.HRManager)
                        return BadRequest("Manager, HRManager taleplerini reddedemez.");

                    leaveRequest.ApprovedByManager = null;
                }
                else if (userRole == "HRManager")
                {
                    leaveRequest.ApprovedByHRManager = null;
                }
            }

            leaveRequest.Status = LeaveStatus.Rejected;
            _context.SaveChanges();
            return Ok(new { message = "İzin talebi reddedildi." });
        }

    }
}
