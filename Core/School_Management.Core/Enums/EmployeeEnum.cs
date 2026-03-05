using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace School_Management.Core.Enums
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
