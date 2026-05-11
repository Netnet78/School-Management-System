using SchoolManagement.Core.Application.Interfaces;
using SchoolManagement.Core.Infrastructure.Interfaces;
using SchoolManagement.Core.Models;

namespace SchoolManagement.Application.Services
{
    public class StudentQRService : CrudServiceBase<StudentQR>, IStudentQRService
    {
        public StudentQRService(IStudentQRRepository repository) : base(repository)
        {
        }
    }
}
