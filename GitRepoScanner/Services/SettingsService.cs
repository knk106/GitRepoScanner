using System.IO;
using System.Text.Json;
using GitRepoScanner.Models;

namespace GitRepoScanner.Services;

public class SettingsService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };

    private readonly string _settingsPath;
    private readonly string _reposPath;

    public SettingsService()
    {
        var exeDir = AppContext.BaseDirectory;
        _settingsPath = Path.Combine(exeDir, "settings.json");
        _reposPath    = Path.Combine(exeDir, "repos.json");
    }

    public AppSettings LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsPath))
                return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(_settingsPath)) ?? new();
        }
        catch { }
        return new AppSettings();
    }

    public void SaveSettings(AppSettings settings)
    {
        try { File.WriteAllText(_settingsPath, JsonSerializer.Serialize(settings, JsonOpts)); }
        catch { }
    }

    public List<GitRepo> LoadRepos()
    {
        try
        {
            if (File.Exists(_reposPath))
                return JsonSerializer.Deserialize<List<GitRepo>>(File.ReadAllText(_reposPath)) ?? [];
        }
        catch { }
        return [];
    }

    public void SaveRepos(List<GitRepo> repos)
    {
        try { File.WriteAllText(_reposPath, JsonSerializer.Serialize(repos, JsonOpts)); }
        catch { }
    }
}
