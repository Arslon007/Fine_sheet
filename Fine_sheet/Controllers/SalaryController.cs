using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Fine_sheet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalaryController : ControllerBase
{
    private readonly ISalaryService _salaryService;
    private readonly IEmployeeService _employeeService;

    public SalaryController(ISalaryService salaryService, IEmployeeService employeeService)
    {
        _salaryService = salaryService;
        _employeeService = employeeService;
    }

    // GET api/salary/5
    [HttpGet("{employeeId}")]
    public ActionResult GetFinalSalary(int employeeId)
    {
        var employee = _employeeService.GetEmployeeById(employeeId);
        if (employee == null)
            return NotFound(new { message = $"Сотрудник с ID {employeeId} не найден." });

        try
        {
            var totalFines = _salaryService.GetTotalFines(employeeId);
            var totalBonuses = _salaryService.GetTotalBonuses(employeeId);
            var finalSalary = _salaryService.CalculateFinalSalary(employeeId);

            return Ok(new
            {
                employeeId,
                fullName = employee.FullName,
                position = employee.Position,
                baseSalary = employee.Salary,
                totalBonuses,
                totalFines,
                finalSalary
            });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
