# Raindrop for Command Palette

Search your Raindrop.io bookmarks from the PowerToys Command Palette. Results open in your browser and offer actions to copy the link or edit the bookmark in Raindrop. The extension caches search results in memory and provides a Clear cache command.

## Prerequisites

- **To run it:** Windows 11 with Microsoft PowerToys (Command Palette enabled). Command Palette does not exist on
  Windows 10 or in PowerToys versions before it shipped.
- **To build it:**
  - .NET 10 SDK (tested with 10.0.404).
  - Windows SDK **10.0.26100.0**. Without it, the build fails in `cswinrt.exe` (missing `Platform.xml`).
  - Visual Studio 2022 Build Tools (or VS 2026) for the Windows/MSIX build targets. The IDE itself is optional.
- **To install the unsigned dev build:** Windows Developer Mode, which needs admin once. Without admin, you need a
  signed `.msix` and its certificate trusted on the machine, which also takes an admin once.

## Build and deploy

Build with the .NET SDK, not with Visual Studio 2022's `msbuild.exe`. VS 2022's MSBuild runs on .NET Framework and
cannot load the .NET 10 SDK tasks (it fails with `MSB4018 ... Microsoft.Deployment.DotNet.Releases`).

```powershell
dotnet build RaindropExtension.sln -p:Configuration=Debug -p:Platform=x64
Add-AppxPackage -Register .\RaindropExtension\bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64\AppxManifest.xml
Get-AppxPackage darkneoss.RaindropCmdPal   # expect Status: Ok
```

Use `-p:Platform=ARM64` (and the matching `win-arm64` output folder) on ARM machines. Visual Studio 2026's
**Deploy** does the same job as the register step.

Then open Command Palette and run **Reload**. Open the Raindrop extension settings, enter your token, and set the
cache lifetime there if you want.

Verified so far: build + package registration on Windows 10 (0 errors), and the Raindrop API calls this extension
makes (search, `#tag` search, empty results, 401 on a bad token). Not yet verified: the UI inside Command Palette.
See `VERIFICATION.md`.

Get a test token from [Raindrop integrations](https://app.raindrop.io/settings/integrations). The token is stored in the Command Palette settings folder as plain text; protect access to your Windows account.

Based on [Garulf/flow-raindrop](https://github.com/Garulf/flow-raindrop). Packaging and initial assets are based on the Microsoft PowerToys Command Palette extension template.
