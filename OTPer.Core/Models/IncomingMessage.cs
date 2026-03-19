using System.ComponentModel.DataAnnotations;

namespace OTPer.Core.Models;

public class IncomingMessage
{
    [Required]
    public required string Message { get; set; }

    [Required]
    public required string Sender { get; set; }
}
