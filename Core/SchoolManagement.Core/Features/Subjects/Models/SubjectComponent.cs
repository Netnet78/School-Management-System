using SchoolManagement.Core.Features.Accessments.Models;
using SchoolManagement.Core.Shared.Contracts;
using System.ComponentModel;

namespace SchoolManagement.Core.Features.Subjects.Models
{
    [Description("ផ្នែកនៃមុខវិជ្ជា")]
    public class SubjectComponent : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public Subject Subject { get; set; } = null!;
        public ICollection<Score> Scores { get; set; } = [];
    }
}
