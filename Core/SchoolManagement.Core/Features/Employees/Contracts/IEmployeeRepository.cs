using SchoolManagement.Core.Features.Employees.Models;
using SchoolManagement.Core.Shared.Contracts;

namespace SchoolManagement.Core.Features.Employees.Contracts
{
    public interface IEmployeeRepository : IBaseRepository<Employee>
    {
    }
}

