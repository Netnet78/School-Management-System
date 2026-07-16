namespace SchoolManagement.Application.Features.Exams.Services
{
    public class ExamService : CrudServiceBase<Exam>, IExamService
    {
        public ExamService(IExamRepository repository) : base(repository)
        {
        }
    }
}


