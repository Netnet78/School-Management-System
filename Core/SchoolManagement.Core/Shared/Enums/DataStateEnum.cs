using System.ComponentModel;

namespace SchoolManagement.Core.Shared.Enums
{
    /// <summary>
    /// This is an enum that describes the data state of an object (is it missing some data or not)
    /// <list>Completed = If the object doesn't contain any missing data</list>
    /// <list>MissingData = If the object does contain any missing data</list>
    /// <list>All = Both 'Completed' and 'MissingData'</list>
    /// </summary>
    public enum DataStateFilterOptions
    {
        [Description("គ្រប់ប្រភេទ")]
        All,
        [Description("គ្រប់គ្រាន់")]
        Completed,
        [Description("ខ្វះទិន្នន័យ")]
        MissingData,
    }
    /// <summary>
    /// This is an enum that describes the data state of an object (is it missing some data or not).
    /// It is used to help differentiate the <strong>Student</strong> or <strong>Candidate</strong> object
    /// if they have enough information or not.
    /// <list>Completed = If the student doesn't contain any missing data</list>
    /// <list>MissingData = If the student does contain any missing data</list>
    /// <list>NoPicture = If the student does not have a picture key</list>
    /// <list>MissingDataAndPicture = If the student does not contain all information and a picture key</list>
    /// <list>All = All of the above combined</list>
    /// </summary>
    public enum StudentDataStateFilterOptions
    {
        [Description("គ្រប់ប្រភេទ")]
        All,
        [Description("គ្រប់គ្រាន់")]
        Completed,
        [Description("ខ្វះទិន្នន័យ")]
        MissingData,
        [Description("ខ្វះរូបភាព")]
        NoPicture,
        [Description("ខ្វះទិន្នន័យ និងរូបភាព")]
        MissingDataAndPicture,
    }
}
