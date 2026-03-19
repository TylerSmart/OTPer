using Microsoft.AspNetCore.Mvc;
using OTPer.Core.Data;
using OTPer.Core.Models;
using OTPer.Core.Services;

namespace OTPer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OtpController : ControllerBase
{
    private readonly IOtpRepository _repository;

    public OtpController(IOtpRepository repository)
    {
        _repository = repository;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] IncomingMessage incoming)
    {
        var record = new OtpRecord
        {
            Sender = incoming.Sender,
            Message = incoming.Message,
            Code = OtpParser.ExtractCode(incoming.Message),
            ReceivedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(record);

        return CreatedAtAction(nameof(Get), new { }, record);
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] int count = 10,
        [FromQuery] string? sender = null,
        [FromQuery] string? keyword = null)
    {
        count = Math.Clamp(count, 1, 100);

        var records = await _repository.GetRecentAsync(count, sender, keyword);
        return Ok(records);
    }
}
