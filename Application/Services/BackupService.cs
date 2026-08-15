using Application.Interfaces;
using DataAccess.Persistence;
using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Application.Services;

public class BackupService : IBackupService
{
    private readonly AppDbContext _db;

    public BackupService(AppDbContext context)
    {
        _db = context;
    }

    public BackupData ExportAll()
    {
        var employees = _db.Employee
            .Include(e => e.Fines)
            .Include(e => e.Bonuses)
            .OrderBy(e => e.Id)
            .ToList();

        return new BackupData
        {
            CreatedAt = DateTime.UtcNow,
            Employees = employees.Select(e => new EmployeeBackup
            {
                FullName = e.FullName,
                Position = e.Position,
                Salary = e.Salary,
                Fines = e.Fines.Select(f => new FineBackup
                {
                    Reason = f.Reason,
                    Amount = f.Amount,
                    Date = f.Date,
                }).ToList(),
                Bonuses = e.Bonuses.Select(b => new BonusBackup
                {
                    Reason = b.Reason,
                    Amount = b.Amount,
                    Date = b.Date,
                }).ToList(),
            }).ToList(),
        };
    }

    public void ImportAll(BackupData data)
    {
        using var transaction = _db.Database.BeginTransaction();

        _db.Bonus.RemoveRange(_db.Bonus.ToList());
        _db.Fine.RemoveRange(_db.Fine.ToList());
        _db.Employee.RemoveRange(_db.Employee.ToList());
        _db.SaveChanges();

        foreach (var emp in data.Employees)
        {
            var employee = new Employee
            {
                FullName = emp.FullName,
                Position = emp.Position,
                Salary = emp.Salary,
            };

            _db.Employee.Add(employee);
            _db.SaveChanges();

            foreach (var fine in emp.Fines)
            {
                _db.Fine.Add(new Fine
                {
                    EmployeeId = employee.Id,
                    Reason = fine.Reason,
                    Amount = fine.Amount,
                    Date = fine.Date,
                });
            }

            foreach (var bonus in emp.Bonuses)
            {
                _db.Bonus.Add(new Bonus
                {
                    EmployeeId = employee.Id,
                    Reason = bonus.Reason,
                    Amount = bonus.Amount,
                    Date = bonus.Date,
                });
            }

            _db.SaveChanges();
        }

        transaction.Commit();
    }

    public List<BackupInfo> GetAll()
    {
        return _db.Backup
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new BackupInfo
            {
                Id = b.Id,
                Name = b.Name,
                CreatedAt = b.CreatedAt,
                EmployeeCount = b.EmployeeCount,
                FineCount = b.FineCount,
                BonusCount = b.BonusCount,
            })
            .ToList();
    }

    public BackupInfo Save(string? name)
    {
        var data = ExportAll();
        var json = JsonSerializer.Serialize(data);

        var totalFines = data.Employees.Sum(e => e.Fines.Count);
        var totalBonuses = data.Employees.Sum(e => e.Bonuses.Count);

        var backup = new Backup
        {
            Name = name ?? $"Bek'ap — {DateTime.UtcNow:dd.MM.yyyy HH:mm}",
            Data = json,
            CreatedAt = DateTime.UtcNow,
            EmployeeCount = data.Employees.Count,
            FineCount = totalFines,
            BonusCount = totalBonuses,
        };

        _db.Backup.Add(backup);
        _db.SaveChanges();

        return new BackupInfo
        {
            Id = backup.Id,
            Name = backup.Name,
            CreatedAt = backup.CreatedAt,
            EmployeeCount = backup.EmployeeCount,
            FineCount = backup.FineCount,
            BonusCount = backup.BonusCount,
        };
    }

    public void Restore(int backupId)
    {
        var backup = _db.Backup.Find(backupId)
            ?? throw new KeyNotFoundException("Bek'ap topilmadi");

        var data = JsonSerializer.Deserialize<BackupData>(backup.Data)
            ?? throw new InvalidOperationException("Bek'ap ma'lumotlari buzilgan");

        ImportAll(data);
    }

    public void Delete(int backupId)
    {
        var backup = _db.Backup.Find(backupId)
            ?? throw new KeyNotFoundException("Bek'ap topilmadi");

        _db.Backup.Remove(backup);
        _db.SaveChanges();
    }
}
