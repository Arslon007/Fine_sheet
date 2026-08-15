using Application.Interfaces;
using DataAccess.Persistence;

namespace Application.Services;

public class SalaryService : ISalaryService
{
    private readonly AppDbContext _db;

    public SalaryService(AppDbContext context)
    {
        _db = context;
    }

    public decimal GetTotalFines(int employeeId) =>
        _db.Fine
            .Where(f => f.EmployeeId == employeeId)
            .Sum(f => f.Amount);

    public decimal GetTotalBonuses(int employeeId) =>
        _db.Bonus
            .Where(b => b.EmployeeId == employeeId)
            .Sum(b => b.Amount);

    public decimal CalculateFinalSalary(int employeeId)
    {
        var employee = _db.Employee.FirstOrDefault(e => e.Id == employeeId)
            ?? throw new ArgumentException($"Сотрудник с ID {employeeId} не найден.");

        decimal totalBonuses = GetTotalBonuses(employeeId);
        decimal totalFines = GetTotalFines(employeeId);

        return employee.Salary + totalBonuses - totalFines;
    }
}
