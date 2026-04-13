using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;
using System.Text.Json;

namespace School_Management.Infrastructure.Services
{
    public class SettingsService : ISettingsService
    {
        private JsonDocument _json;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public string SettingsPath { get; }

        public SettingsService()
        {
            string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string assemblyDirectory = Path.GetDirectoryName(assemblyPath) ?? string.Empty;
            SettingsPath = Path.Combine(assemblyDirectory, "appsettings.json");

            if (!Path.Exists(SettingsPath))
            {
                CreateSettingsFile();
            }

            string jsonString = File.ReadAllText(SettingsPath);
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
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

            Directory.CreateDirectory(previous.StudentPhotoFolderPath);
            Directory.CreateDirectory(previous.EmployeePhotoFolderPath);
        }

        private void CreateSettingsFile()
        {
            File.Create(SettingsPath);
            var obj = new
            {
                Theme = string.Empty,
                Storage = new
                {
                    StudentPhotoFolderPath = string.Empty,
                    StudentPhotoFolderBucketPath = string.Empty,
                    EmployeePhotoFolderPath = string.Empty,
                    EmployeePhotoFolderBucketPath = string.Empty,
                    BucketName = string.Empty,
                }
            };

            string json = JsonSerializer.Serialize(obj, _jsonSerializerOptions);
            File.WriteAllText(SettingsPath, json);
        }

        public Settings GetAllSettings()
        {
            JsonElement root = _json.RootElement;
            JsonElement storage = root.GetProperty("Storage");
            return new Settings
            {
                Theme = root.GetProperty("Theme").GetString() ?? "Light",
                StudentPhotoFolderPath = storage.GetProperty("StudentPhotoFolderPath").GetString() ?? string.Empty,
                StudentPhotoFolderBucketPath = storage.GetProperty("StudentPhotoFolderBucketPath").GetString() ?? string.Empty,
                EmployeePhotoFolderPath = storage.GetProperty("EmployeePhotoFolderPath").GetString() ?? string.Empty,
                EmployeePhotoFolderBucketPath = storage.GetProperty("EmployeePhotoFolderBucketPath").GetString() ?? string.Empty,
                BucketName = storage.GetProperty("BucketName").GetString() ?? string.Empty,
            };
        }

        public void SaveAllSettings(Settings settings)
        {
            var obj = new
            {
                Theme = settings.Theme,
                Storage = new
                {
                    settings.StudentPhotoFolderPath,
                    settings.StudentPhotoFolderBucketPath,
                    settings.EmployeePhotoFolderPath,
                    settings.EmployeePhotoFolderBucketPath,
                    settings.BucketName,
                }
            };

            string json = JsonSerializer.Serialize(obj, _jsonSerializerOptions);
            File.WriteAllText(SettingsPath, json);

            string jsonString = File.ReadAllText(SettingsPath);
            _json = JsonDocument.Parse(jsonString);
        }
    }
}