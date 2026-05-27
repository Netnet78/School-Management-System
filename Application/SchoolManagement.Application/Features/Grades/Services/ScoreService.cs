using SchoolManagement.Core.Features.Accessments.Models;

namespace SchoolManagement.Application.Features.Grades.Services
{
    public class ScoreService : CrudServiceBase<Assessment>, IAccessmentService
    {
        public ScoreService(IAccessmentRepository repository) : base(repository)
        {
        }
    }
}


