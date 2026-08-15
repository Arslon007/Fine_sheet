using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Fine_sheet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BackupController : ControllerBase
{
    private readonly IBackupService _backupService;
    private readonly ITelegramService _telegramService;

    public BackupController(IBackupService backupService, ITelegramService telegramService)
    {
        _backupService = backupService;
        _telegramService = telegramService;
    }

    // GET api/backup/export
    [HttpGet("export")]
    public ActionResult Export()
    {
        var data = _backupService.ExportAll();
        return Ok(data);
    }

    // POST api/backup/import
    [HttpPost("import")]
    public ActionResult Import([FromBody] BackupData data)
    {
        if (data == null || data.Employees == null || data.Employees.Count == 0)
            return BadRequest(new { message = "Fayl bo'sh yoki formati noto'g'ri." });

        _backupService.ImportAll(data);
        return Ok(new { message = $"Muvaffaqiyatli tiklandi. {data.Employees.Count} ta xodim import qilindi." });
    }

    // GET api/backup/list
    [HttpGet("list")]
    public ActionResult GetAll()
    {
        var backups = _backupService.GetAll();
        return Ok(backups);
    }

    // POST api/backup/save
    [HttpPost("save")]
    public async Task<ActionResult> Save([FromBody] SaveBackupRequest? request)
    {
        var info = _backupService.Save(request?.Name);

        // Отправляем бэкап файл в Telegram
        var data = _backupService.ExportAll();
        var json = System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        var fileName = $"backup_{DateTime.UtcNow:yyyy-MM-dd_HH-mm}.json";
        await _telegramService.SendBackupFileAsync(json, fileName);

        return Ok(info);
    }

    // POST api/backup/restore/{id}
    [HttpPost("restore/{id}")]
    public ActionResult Restore(int id)
    {
        try
        {
            _backupService.Restore(id);
            return Ok(new { message = "Ma'lumotlar muvaffaqiyatli tiklandi." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // DELETE api/backup/{id}
    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        try
        {
            _backupService.Delete(id);
            return Ok(new { message = "Bek'ap o'chirildi." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

public class SaveBackupRequest
{
    public string? Name { get; set; }
}
