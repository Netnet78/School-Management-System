namespace SchoolManagement.Application.Features.Students.Services
{
    public class StudentClassService : CrudServiceBase<StudentClass>, IStudentClassService
    {
        public StudentClassService(IStudentClassRepository repository) : base(repository)
        {
        }
    }
}


