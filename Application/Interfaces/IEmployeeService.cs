using Domain.Entity;

namespace Application.Interfaces;

public interface IEmployeeService
{
    List<Employee> GetAllEmployees();
    Employee? GetEmployeeById(int id);
    void AddEmployee(string fullName, string position, decimal salary);
    bool UpdateEmployee(int id, string fullName, string position, decimal salary);
    bool DeleteEmployee(int id);
}
