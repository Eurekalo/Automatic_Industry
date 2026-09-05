# setup-environment.ps1
# Sets up the complete development environment for AutomaticIndustry mod.
# Creates:
#   1. Symlink of Managed DLLs into the toolchain build path
#   2. Verifies ilspycmd is installed
#   3. Runs the decompilation of game types
#   4. Creates the sandbox project for testing

param(
    [string]$GameManagedDir = "",
    [switch]$SkipDecompile
)

$ErrorActionPreference = "Stop"
$Root = Resolve-Path "$PSScriptRoot\..\.."

Write-Host "==============================================" -ForegroundColor Cyan
Write-Host " AutomaticIndustry Development Environment Setup" -ForegroundColor Cyan
Write-Host "==============================================" -ForegroundColor Cyan

# --- Step 1: Verify game assemblies ---
Write-Host "`n[1/5] Verifying game assemblies..." -ForegroundColor Yellow

$localManaged = Join-Path $Root "Managed\Managed"
if (Test-Path (Join-Path $localManaged "Assembly-CSharp.dll")) {
    Write-Host "  Found local Managed directory: $localManaged" -ForegroundColor Green
    $ManagedDir = $localManaged
} elseif ($GameManagedDir -and (Test-Path (Join-Path $GameManagedDir "Assembly-CSharp.dll"))) {
    Write-Host "  Using provided game managed dir: $GameManagedDir" -ForegroundColor Green
    $ManagedDir = $GameManagedDir
} else {
    Write-Host "  ERROR: No game assemblies found!" -ForegroundColor Red
    Write-Host "  Provide -GameManagedDir pointing to OxygenNotIncluded_Data/Managed" -ForegroundColor Red
    exit 1
}

# --- Step 2: Verify ilspycmd ---
Write-Host "`n[2/5] Verifying ilspycmd..." -ForegroundColor Yellow
try {
    $version = & ilspycmd --version 2>&1 | Select-Object -First 1
    Write-Host "  ilspycmd: $version" -ForegroundColor Green
} catch {
    Write-Host "  ilspycmd not found, installing..." -ForegroundColor Yellow
    dotnet tool install --global ilspycmd
}

# --- Step 3: Create toolchain/Managed symlink for build ---
Write-Host "`n[3/5] Setting up toolchain/Managed for build..." -ForegroundColor Yellow
$toolchainManaged = Join-Path $Root "toolchain\Managed"
if (!(Test-Path $toolchainManaged)) {
    try {
        New-Item -ItemType Junction -Path $toolchainManaged -Target $ManagedDir -Force | Out-Null
        Write-Host "  Created junction: $toolchainManaged -> $ManagedDir" -ForegroundColor Green
    } catch {
        Write-Host "  Junction failed, copying key DLLs instead..." -ForegroundColor Yellow
        New-Item -ItemType Directory -Path $toolchainManaged -Force | Out-Null
        $keyDlls = @(
            "Assembly-CSharp.dll", "Assembly-CSharp-firstpass.dll",
            "UnityEngine.dll", "UnityEngine.CoreModule.dll",
            "0Harmony.dll", "Newtonsoft.Json.dll", "mscorlib.dll",
            "System.dll", "System.Core.dll", "netstandard.dll"
        )
        foreach ($dll in $keyDlls) {
            $src = Join-Path $ManagedDir $dll
            if (Test-Path $src) {
                Copy-Item $src $toolchainManaged
            }
        }
        Write-Host "  Copied $(($keyDlls | Where-Object { Test-Path (Join-Path $ManagedDir $_) }).Count) DLLs" -ForegroundColor Green
    }
}

# --- Step 4: Decompile game types ---
if (!$SkipDecompile) {
    Write-Host "`n[4/5] Decompiling game types..." -ForegroundColor Yellow
    $decompileScript = Join-Path $PSScriptRoot "decompile-game-types.ps1"
    & pwsh -File $decompileScript
} else {
    Write-Host "`n[4/5] Skipping decompilation (use without -SkipDecompile to run)" -ForegroundColor DarkGray
}

# --- Step 5: Verify source project ---
Write-Host "`n[5/5] Verifying source project..." -ForegroundColor Yellow
$srcProject = Join-Path $Root "AutomaticIndustry-src-2.4.0\AutomaticIndustry-src-2.4.0\AutoMachineRebuilt.csproj"
if (Test-Path $srcProject) {
    Write-Host "  Source project found: $srcProject" -ForegroundColor Green
} else {
    Write-Host "  WARNING: Source project not found at expected location" -ForegroundColor Yellow
}

Write-Host "`n==============================================" -ForegroundColor Cyan
Write-Host " Environment setup complete!" -ForegroundColor Cyan
Write-Host "==============================================" -ForegroundColor Cyan
Write-Host @"

Quick Reference:
  Managed DLLs:          $ManagedDir
  Decompiled sources:    $(Join-Path $Root 'toolchain\decompiled')
  Sandbox project:       $(Join-Path $Root 'toolchain\sandbox')
  Source project:        $srcProject

To build the mod:
  cd $(Split-Path $srcProject)
  dotnet build -p:ONIManaged="$ManagedDir"

To decompile additional types:
  ilspycmd "$ManagedDir\Assembly-CSharp.dll" -t TypeName

"@
