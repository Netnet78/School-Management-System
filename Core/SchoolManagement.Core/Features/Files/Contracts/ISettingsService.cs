using SchoolManagement.Core.Shared.Models;

namespace SchoolManagement.Core.Features.Files.Contracts;

public interface ISettingsService
{
    public string SettingsPath { get; }
    Settings GetAllSettings();
    void SaveAllSettings(Settings settings);
}
