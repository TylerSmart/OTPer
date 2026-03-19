using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using OTPer.Core.Data;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        var httpContext = context.ApplicationServices
            .GetRequiredService<IHttpContextAccessor>().HttpContext;
        if (httpContext is not null)
        {
            var scheme = httpContext.Request.Scheme;
            var host = httpContext.Request.Host;
            document.Servers = [new() { Url = $"{scheme}://{host}" }];
        }
        return Task.CompletedTask;
    });
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = null;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=data/otper.db";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<IOtpRepository, OtpRepository>();

var app = builder.Build();

// Must be first so scheme/host are correct for all downstream middleware.
app.UseForwardedHeaders();

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

app.MapGet("/api/version", () => new
{
    CommitSha = Environment.GetEnvironmentVariable("COMMIT_SHA") ?? "dev",
    Environment = app.Environment.EnvironmentName
});

app.MapControllers();

app.Run();
