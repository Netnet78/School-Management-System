using SchoolManagement.Core.Shared.Models;

namespace SchoolManagement.Core.Infrastructure.Interfaces;

public interface ISettingsService
{
    public string SettingsPath { get; }
    Settings GetAllSettings();
    void SaveAllSettings(Settings settings);
}