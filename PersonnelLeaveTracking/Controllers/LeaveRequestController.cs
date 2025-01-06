using Microsoft.AspNetCore.Mvc;
using PersonnelLeaveTracking.Data;
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
        public IActionResult GetAllLeaveRequests()
        {
            var leaveRequests = _context.LeaveRequests.ToList();
            return Ok(leaveRequests);
        }

        // 2. Belirli bir çalışanın izin taleplerini listele
        [HttpGet("employee/{employeeId}")]
        public IActionResult GetLeaveRequestsByEmployee(int employeeId)
        {
            var leaveRequests = _context.LeaveRequests.Where(lr => lr.EmployeeId == employeeId).ToList();
            if (!leaveRequests.Any())
                return NotFound("Bu çalışan için izin talebi bulunamadı.");

            return Ok(leaveRequests);
        }

        // 3. Yeni izin talebi oluştur
        [HttpPost]
        public IActionResult CreateLeaveRequest(LeaveRequest leaveRequest)
        {

            var employee = _context.Employees.Find(leaveRequest.EmployeeId);
            if (employee == null)
                return NotFound("Çalışan bulunamadı.");

            var totalLeaveDays = (leaveRequest.EndDate - leaveRequest.StartDate).Days + 1;
            if (employee.RemainingLeaves < totalLeaveDays)
                return BadRequest("Yeterli izin hakkı bulunmuyor.");

            _context.LeaveRequests.Add(leaveRequest);
            employee.RemainingLeaves -= totalLeaveDays; // Kalan izin günlerini güncelle
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetAllLeaveRequests), new { id = leaveRequest.Id }, leaveRequest);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateLeaveRequestStatus(int id, [FromBody] string status)
        {
            var leaveRequest = _context.LeaveRequests.Find(id);
            if (leaveRequest == null)
                return NotFound("İzin talebi bulunamadı.");

            leaveRequest.Status = status;
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
