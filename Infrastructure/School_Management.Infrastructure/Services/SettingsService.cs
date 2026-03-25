using System.Text.Json;
using School_Management.Core.Interfaces;
using School_Management.Core.Models;

namespace School_Management.Infrastructure.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly JsonDocument _json;

        public SettingsService()
        {
            string jsonString = File.ReadAllText("appsettings.json");
            _json = JsonDocument.Parse(jsonString);

            CheckPhotoPathConfig();
        }

        private void CheckPhotoPathConfig()
        {
            Settings previous = GetAllSettings();
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            bool isChanged = false;

            if (string.IsNullOrWhiteSpace(previous.StudentPhotoFolderPath))
            {
                previous.StudentPhotoFolderPath = Path.Combine(baseDirectory, "photos", "students");
                isChanged = true;
            }

            if (string.IsNullOrWhiteSpace(previous.EmployeePhotoFolderPath))
            {
                previous.EmployeePhotoFolderPath = Path.Combine(baseDirectory, "photos", "employees");
                isChanged = true;
            }

            if (isChanged)
            {
                SaveAllSettings(previous);
            }
        }

        public Settings GetAllSettings()
        {
            JsonElement root = _json.RootElement;
            JsonElement storage = _json.RootElement.GetProperty("Storage");
            return new Settings
            {
                Theme = root.GetProperty("Theme").GetString() ?? "Light",
                StudentPhotoFolderPath = storage.GetProperty("StudentPhotoFolderPath").GetString() ?? "",
                StudentPhotoFolderBucketPath = storage.GetProperty("StudentPhotoFolderBucketPath").GetString() ?? "",
                EmployeePhotoFolderPath = storage.GetProperty("EmployeePhotoFolderPath").GetString() ?? "",
                EmployeePhotoFolderBucketPath = storage.GetProperty("EmployeePhotoFolderBucketPath").GetString() ?? "",
            };
        }

        public void SaveAllSettings(Settings settings)
        {
            var obj = new
            {
                Storage = settings
            };

            var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText("appsettings.json", json);
        }
    }
}
