using School_Management.Core.Models;

namespace School_Management.Core.Interfaces;

public interface ISettingsService
{
    Settings GetAllSettings();
    void SaveAllSettings(Settings settings);
}