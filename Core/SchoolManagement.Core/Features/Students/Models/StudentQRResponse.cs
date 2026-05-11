using SchoolManagement.Core.Shared.Models;

namespace SchoolManagement.Core.Models
{
    public class StudentQRResponse : ReturnResponse
    {
        public Student? Student { get; set; }
    }
}
