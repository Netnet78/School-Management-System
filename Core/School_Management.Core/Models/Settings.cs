namespace School_Management.Core.Models
{
    public class Settings
    {
        private static string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        private static string assemblyDirectory = Path.GetDirectoryName(assemblyPath) ?? string.Empty;

        public string SettingsPath { get; } = Path.Combine(assemblyDirectory, "appsettings.json");
        public string Theme { get; set; } = string.Empty;
        public string StudentPhotoFolderPath { get; set; } = string.Empty;
        public string EmployeePhotoFolderPath { get; set; } = string.Empty;
        public string StudentPhotoFolderBucketPath { get; set; } = string.Empty;
        public string EmployeePhotoFolderBucketPath { get; set; } = string.Empty;
        public string BucketName { get; set; } = string.Empty;
    }
}
