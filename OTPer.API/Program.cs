using Microsoft.EntityFrameworkCore;
using OTPer.Core.Data;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=data/otper.db";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<IOtpRepository, OtpRepository>();

var app = builder.Build();

// Ensure the database directory and tables exist.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var dbPath = db.Database.GetConnectionString()!;
    var dataSource = dbPath.Replace("Data Source=", "", StringComparison.OrdinalIgnoreCase);
    var dir = Path.GetDirectoryName(dataSource);
    if (!string.IsNullOrEmpty(dir))
    {
        Directory.CreateDirectory(dir);
    }
    db.Database.EnsureCreated();
}

app.MapOpenApi();
app.MapScalarApiReference();

app.UseAuthorization();

app.MapControllers();

app.Run();
