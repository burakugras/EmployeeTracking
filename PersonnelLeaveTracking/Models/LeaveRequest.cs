using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PersonnelLeaveTracking.Enums; // Enum'ları kullanmak için ekleyin

namespace PersonnelLeaveTracking.Models
{
    public class LeaveRequest
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Employee")]
        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

        public string? ApprovedBy { get; set; }
    }
}
