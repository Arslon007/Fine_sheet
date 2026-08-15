namespace Domain.Entity;

public class Employee
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public decimal Salary { get; set; }


    public List<Fine> Fines { get; set; } = new();
    public List<Bonus> Bonuses { get; set; } = new();
}
