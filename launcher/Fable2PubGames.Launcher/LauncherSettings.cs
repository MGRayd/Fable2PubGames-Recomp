using System.Text.Json;

namespace Fable2PubGames.Launcher;

internal sealed class LauncherSettings
{
    public bool Windowed { get; set; } = true;
    public int Width { get; set; } = 1280;
    public int Height { get; set; } = 720;
    public bool ReliableTextRendering { get; set; } = true;
    public bool DetailedLogs { get; set; }
    public bool SyncFable2Saves { get; set; }
    public string Fable2SavesPath { get; set; } = string.Empty;
}

internal static class LauncherSettingsStore
{
    private static string SettingsPath => Path.Combine(AppContext.BaseDirectory, "launcher-settings.json");

    public static LauncherSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsPath)) return new LauncherSettings();
            return JsonSerializer.Deserialize<LauncherSettings>(File.ReadAllText(SettingsPath))
                   ?? new LauncherSettings();
        }
        catch
        {
            return new LauncherSettings();
        }
    }

    public static void Save(LauncherSettings settings)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(settings, options));
    }
}
