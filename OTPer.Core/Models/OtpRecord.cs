namespace OTPer.Core.Models;

public class OtpRecord
{
    public int Id { get; set; }
    public required string Sender { get; set; }
    public string? Sms { get; set; }
    public string? Mms { get; set; }
    public string? Code { get; set; }
    public DateTime ReceivedAt { get; set; }
}
