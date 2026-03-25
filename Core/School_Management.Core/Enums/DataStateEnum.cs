using System.ComponentModel;

namespace School_Management.Core.Enums
{
    public enum DataStateFilterOptions
    {
        [Description("គ្រប់ប្រភេទ")]
        All,
        [Description("គ្រប់គ្រាន់")]
        Completed,
        [Description("ខ្វះខាតទិន្នន័យ")]
        MissingData,
        [Description("ខ្វះខាតរូបភាព")]
        NoPicture,
        [Description("ខ្វះខាតទិន្នន័យ និងរូបភាព")]
        MissingDataAndPicture,
    }
}
