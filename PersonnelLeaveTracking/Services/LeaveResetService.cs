using Microsoft.EntityFrameworkCore;
using PersonnelLeaveTracking.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PersonnelLeaveTracking.Services
{
    public class LeaveResetService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public LeaveResetService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);

                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var today = DateTime.UtcNow.Date;

                    var employees = await context.Employees
                    .Where(e => (today - e.HireDate).TotalDays >= 365)
                    .ToListAsync(stoppingToken);


                    foreach (var employee in employees)
                    {
                        employee.RemainingLeaves = 14;
                        employee.HireDate = employee.HireDate.AddYears(1);
                    }

                    await context.SaveChangesAsync(stoppingToken);
                }
            }
        }
    }
}