using System.ComponentModel.DataAnnotations;

namespace PersonnelLeaveTracking.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
