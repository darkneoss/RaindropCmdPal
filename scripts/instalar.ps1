<#
.SYNOPSIS
  Installs RaindropCmdPal on a Windows 11 PC for testing, without Developer Mode or dev tools.

.DESCRIPTION
  Run from an ADMIN PowerShell, in a folder that holds:
    RaindropCmdPal.cer and RaindropCmdPal_x64.msix and/or RaindropCmdPal_arm64.msix.
  What it does (and limpiar.ps1 undoes):
    1. Installs PowerToys via winget if Command Palette is missing.
    2. Trusts the test certificate in LocalMachine\TrustedPeople.
    3. Installs the extension .msix.
  Everything it changes is recorded in %LOCALAPPDATA%\RaindropCmdPal-test\state.json.

.EXAMPLE
  powershell -ExecutionPolicy Bypass -File .\instalar.ps1
#>
[CmdletBinding()]
param(
    [string]$Folder = $PSScriptRoot
)

$ErrorActionPreference = 'Stop'

function Write-Step($msg) { Write-Host "`n==> $msg" -ForegroundColor Cyan }

# --- Preconditions -----------------------------------------------------------------------------------------
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()
    ).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) { throw 'Run this script from a PowerShell opened as Administrator.' }

$build = [Environment]::OSVersion.Version.Build
if ($build -lt 22000) { throw "Windows 11 is required (build $build detected). Command Palette does not run on Windows 10." }

$arch = if ($env:PROCESSOR_ARCHITECTURE -eq 'ARM64') { 'arm64' } else { 'x64' }
$msix = Join-Path $Folder "RaindropCmdPal_$arch.msix"
$cer  = Join-Path $Folder 'RaindropCmdPal.cer'
foreach ($f in @($msix, $cer)) { if (-not (Test-Path $f)) { throw "Missing file: $f" } }

$stateDir  = Join-Path $env:LOCALAPPDATA 'RaindropCmdPal-test'
$statePath = Join-Path $stateDir 'state.json'
New-Item -ItemType Directory -Force -Path $stateDir | Out-Null
$state = [ordered]@{
    installedAt          = (Get-Date).ToString('s')
    powerToysInstalledByUs = $false
    certThumbprint       = $null
}

function Save-State { $state | ConvertTo-Json | Set-Content -Path $statePath -Encoding UTF8 }

# --- 1. PowerToys / Command Palette ------------------------------------------------------------------------
Write-Step 'Checking Command Palette'
if (Get-AppxPackage -Name 'Microsoft.CommandPalette' -ErrorAction SilentlyContinue) {
    Write-Host 'Command Palette is already installed; PowerToys will not be touched.'
} else {
    Write-Host 'Command Palette not found. Installing PowerToys with winget (~1 GB)...'
    winget install --id Microsoft.PowerToys --exact --accept-package-agreements --accept-source-agreements
    if ($LASTEXITCODE -ne 0) { throw "winget failed (exit $LASTEXITCODE)." }
    $state.powerToysInstalledByUs = $true
    Save-State
    Write-Host 'PowerToys installed. If Command Palette does not open with Win+Alt+Space, enable it in PowerToys settings.'
}

# --- 2. Trust the test certificate -------------------------------------------------------------------------
Write-Step 'Trusting the test certificate (LocalMachine\TrustedPeople)'
$imported = Import-Certificate -FilePath $cer -CertStoreLocation 'Cert:\LocalMachine\TrustedPeople'
$state.certThumbprint = $imported.Thumbprint
Save-State
Write-Host "Certificate trusted: $($imported.Subject)  [$($imported.Thumbprint)]"

# --- 3. Extension ----------------------------------------------------------------------------------------
Write-Step "Installing the extension ($arch)"
# The package is self-contained .NET 10 with no PackageDependency entries, so there is nothing else to install.
Add-AppxPackage -Path $msix -ForceApplicationShutdown
$pkg = Get-AppxPackage -Name 'darkneoss.RaindropCmdPal'
if (-not $pkg -or $pkg.Status -ne 'Ok') { throw 'The extension did not install correctly (Get-AppxPackage shows no Status=Ok).' }
Write-Host "Installed: $($pkg.PackageFullName)  Status=$($pkg.Status)" -ForegroundColor Green

Write-Host @"

Done. Next steps:
  1. Open Command Palette (Win+Alt+Space) and run "Reload" (or close and reopen it).
  2. Search for "Search Raindrop" -> extension settings -> paste your Raindrop token.
  3. Test: search, #tag search, Enter opens the link, context menu (Copy link / Open in Raindrop), icons.
  4. When finished: run limpiar.ps1 as admin, then REGENERATE the token at
     https://app.raindrop.io/settings/integrations
"@
