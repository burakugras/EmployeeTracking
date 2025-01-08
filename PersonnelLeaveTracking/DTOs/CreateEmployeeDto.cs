using System.ComponentModel.DataAnnotations;

namespace PersonnelLeaveTracking.DTOs
{
    public class CreateEmployeeDto
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(4)]
        public string Password { get; set; }

        [Required]
        public int Title { get; set; }

        [Required]
        public DateTime HireDate { get; set; }

        public DateTime? BirthDate { get; set; }

        public int? DepartmentId { get; set; }
    }
}
