using SchoolManagement.Core.Features.Classes.Models;
using SchoolManagement.Core.Shared.Contracts;
using System.ComponentModel;

namespace SchoolManagement.Core.Features.Grades.Models
{
    [Description("កម្រិត")]
    public class Grade : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string KhmerName { get; set; } = string.Empty;
        public List<Class> Classes { get; set; } = [];
    }
}
