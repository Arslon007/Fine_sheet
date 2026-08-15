using Application.Interfaces;
using DataAccess.Persistence;
using Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly AppDbContext _db;

    public EmployeeService(AppDbContext context)
    {
        _db = context;
    }

    public List<Employee> GetAllEmployees() =>
        _db.Employee
            .Include(e => e.Fines)
            .Include(e => e.Bonuses)
            .OrderBy(e => e.Id)
            .ToList();

    public Employee? GetEmployeeById(int id) =>
        _db.Employee
            .Include(e => e.Fines)
            .Include(e => e.Bonuses)
            .FirstOrDefault(e => e.Id == id);

    public void AddEmployee(string fullName, string position, decimal salary)
    {
        var employee = new Employee
        {
            FullName = fullName,
            Position = position,
            Salary = salary
        };

        _db.Employee.Add(employee);
        _db.SaveChanges();
    }

    public bool UpdateEmployee(int id, string fullName, string position, decimal salary)
    {
        var employee = _db.Employee.FirstOrDefault(e => e.Id == id);
        if (employee == null) return false;

        employee.FullName = fullName;
        employee.Position = position;
        employee.Salary = salary;
        _db.SaveChanges();
        return true;
    }

    public bool DeleteEmployee(int id)
    {
        var employee = _db.Employee.FirstOrDefault(e => e.Id == id);
        if (employee == null) return false;

        _db.Employee.Remove(employee);
        _db.SaveChanges();
        return true;
    }
}
