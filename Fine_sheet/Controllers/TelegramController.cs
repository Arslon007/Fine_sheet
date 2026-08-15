using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Fine_sheet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TelegramController : ControllerBase
{
    private readonly ITelegramService _telegramService;

    public TelegramController(ITelegramService telegramService)
    {
        _telegramService = telegramService;
    }

    [HttpPost("send-report")]
    public async Task<IActionResult> SendReport()
    {
        await _telegramService.SendDailyReportAsync();
        return Ok(new { message = "Hisobot Telegramga yuborildi." });
    }
}
