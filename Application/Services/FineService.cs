using Application.Interfaces;
using DataAccess.Persistence;
using Domain.Entity;

namespace Application.Services;

public class FineService : IFineService
{
    private readonly AppDbContext _db;

    public FineService(AppDbContext context)
    {
        _db = context;
    }

    public List<Fine> GetFinesByEmployeeId(int employeeId) =>
        _db.Fine
            .Where(f => f.EmployeeId == employeeId)
            .OrderByDescending(f => f.Date)
            .ToList();

    public decimal GetTotalFines(int employeeId) =>
        _db.Fine
            .Where(f => f.EmployeeId == employeeId)
            .Sum(f => f.Amount);

    public void AddFine(int employeeId, string reason, decimal amount)
    {
        var employee = _db.Employee.FirstOrDefault(e => e.Id == employeeId)
            ?? throw new ArgumentException($"Сотрудник с ID {employeeId} не найден.");

        var fine = new Fine
        {
            EmployeeId = employeeId,
            Reason = reason,
            Amount = amount,
            Date = DateTime.UtcNow
        };

        _db.Fine.Add(fine);
        _db.SaveChanges();
    }

    public void UpdateFine(int fineId, string reason, decimal amount)
    {
        var fine = _db.Fine.FirstOrDefault(f => f.Id == fineId)
            ?? throw new ArgumentException($"Штраф с ID {fineId} не найден.");

        fine.Reason = reason;
        fine.Amount = amount;
        _db.SaveChanges();
    }

    public void DeleteFine(int fineId)
    {
        var fine = _db.Fine.FirstOrDefault(f => f.Id == fineId)
            ?? throw new ArgumentException($"Штраф с ID {fineId} не найден.");

        _db.Fine.Remove(fine);
        _db.SaveChanges();
    }
}
