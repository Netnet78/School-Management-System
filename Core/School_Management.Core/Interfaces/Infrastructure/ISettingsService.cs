using School_Management.Core.Models;

namespace School_Management.Core.Interfaces.Infrastructure;

public interface ISettingsService
{
    public string SettingsPath { get; }
    Settings GetAllSettings();
    void SaveAllSettings(Settings settings);
}