using Microsoft.AspNetCore.Mvc;
using PersonnelLeaveTracking.Data;
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
            var employees = _context.Employees.ToList();
            return Ok(employees);
        }

        [HttpGet("{id}")]
        public IActionResult GetEmployee(int id)
        {
            var employee = _context.Employees.Find(id);
            if (employee == null)
                return NotFound("Çalışan bulunamadı.");
            return Ok(employee);
        }

        [HttpPost]
        public IActionResult AddEmployee(Employee employee)
        {
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
            employee.Title = updatedEmployee.Title;
            employee.HireDate = updatedEmployee.HireDate;
            employee.BirthDate = updatedEmployee.BirthDate;
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
