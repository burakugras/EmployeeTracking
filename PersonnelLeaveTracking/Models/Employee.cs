using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersonnelLeaveTracking.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public DateTime HireDate { get; set; }

        public DateTime? BirthDate { get; set; }

        [Required]
        public string Title { get; set; } // Çalışan, Müdür, IK Müdür

        [ForeignKey("Department")]
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }

        public int RemainingLeaves { get; set; } = 14;
    }
}
