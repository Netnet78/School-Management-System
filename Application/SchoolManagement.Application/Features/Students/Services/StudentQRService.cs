namespace SchoolManagement.Application.Features.Students.Services
{
    public class StudentQRService : CrudServiceBase<StudentQR>, IStudentQRService
    {
        private readonly IStudentQRRepository studentQRRepository;
        public StudentQRService(IStudentQRRepository repository) : base(repository)
        {
            studentQRRepository = repository;
        }

        public async Task<ReturnResponse<StudentQR>> GetQRByStudentId(int id)
        {
            try
            {
                StudentQR? qr = (await studentQRRepository.FindAsync([
                        new(s => s.Student.Id, FilterOperator.Equals, id)
                    ])).FirstOrDefault();

                if (qr == null)
                {
                    return new()
                    {
                        Status = Status.Rejected,
                        Message = $"Couldn't find QR value based on the student id!"
                    };
                }
                else
                {
                    return new()
                    {
                        Status = Status.Success,
                        Value = qr
                    };
                }
            }
            catch (Exception ex)
            {
                return new()
                {
                    Status = Status.Failed,
                    Message = $"Failed to get QR by student id! \n{ex.Message}"
                };
            }
        }
    }
}


