using Microsoft.EntityFrameworkCore;
using PersonnelLeaveTracking.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

    // app.UseSwagger();
    // app.UseSwaggerUI();


app.UseHttpsRedirection();

// Yetkilendirme middleware'i kaldırıldı
app.UseAuthorization();

app.MapControllers();

app.Run();
