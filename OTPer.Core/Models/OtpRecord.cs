namespace OTPer.Core.Models;

public class OtpRecord
{
    public int Id { get; set; }
    public required string Sender { get; set; }
    public required string Message { get; set; }
    public required string Code { get; set; }
    public bool Used { get; set; }
    public DateTime ReceivedAt { get; set; }
}
