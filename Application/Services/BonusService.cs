using Application.Interfaces;
using DataAccess.Persistence;
using Domain.Entity;

namespace Application.Services;

public class BonusService : IBonusService
{
    private readonly AppDbContext _db;

    public BonusService(AppDbContext context)
    {
        _db = context;
    }

    public List<Bonus> GetBonusesByEmployeeId(int employeeId) =>
        _db.Bonus
            .Where(b => b.EmployeeId == employeeId)
            .OrderByDescending(b => b.Date)
            .ToList();

    public decimal GetTotalBonuses(int employeeId) =>
        _db.Bonus
            .Where(b => b.EmployeeId == employeeId)
            .Sum(b => b.Amount);

    public void AddBonus(int employeeId, string reason, decimal amount)
    {
        var employee = _db.Employee.FirstOrDefault(e => e.Id == employeeId)
            ?? throw new ArgumentException($"Сотрудник с ID {employeeId} не найден.");

        var bonus = new Bonus
        {
            EmployeeId = employeeId,
            Reason = reason,
            Amount = amount,
            Date = DateTime.UtcNow
        };

        _db.Bonus.Add(bonus);
        _db.SaveChanges();
    }

    public void UpdateBonus(int bonusId, string reason, decimal amount)
    {
        var bonus = _db.Bonus.FirstOrDefault(b => b.Id == bonusId)
            ?? throw new ArgumentException($"Бонус с ID {bonusId} не найден.");

        bonus.Reason = reason;
        bonus.Amount = amount;
        _db.SaveChanges();
    }

    public void DeleteBonus(int bonusId)
    {
        var bonus = _db.Bonus.FirstOrDefault(b => b.Id == bonusId)
            ?? throw new ArgumentException($"Бонус с ID {bonusId} не найден.");

        _db.Bonus.Remove(bonus);
        _db.SaveChanges();
    }
}
