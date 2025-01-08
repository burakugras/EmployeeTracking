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
    public class EmployeeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EmployeeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "HRManager")]
        public IActionResult GetEmployees()
        {
            var employees = _context.Employees
                                    .Include(e => e.Department)
                                    .Select(e => new
                                    {
                                        e.Id,
                                        e.FirstName,
                                        e.LastName,
                                        e.Email,
                                        e.HireDate,
                                        e.BirthDate,
                                        Title = e.Title.ToString(),
                                        Department = e.Department != null
                                            ? new { e.Department.Id, e.Department.Name }
                                            : null,
                                        e.RemainingLeaves
                                    })
                                    .ToList();

            return Ok(employees);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "HRManager")]
        public IActionResult GetEmployee(int id)
        {
            var employee = _context.Employees
                                   .Include(e => e.Department)
                                   .Where(e => e.Id == id)
                                   .Select(e => new
                                   {
                                       e.Id,
                                       e.FirstName,
                                       e.LastName,
                                       e.Email,
                                       e.HireDate,
                                       e.BirthDate,
                                       Title = e.Title.ToString(),
                                       Department = e.Department != null
                                           ? new { e.Department.Id, e.Department.Name }
                                           : null,
                                       e.RemainingLeaves
                                   })
                                   .FirstOrDefault();

            if (employee == null)
                return NotFound("Çalışan bulunamadı.");

            return Ok(employee);
        }

        [HttpPost]
        public IActionResult AddEmployee([FromBody] CreateEmployeeDto createEmployeeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (_context.Employees.Any(e => e.Email == createEmployeeDto.Email))
                return BadRequest("Bu e-posta adresi zaten kayıtlı.");

            var employee = new Employee
            {
                FirstName = createEmployeeDto.FirstName,
                LastName = createEmployeeDto.LastName,
                Email = createEmployeeDto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(createEmployeeDto.Password),
                Title = (EmployeeTitle)createEmployeeDto.Title,
                HireDate = createEmployeeDto.HireDate,
                BirthDate = createEmployeeDto.BirthDate,
                DepartmentId = createEmployeeDto.DepartmentId,
                RemainingLeaves = 14
            };

            _context.Employees.Add(employee);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, new
            {
                Message = "Çalışan başarıyla eklendi.",
                EmployeeId = employee.Id
            });
        }

        [HttpPut("{id}")]
        public IActionResult UpdateEmployee(int id, [FromBody] UpdateEmployeeDto updateEmployeeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var employee = _context.Employees.FirstOrDefault(e => e.Id == id);
            if (employee == null)
                return NotFound("Çalışan bulunamadı.");

            employee.FirstName = updateEmployeeDto.FirstName;
            employee.LastName = updateEmployeeDto.LastName;
            employee.Email = updateEmployeeDto.Email;
            if (!string.IsNullOrWhiteSpace(updateEmployeeDto.Password))
            {
                employee.Password = BCrypt.Net.BCrypt.HashPassword(updateEmployeeDto.Password);
            }
            employee.Title = (EmployeeTitle)updateEmployeeDto.Title;
            employee.HireDate = updateEmployeeDto.HireDate;
            employee.BirthDate = updateEmployeeDto.BirthDate;
            employee.DepartmentId = updateEmployeeDto.DepartmentId;

            _context.SaveChanges();

            return Ok("Çalışan bilgileri başarıyla güncellendi.");
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteEmployee(int id)
        {
            var employee = _context.Employees.Find(id);
            if (employee == null)
                return NotFound("Çalışan bulunamadı.");

            _context.Employees.Remove(employee);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
