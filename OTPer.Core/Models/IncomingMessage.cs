using System.ComponentModel.DataAnnotations;

namespace OTPer.Core.Models;

public class IncomingMessage
{
    public string? Sms { get; set; }

    public string? Mms { get; set; }

    [Required]
    public required string Sender { get; set; }
}
