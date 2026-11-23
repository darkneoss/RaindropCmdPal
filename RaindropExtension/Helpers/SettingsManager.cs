using System;
using System.IO;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace RaindropExtension;

internal sealed partial class SettingsManager : JsonSettingsManager
{
    private readonly TextSetting _token = new("api_token", "API token", "Raindrop test token", "");

    private readonly ChoiceSetSetting _ttl = new(
        "cache_ttl", "Cache lifetime", "How long search results are cached",
        [
            new("5 minutes", "5m"),
            new("15 minutes", "15m"),
            new("30 minutes", "30m"),
            new("1 hour", "1h"),
            new("6 hours", "6h"),
            new("12 hours", "12h"),
            new("1 day", "1d"),
            new("3 days", "3d"),
        ]);

    public string Token => _token.Value ?? string.Empty;

    public TimeSpan Ttl => _ttl.Value switch
    {
        "15m" => TimeSpan.FromMinutes(15),
        "30m" => TimeSpan.FromMinutes(30),
        "1h" => TimeSpan.FromHours(1),
        "6h" => TimeSpan.FromHours(6),
        "12h" => TimeSpan.FromHours(12),
        "1d" => TimeSpan.FromDays(1),
        "3d" => TimeSpan.FromDays(3),
        _ => TimeSpan.FromMinutes(5),
    };

    public SettingsManager()
    {
        var directory = Utilities.BaseSettingsPath("RaindropExtension");
        Directory.CreateDirectory(directory);
        FilePath = Path.Combine(directory, "settings.json");
        Settings.Add(_token);
        Settings.Add(_ttl);
        LoadSettings();
        Settings.SettingsChanged += (_, _) => SaveSettings();
    }
}
