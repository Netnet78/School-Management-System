namespace SchoolManagement.Infrastructure.Features.Files.Contracts;

public interface ISettingsService
{
    public string SettingsPath { get; }
    Settings GetAllSettings();
    void SaveAllSettings(Settings settings);
}
