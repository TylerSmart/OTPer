using System.Text.RegularExpressions;

namespace OTPer.Core.Services;

public static partial class OtpParser
{
    /// <summary>
    /// Extracts a 4–8 digit numeric OTP code from the given message text.
    /// Returns <c>null</c> when no code is found.
    /// </summary>
    public static string? ExtractCode(string message)
    {
        var match = OtpRegex().Match(message);
        return match.Success ? match.Groups[1].Value : null;
    }

    // Matches a standalone sequence of 4–8 digits that is not part of a longer number.
    [GeneratedRegex(@"(?<!\d)(\d{4,8})(?!\d)")]
    private static partial Regex OtpRegex();
}
