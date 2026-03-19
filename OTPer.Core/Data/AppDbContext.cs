using Microsoft.EntityFrameworkCore;
using OTPer.Core.Models;

namespace OTPer.Core.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<OtpRecord> OtpRecords => Set<OtpRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OtpRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Sender).IsRequired();
            entity.Property(e => e.Message).IsRequired();
            entity.HasIndex(e => e.ReceivedAt);
        });
    }
}
