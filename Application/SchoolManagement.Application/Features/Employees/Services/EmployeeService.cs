namespace SchoolManagement.Application.Features.Employees.Services
{
    public class EmployeeService : CrudServiceBase<Employee>, IEmployeeService
    {
        public EmployeeService(IEmployeeRepository repository) : base(repository)
        {
        }
    }
}


