using SchoolManagement.Core.Application.Interfaces;
using SchoolManagement.Core.Infrastructure.Interfaces;
using SchoolManagement.Core.Models;

namespace SchoolManagement.Application.Services
{
    public class DepartmentService : CrudServiceBase<Department>, IDepartmentService
    {
        public DepartmentService(IDepartmentRepository repository) : base(repository)
        {
        }
    }
}
