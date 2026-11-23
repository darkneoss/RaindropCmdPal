using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace RaindropExtension;

public partial class RaindropExtensionCommandsProvider : CommandProvider
{
    private readonly ICommandItem[] _commands;
    private readonly SettingsManager _settings = new();

    public RaindropExtensionCommandsProvider()
    {
        DisplayName = "Raindrop";
        Icon = IconHelpers.FromRelativePath(Icons.Logo);
        var page = new RaindropSearchPage(_settings);
        _commands =
        [
            new CommandItem(page)
            {
                Title = "Search Raindrop",
                MoreCommands = [new CommandContextItem(_settings.Settings.SettingsPage)],
            },
            new CommandItem(new OpenUrlCommand("https://app.raindrop.io"))
            {
                Title = "Open Raindrop.io",
            },
            new CommandItem("Clear cache", action: page.ClearCache),
        ];
        Settings = _settings.Settings;
    }

    public override ICommandItem[] TopLevelCommands() => _commands;
}
