using System.Text.Json;

namespace SchoolManagement.Infrastructure.Features.Files.Services
{
    public class SettingsService : ISettingsService
    {
        private JsonDocument? _json;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private bool _isInitialized;
        private object _initLock = new();

        public string SettingsPath { get; }

        public SettingsService()
        {
            string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string assemblyDirectory = Path.GetDirectoryName(assemblyPath) ?? string.Empty;
            SettingsPath = Path.Combine(assemblyDirectory, "appsettings.json");

            _jsonSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
        }

        private JsonDocument InitSettings()
        {
            if (!Path.Exists(SettingsPath))
                CreateSettingsFile();

            string jsonString = File.ReadAllText(SettingsPath);
            _json = JsonDocument.Parse(jsonString);
            _isInitialized = true;

            CheckPhotoPathConfig();
            return _json;
        }

        private void CheckPhotoPathConfig()
        {
            Settings previous = GetAllSettings();
            string baseDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "SchoolManagement"
                );

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
            EnsureInitialized();
            JsonElement root = _json?.RootElement ??
                throw new FileNotFoundException("មិនអាចរកឃើញ appsettings.json បានទេ!");
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
            EnsureInitialized();
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

        private void EnsureInitialized()
        {
            if (_isInitialized) return;
            lock (_initLock)
            {
                if (_isInitialized) return;
                InitSettings();
            }
        }
    }
}
