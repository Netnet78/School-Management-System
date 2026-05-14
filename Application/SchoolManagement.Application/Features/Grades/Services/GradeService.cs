namespace SchoolManagement.Application.Features.Grades.Services
{
    public class GradeService : CrudServiceBase<Grade>, IGradeService
    {
        public GradeService(IGradeRepository repository) : base(repository)
        {
        }
    }
}


