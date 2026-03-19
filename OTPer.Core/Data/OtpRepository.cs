using Microsoft.EntityFrameworkCore;
using OTPer.Core.Models;

namespace OTPer.Core.Data;

public class OtpRepository : IOtpRepository
{
    private readonly AppDbContext _db;

    public OtpRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<OtpRecord> AddAsync(OtpRecord record)
    {
        _db.OtpRecords.Add(record);
        await _db.SaveChangesAsync();
        return record;
    }

    public async Task<List<OtpRecord>> GetRecentAsync(int count, string? sender, string? keyword, DateTime? since)
    {
        IQueryable<OtpRecord> query = _db.OtpRecords;

        if (!string.IsNullOrWhiteSpace(sender))
        {
            query = query.Where(r => r.Sender.ToLower().Contains(sender.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(r => r.Message.ToLower().Contains(keyword.ToLower()));
        }

        if (since.HasValue)
        {
            query = query.Where(r => r.ReceivedAt >= since.Value);
        }

        return await query
            .Where(r => r.Used == false)
            .OrderByDescending(r => r.ReceivedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<OtpRecord?> MarkUsedAsync(int id)
    {
        var record = await _db.OtpRecords.FindAsync(id);
        if (record is null)
            return null;

        record.Used = true;
        await _db.SaveChangesAsync();
        return record;
    }
}
