namespace Application.Interfaces;
public interface ISalaryService
{
    decimal GetTotalFines(int employeeId);
    decimal GetTotalBonuses(int employeeId);
    decimal CalculateFinalSalary(int employeeId);
}
