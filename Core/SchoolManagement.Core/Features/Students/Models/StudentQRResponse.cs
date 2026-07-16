using SchoolManagement.Core.Shared.Models;

namespace SchoolManagement.Core.Features.Students.Models
{
    public class StudentQRResponse : ReturnResponse
    {
        public Student? Student { get; set; }
    }
}
