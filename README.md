# Raindrop for Command Palette

Search your Raindrop.io bookmarks from the PowerToys Command Palette. Results open in your browser and offer actions to copy the link or edit the bookmark in Raindrop. The extension caches search results in memory and provides a Clear cache command.

## Prerequisites

- Windows 11, Microsoft PowerToys with Command Palette enabled, and Windows Developer Mode enabled.
- Visual Studio with the Windows application development and MSIX packaging tools, plus the Windows SDK. The current upstream template targets .NET 10; use Visual Studio 2026 and the .NET 10 SDK. Visual Studio 2022 cannot target .NET 10; using it requires retargeting and testing against a compatible SDK and toolkit version.

## Build and deploy

1. Open `RaindropExtension.sln` in Visual Studio and restore NuGet packages. Select x64 or ARM64 to match your machine.
2. Build the solution and select **Deploy** for the RaindropExtension project. Configure a local package signing certificate if Visual Studio requests one.
3. Open Command Palette and reload extensions (or restart Command Palette if needed). Open the Raindrop extension settings and enter your test token. Set the cache lifetime there as desired.

Get a test token from [Raindrop integrations](https://app.raindrop.io/settings/integrations). The token is stored in the Command Palette settings folder as plain text; protect access to your Windows account.

Based on [Garulf/flow-raindrop](https://github.com/Garulf/flow-raindrop). Packaging and initial assets are based on the Microsoft PowerToys Command Palette extension template.
