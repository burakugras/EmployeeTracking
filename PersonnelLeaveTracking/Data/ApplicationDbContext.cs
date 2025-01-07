using Microsoft.EntityFrameworkCore;
using PersonnelLeaveTracking.Models;
using PersonnelLeaveTracking.Enums;

namespace PersonnelLeaveTracking.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>()
                        .Property(e => e.Title)
                        .HasConversion<string>();

            modelBuilder.Entity<LeaveRequest>()
                        .Property(lr => lr.Status)
                        .HasConversion<string>();
        }
    }
}
