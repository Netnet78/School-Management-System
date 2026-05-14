namespace SchoolManagement.Application.Features.Departments.Services
{
    public class DepartmentService : CrudServiceBase<Department>, IDepartmentService
    {
        public DepartmentService(IDepartmentRepository repository) : base(repository)
        {
        }
    }
}


