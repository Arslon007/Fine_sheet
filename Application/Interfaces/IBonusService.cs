using Domain.Entity;

namespace Application.Interfaces;

public interface IBonusService
{
    List<Bonus> GetBonusesByEmployeeId(int employeeId);
    decimal GetTotalBonuses(int employeeId);
    void AddBonus(int employeeId, string reason, decimal amount);
    void UpdateBonus(int bonusId, string reason, decimal amount);
    void DeleteBonus(int bonusId);
}
