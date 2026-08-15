using Application.Interfaces;
using Domain.Entity;
using Microsoft.AspNetCore.Mvc;

namespace Fine_sheet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FineController : ControllerBase
{
    private readonly IFineService _fineService;

    public FineController(IFineService fineService)
    {
        _fineService = fineService;
    }

    // GET api/fine/employee/5
    [HttpGet("employee/{employeeId}")]
    public ActionResult<List<Fine>> GetByEmployee(int employeeId)
    {
        var fines = _fineService.GetFinesByEmployeeId(employeeId);
        return Ok(fines);
    }

    // GET api/fine/employee/5/total
    [HttpGet("employee/{employeeId}/total")]
    public ActionResult<decimal> GetTotal(int employeeId)
    {
        var total = _fineService.GetTotalFines(employeeId);
        return Ok(new { employeeId, totalFines = total });
    }

    // POST api/fine
    [HttpPost]
    public ActionResult AddFine([FromBody] CreateFineRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            return BadRequest(new { message = "Причина штрафа не может быть пустой." });

        if (request.Amount <= 0)
            return BadRequest(new { message = "Сумма штрафа должна быть больше нуля." });

        try
        {
            _fineService.AddFine(request.EmployeeId, request.Reason, request.Amount);
            return Ok(new { message = $"Штраф {request.Amount:N2} руб. успешно добавлен." });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // PUT api/fine/{id}
    [HttpPut("{id}")]
    public ActionResult UpdateFine(int id, [FromBody] UpdateFineRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            return BadRequest(new { message = "Причина штрафа не может быть пустой." });

        if (request.Amount <= 0)
            return BadRequest(new { message = "Сумма штрафа должна быть больше нуля." });

        try
        {
            _fineService.UpdateFine(id, request.Reason, request.Amount);
            return Ok(new { message = "Штраф успешно обновлён." });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // DELETE api/fine/{id}
    [HttpDelete("{id}")]
    public ActionResult DeleteFine(int id)
    {
        try
        {
            _fineService.DeleteFine(id);
            return Ok(new { message = "Штраф успешно удалён." });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

public class UpdateFineRequest
{
    public string Reason { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
