using School_Management.Core.Interfaces.Application;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;

namespace School_Management.Application.Services
{
    public class ExamService : CrudServiceBase<Exam>, IExamService
    {
        public ExamService(IExamRepository repository) : base(repository)
        {
        }
    }
}
