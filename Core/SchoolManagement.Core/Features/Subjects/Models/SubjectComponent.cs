using SchoolManagement.Core.Shared.Contracts;
using System.ComponentModel;

namespace SchoolManagement.Core.Features.Subjects.Models
{
    [Description("ផ្នែកនៃមុខវិជ្ជា")]
    public class SubjectComponent : IEntity, IAuditableEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string KhmerName { get; set; } = string.Empty;
        public List<SubjectMapper> Mappers { get; set; } = [];

        public string CustomAuditDescription()
        {
            return "";
        }

        public string CustomAuditName()
        {
            return $"{KhmerName}";
        }
    }
}
