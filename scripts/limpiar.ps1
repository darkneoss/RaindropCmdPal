<#
.SYNOPSIS
  Undoes everything instalar.ps1 did, leaving the borrowed PC as it was.

.DESCRIPTION
  Run from an ADMIN PowerShell. Reads %LOCALAPPDATA%\RaindropCmdPal-test\state.json to know what was changed:
    1. Uninstalls the extension and deletes its settings (they contain your Raindrop token in plain text).
    2. Removes the test certificate (only the one that was imported, by thumbprint).
    3. Uninstalls PowerToys ONLY if instalar.ps1 installed it.
  After this, regenerate your token at https://app.raindrop.io/settings/integrations anyway.

.EXAMPLE
  powershell -ExecutionPolicy Bypass -File .\limpiar.ps1
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Continue'

function Write-Step($msg) { Write-Host "`n==> $msg" -ForegroundColor Cyan }

$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()
    ).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) { throw 'Run this script from a PowerShell opened as Administrator.' }

$stateDir  = Join-Path $env:LOCALAPPDATA 'RaindropCmdPal-test'
$statePath = Join-Path $stateDir 'state.json'
$state = if (Test-Path $statePath) { Get-Content $statePath -Raw | ConvertFrom-Json } else { $null }
if (-not $state) { Write-Warning 'No state.json found: will only remove the extension and its settings.' }

# --- 1. Extension + settings -------------------------------------------------------------------------------
Write-Step 'Removing the extension'
$pkg = Get-AppxPackage -Name 'darkneoss.RaindropforCommandPalette'
if ($pkg) {
    $familyName = $pkg.PackageFamilyName
    Remove-AppxPackage -Package $pkg.PackageFullName
    Write-Host "Removed $($pkg.PackageFullName)"
} else {
    $familyName = $null
    Write-Host 'The extension was not installed.'
}

Write-Step 'Deleting settings (contain the token)'
# Settings live in the package's app data:
#   %LOCALAPPDATA%\Packages\darkneoss.RaindropforCommandPalette_1egvxjvtdrd2w\LocalState\settings.json
# Remove-AppxPackage normally deletes this folder; this makes sure, even if the uninstall failed.
$settingsDir = Join-Path $env:LOCALAPPDATA 'Packages\darkneoss.RaindropforCommandPalette_1egvxjvtdrd2w'
if (Test-Path $settingsDir) {
    Remove-Item -Recurse -Force $settingsDir
    Write-Host "Deleted $settingsDir"
} else {
    Write-Host 'No settings folder left (Windows already removed it).'
}

# --- 2. Certificate ----------------------------------------------------------------------------------------
Write-Step 'Removing the test certificate'
$thumb = if ($state) { $state.certThumbprint } else { $null }
if ($thumb -and (Test-Path "Cert:\LocalMachine\TrustedPeople\$thumb")) {
    Remove-Item "Cert:\LocalMachine\TrustedPeople\$thumb"
    Write-Host "Removed certificate $thumb"
} else {
    Get-ChildItem 'Cert:\LocalMachine\TrustedPeople' |
        Where-Object { $_.Subject -eq 'CN=042B73EC-9E03-4437-8640-580CF72BA404' } |
        ForEach-Object { Remove-Item $_.PSPath; Write-Host "Removed certificate $($_.Thumbprint)" }
}

# --- 3. PowerToys (only if we installed it) ----------------------------------------------------------------
if ($state -and $state.powerToysInstalledByUs) {
    Write-Step 'Uninstalling PowerToys (instalar.ps1 installed it)'
    winget uninstall --id Microsoft.PowerToys --exact
} else {
    Write-Host "`nPowerToys was already on this PC; it is left untouched."
}

Remove-Item -Recurse -Force $stateDir -ErrorAction SilentlyContinue

Write-Host @"

Cleanup finished.
REMEMBER: regenerate your Raindrop token at https://app.raindrop.io/settings/integrations
"@ -ForegroundColor Green
