
using Domain.Entity;

namespace Application.Interfaces;

public interface IFineService
{
    List<Fine> GetFinesByEmployeeId(int employeeId);
    decimal GetTotalFines(int employeeId);
    void AddFine(int employeeId, string reason, decimal amount);
    void UpdateFine(int fineId, string reason, decimal amount);
    void DeleteFine(int fineId);
}
