using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonnelLeaveTracking.Data;
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
                                       Department = new
                                       {
                                           e.Department.Id,
                                           e.Department.Name
                                       },
                                       e.RemainingLeaves
                                   })
                                   .FirstOrDefault();

            if (employee == null)
                return NotFound("Çalışan bulunamadı.");

            return Ok(employee);
        }

        [HttpPost]
        public IActionResult AddEmployee(Employee employee)
        {
            employee.Department = null;
            if (employee.RemainingLeaves == 0)
            {
                employee.RemainingLeaves = 14;
            }

            _context.Employees.Add(employee);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateEmployee(int id, Employee updatedEmployee)
        {
            var employee = _context.Employees.Find(id);
            if (employee == null)
                return NotFound("Çalışan bulunamadı.");

            employee.FirstName = updatedEmployee.FirstName;
            employee.LastName = updatedEmployee.LastName;
            employee.Email = updatedEmployee.Email;
            employee.HireDate = updatedEmployee.HireDate;
            employee.BirthDate = updatedEmployee.BirthDate;

            if (Enum.TryParse(updatedEmployee.Title.ToString(), out EmployeeTitle parsedTitle))
            {
                employee.Title = parsedTitle;
            }
            else
            {
                return BadRequest("Geçersiz unvan.");
            }

            employee.DepartmentId = updatedEmployee.DepartmentId;
            employee.RemainingLeaves = updatedEmployee.RemainingLeaves == 0 ? 14 : updatedEmployee.RemainingLeaves;

            _context.SaveChanges();
            return NoContent();
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
