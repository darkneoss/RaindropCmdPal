# Verification evidence

No .NET SDK or Visual Studio is installed on this machine, so compilation, deployment, UI behavior, and live API requests could not be tested. The upstream template currently targets .NET 10; the README explains the Visual Studio version constraint. The original repository has no GitHub-detected license and no LICENSE or COPYING file in its tree; this project's MIT license is a new grant by darkneoss, not a claim about the original project.

CLSID verification (`rg -n 984C5EA0-1F84-4E75-BE60-D1E904424EE3 RaindropExtension`, exit 0):

```text
RaindropExtension/RaindropExtension.cs:12:[Guid("984C5EA0-1F84-4E75-BE60-D1E904424EE3")]
RaindropExtension/Package.appxmanifest:50:<com:Class Id="984C5EA0-1F84-4E75-BE60-D1E904424EE3" DisplayName="Raindrop" />
RaindropExtension/Package.appxmanifest:63:<CreateInstance ClassId="984C5EA0-1F84-4E75-BE60-D1E904424EE3" />
```

PowerToys source checked at `src/modules/cmdpal/` on GitHub main:

- Template extension implementation, IExtension, ProviderType, COM server registration, project and manifest: `ExtensionTemplate/TemplateCmdPalExtension/TemplateCmdPalExtension/{TemplateCmdPalExtension.cs,Program.cs,TemplateCmdPalExtensionCommandsProvider.cs,TemplateCmdPalExtension.csproj,Package.appxmanifest}`.
- `CommandProvider`, `CommandItem`, `CommandContextItem`, `ListItem`, `DynamicListPage`, `ListPage`, `Page`, `IconInfo`, `IconHelpers`, `Tag`, `JsonSettingsManager`, `Settings`, `TextSetting`, `ChoiceSetSetting`, and `Utilities`: corresponding `.cs` files under `extensionsdk/Microsoft.CommandPalette.Extensions.Toolkit/`.
- `CopyTextCommand`, `OpenUrlCommand`, `NoOpCommand`: corresponding files under `extensionsdk/Microsoft.CommandPalette.Extensions.Toolkit/Commands/`.
- Dynamic search override and notifications: `ext/SamplePagesExtension/Pages/SampleDynamicListPage.cs`.
- Settings manager initialization and standard path: `ExtensionTemplate/TemplateCmdPalExtension/TemplateCmdPalExtension/Helpers/SettingsManager.cs`.

The item-edit route is used by Raindrop's own app source at `https://github.com/raindropio/app/blob/0af0e212b39a5323a12c8d653a05246cc16041d7/src/co/stella/index.js`. Whether the host renders remote cover URLs and falls back after a remote image load fails remains unverified; missing and invalid cover URLs explicitly use the template icon.

File tree (`rg --files`, excluding .gitignore):

```text
Directory.Build.props
Directory.Packages.props
LICENSE
nuget.config
RaindropExtension.sln
README.md
VERIFICATION.md
RaindropExtension/app.manifest
RaindropExtension/Assets/{LockScreenLogo.scale-200.png,SplashScreen.scale-200.png,Square150x150Logo.scale-200.png,Square44x44Logo.scale-200.png,Square44x44Logo.targetsize-24_altform-unplated.png,StoreLogo.png,Wide310x150Logo.scale-200.png}
RaindropExtension/Helpers/SettingsManager.cs
RaindropExtension/Icons.cs
RaindropExtension/Package.appxmanifest
RaindropExtension/Pages/RaindropSearchPage.cs
RaindropExtension/Program.cs
RaindropExtension/Properties/launchSettings.json
RaindropExtension/Properties/PublishProfiles/{win-arm64.pubxml,win-x64.pubxml}
RaindropExtension/RaindropExtension.cs
RaindropExtension/RaindropExtension.csproj
RaindropExtension/RaindropExtensionCommandsProvider.cs
RaindropExtension/Services/Raindrop.cs
RaindropExtension/Services/RaindropClient.cs
```

After the readability refactor, C# line counts are: `Icons.cs` 6, `Program.cs` 41,
`RaindropExtension.cs` 35, `RaindropExtensionCommandsProvider.cs` 33,
`Helpers/SettingsManager.cs` 48, `Pages/RaindropSearchPage.cs` 174,
`Services/Raindrop.cs` 12, and `Services/RaindropClient.cs` 60. All C# lines
are at most 120 characters. The source-level checks do not replace a build or
runtime test.
