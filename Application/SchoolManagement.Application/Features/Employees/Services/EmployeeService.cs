using SchoolManagement.Core.Application.Interfaces;
using SchoolManagement.Core.Infrastructure.Interfaces;
using SchoolManagement.Core.Models;

namespace SchoolManagement.Application.Services
{
    public class EmployeeService : CrudServiceBase<Employee>, IEmployeeService
    {
        public EmployeeService(IEmployeeRepository repository) : base(repository)
        {
        }
    }
}
