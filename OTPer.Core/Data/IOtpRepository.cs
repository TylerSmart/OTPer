using OTPer.Core.Models;

namespace OTPer.Core.Data;

public interface IOtpRepository
{
    Task<OtpRecord> AddAsync(OtpRecord record);
    Task<List<OtpRecord>> GetRecentAsync(int count, string? sender, string? keyword);
}
