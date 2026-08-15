using Application.Interfaces;
using Domain.Entity;
using Microsoft.AspNetCore.Mvc;

namespace Fine_sheet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BonusController : ControllerBase
{
    private readonly IBonusService _bonusService;

    public BonusController(IBonusService bonusService)
    {
        _bonusService = bonusService;
    }

    // GET api/bonus/employee/5
    [HttpGet("employee/{employeeId}")]
    public ActionResult<List<Bonus>> GetByEmployee(int employeeId)
    {
        var bonuses = _bonusService.GetBonusesByEmployeeId(employeeId);
        return Ok(bonuses);
    }

    // GET api/bonus/employee/5/total
    [HttpGet("employee/{employeeId}/total")]
    public ActionResult<decimal> GetTotal(int employeeId)
    {
        var total = _bonusService.GetTotalBonuses(employeeId);
        return Ok(new { employeeId, totalBonuses = total });
    }

    // POST api/bonus
    [HttpPost]
    public ActionResult AddBonus([FromBody] CreateBonusRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            return BadRequest(new { message = "Причина бонуса не может быть пустой." });

        if (request.Amount <= 0)
            return BadRequest(new { message = "Сумма бонуса должна быть больше нуля." });

        try
        {
            _bonusService.AddBonus(request.EmployeeId, request.Reason, request.Amount);
            return Ok(new { message = $"Бонус {request.Amount:N2} руб. успешно добавлен." });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // PUT api/bonus/{id}
    [HttpPut("{id}")]
    public ActionResult UpdateBonus(int id, [FromBody] UpdateBonusRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            return BadRequest(new { message = "Причина бонуса не может быть пустой." });

        if (request.Amount <= 0)
            return BadRequest(new { message = "Сумма бонуса должна быть больше нуля." });

        try
        {
            _bonusService.UpdateBonus(id, request.Reason, request.Amount);
            return Ok(new { message = "Бонус успешно обновлён." });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // DELETE api/bonus/{id}
    [HttpDelete("{id}")]
    public ActionResult DeleteBonus(int id)
    {
        try
        {
            _bonusService.DeleteBonus(id);
            return Ok(new { message = "Бонус успешно удалён." });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

public class CreateBonusRequest
{
    public int EmployeeId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class UpdateBonusRequest
{
    public string Reason { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
