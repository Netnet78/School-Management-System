using Microsoft.Extensions.Options;
using System.Linq.Expressions;

namespace SchoolManagement.Application.Features.Students.Services
{
    public class StudentService : CrudServiceBase<Student>, IStudentService
    {
        private readonly IStudentRepository _studentRepositoy;
        private readonly IAuthorizationService _authorizationService;

        public StudentService(IStudentRepository studentRepository,
                              IAuthorizationService authorizationService) : base(studentRepository)
        {
            _studentRepositoy = studentRepository;
            _authorizationService = authorizationService;
        }
    }
}
