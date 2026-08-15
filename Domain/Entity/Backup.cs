namespace Domain.Entity;

public class Backup
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int EmployeeCount { get; set; }
    public int FineCount { get; set; }
    public int BonusCount { get; set; }
}
