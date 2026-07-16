using System.ComponentModel;

namespace SchoolManagement.Core.Features.Employees.Enums
{
    public enum MaritalStatus
    {
        [Description("មានគ្រួសារ")]
        Married,
        [Description("នៅលីវ")]
        Single,
        [Description("បានលែងលះ")]
        Divorced,
        [Description("ពោះម៉ាយ/មេម៉ាយ")]
        Widowed
    }
}
