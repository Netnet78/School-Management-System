using SchoolManagement.Core.Application.Interfaces;
using SchoolManagement.Core.Infrastructure.Interfaces;
using SchoolManagement.Core.Models;

namespace SchoolManagement.Application.Services
{
    public class ExamService : CrudServiceBase<Exam>, IExamService
    {
        public ExamService(IExamRepository repository) : base(repository)
        {
        }
    }
}
