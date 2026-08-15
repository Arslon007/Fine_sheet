using Domain.Entity;

namespace Application.Interfaces;

public class BackupData
{
    public DateTime CreatedAt { get; set; }
    public List<EmployeeBackup> Employees { get; set; } = new();
}

public class EmployeeBackup
{
    public string FullName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public List<FineBackup> Fines { get; set; } = new();
    public List<BonusBackup> Bonuses { get; set; } = new();
}

public class FineBackup
{
    public string Reason { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
}

public class BonusBackup
{
    public string Reason { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
}

public class BackupInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int EmployeeCount { get; set; }
    public int FineCount { get; set; }
    public int BonusCount { get; set; }
}

public interface IBackupService
{
    BackupData ExportAll();
    void ImportAll(BackupData data);
    List<BackupInfo> GetAll();
    BackupInfo Save(string? name);
    void Restore(int backupId);
    void Delete(int backupId);
}
