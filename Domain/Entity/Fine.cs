namespace Domain.Entity;

public class Fine
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;


    public Employee Employee { get; set; } = null!;
}
