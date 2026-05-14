namespace SchoolManagement.Application.Features.Students.Services
{
    public class StudentQRService : CrudServiceBase<StudentQR>, IStudentQRService
    {
        public StudentQRService(IStudentQRRepository repository) : base(repository)
        {
        }
    }
}


