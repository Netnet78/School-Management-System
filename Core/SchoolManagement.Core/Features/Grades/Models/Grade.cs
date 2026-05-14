using SchoolManagement.Core.Features.Classes.Models;
using SchoolManagement.Core.Shared.Contracts;

namespace SchoolManagement.Core.Features.Grades.Models
{
    public class Grade : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string KhmerName { get; set; } = string.Empty;
        public ICollection<Class> Classes { get; set; } = [];
    }
}
