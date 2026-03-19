using Microsoft.AspNetCore.Mvc;
using OTPer.Core.Data;
using OTPer.Core.Models;
using OTPer.Core.Services;

namespace OTPer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OTPController : ControllerBase
{
    private readonly IOtpRepository _repository;

    public OTPController(IOtpRepository repository)
    {
        _repository = repository;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] IncomingMessage incoming)
    {
        var code = OtpParser.ExtractCode(incoming.Message);
        if (code is null)
        {
            return UnprocessableEntity(new { error = "No OTP code found in the message." });
        }

        var record = new OtpRecord
        {
            Sender = incoming.Sender,
            Message = incoming.Message,
            Code = code,
            ReceivedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(record);

        return CreatedAtAction(nameof(Get), new { }, record);
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] int count = 10,
        [FromQuery] string? sender = null,
        [FromQuery] string? keyword = null,
        [FromQuery] DateTime? since = null)
    {
        count = Math.Clamp(count, 1, 100);

        var records = await _repository.GetRecentAsync(count, sender, keyword, since);
        return Ok(records);
    }

    [HttpPatch("{id}/used")]
    public async Task<IActionResult> MarkUsed(int id)
    {
        var record = await _repository.MarkUsedAsync(id);
        if (record is null)
        {
            return NotFound();
        }

        return Ok(record);
    }
}
