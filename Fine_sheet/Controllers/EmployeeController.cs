using Application.Interfaces;
using Domain.Entity;
using Microsoft.AspNetCore.Mvc;

namespace Fine_sheet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeeController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    // GET api/employee
    [HttpGet]
    public ActionResult<List<Employee>> GetAll()
    {
        var employees = _employeeService.GetAllEmployees();
        return Ok(employees);
    }

    // GET api/employee/5
    [HttpGet("{id}")]
    public ActionResult<Employee> GetById(int id)
    {
        var employee = _employeeService.GetEmployeeById(id);
        if (employee == null)
            return NotFound(new { message = $"Сотрудник с ID {id} не найден." });

        return Ok(employee);
    }

    // POST api/employee
    [HttpPost]
    public ActionResult AddEmployee([FromBody] CreateEmployeeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
            return BadRequest(new { message = "ФИО не может быть пустым." });

        if (string.IsNullOrWhiteSpace(request.Position))
            return BadRequest(new { message = "Должность не может быть пустой." });

        if (request.Salary < 0)
            return BadRequest(new { message = "Зарплата не может быть отрицательной." });

        _employeeService.AddEmployee(request.FullName, request.Position, request.Salary);
        return Ok(new { message = $"Сотрудник \"{request.FullName}\" успешно добавлен." });
    }

    // PUT api/employee/5
    [HttpPut("{id}")]
    public ActionResult UpdateEmployee(int id, [FromBody] CreateEmployeeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
            return BadRequest(new { message = "ФИО не может быть пустым." });

        if (string.IsNullOrWhiteSpace(request.Position))
            return BadRequest(new { message = "Должность не может быть пустой." });

        if (request.Salary < 0)
            return BadRequest(new { message = "Зарплата не может быть отрицательной." });

        var result = _employeeService.UpdateEmployee(id, request.FullName, request.Position, request.Salary);
        if (!result)
            return NotFound(new { message = $"Сотрудник с ID {id} не найден." });

        return Ok(new { message = $"Сотрудник \"{request.FullName}\" успешно обновлён." });
    }

    // DELETE api/employee/5
    [HttpDelete("{id}")]
    public ActionResult DeleteEmployee(int id)
    {
        var result = _employeeService.DeleteEmployee(id);
        if (!result)
            return NotFound(new { message = $"Сотрудник с ID {id} не найден." });

        return Ok(new { message = "Сотрудник успешно удалён." });
    }
}

public class CreateEmployeeRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public decimal Salary { get; set; }
}
