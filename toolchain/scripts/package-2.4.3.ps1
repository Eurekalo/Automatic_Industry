param(
    [string]$WorkspaceRoot = (Resolve-Path "$PSScriptRoot\..\..").Path,
    [string]$Version = "2.4.3"
)

$ErrorActionPreference = "Stop"

$srcFolder = Join-Path $WorkspaceRoot "AutomaticIndustry-src-2.4.0\AutomaticIndustry-src-2.4.0"
$csproj = Join-Path $srcFolder "AutoMachineRebuilt.csproj"
$managedDir = Join-Path $WorkspaceRoot "Managed\Managed"
$archiveDir = Join-Path $WorkspaceRoot "zip_src_archived"

if (-not (Test-Path $archiveDir)) {
    New-Item -ItemType Directory -Path $archiveDir | Out-Null
}

Write-Host "=== 1. Archiving Previous Release and Source Zips ==="
Get-ChildItem -Path $WorkspaceRoot -Filter "AutomaticIndustry-*.zip" -File | ForEach-Object {
    if ($_.Name -ne "AutomaticIndustry-$Version.zip" -and $_.Name -ne "AutomaticIndustry-src-$Version.zip") {
        $dest = Join-Path $archiveDir $_.Name
        Move-Item -Path $_.FullName -Destination $dest -Force
        Write-Host "  Archived: $($_.Name) -> zip_src_archived/"
    }
}

# Clean up any leftover uncompressed release/src folders from older releases
Get-ChildItem -Path $WorkspaceRoot -Directory | Where-Object {
    ($_.Name -match '^AutomaticIndustry-\d+\.\d+\.\d+$' -or $_.Name -match '^AutomaticIndustry-src-\d+\.\d+\.\d+$') -and
    $_.Name -ne "AutomaticIndustry-src-2.4.0"
} | ForEach-Object {
    Write-Host "  Removing uncompressed folder: $($_.Name)"
    Remove-Item -Path $_.FullName -Recurse -Force
}

Write-Host "=== 2. Building Release Assembly with ILRepack ($Version) ==="
& dotnet build $csproj -c Release --no-incremental -p:ONIManaged=$managedDir
if ($LASTEXITCODE -ne 0) {
    throw "Build failed!"
}

$builtDll = Join-Path $srcFolder "bin\Release\net48\AutomaticIndustry.dll"
if (-not (Test-Path $builtDll)) {
    throw "Output DLL not found: $builtDll"
}

$dllSize = (Get-Item $builtDll).Length
Write-Host "Output DLL size: $dllSize bytes"

Write-Host "=== 3. Staging and Creating Release ZIP Archive ==="
$tempDir = Join-Path $WorkspaceRoot "toolchain\temp_staging"
if (Test-Path $tempDir) {
    Remove-Item -Recurse -Force $tempDir
}
$tempRel = Join-Path $tempDir "release"
$tempSrc = Join-Path $tempDir "src"
New-Item -ItemType Directory -Path $tempRel -Force | Out-Null
New-Item -ItemType Directory -Path $tempSrc -Force | Out-Null

Copy-Item $builtDll (Join-Path $tempRel "AutomaticIndustry.dll")
Copy-Item (Join-Path $srcFolder "mod.yaml") (Join-Path $tempRel "mod.yaml")
Copy-Item (Join-Path $srcFolder "mod_info.yaml") (Join-Path $tempRel "mod_info.yaml")
Copy-Item (Join-Path $srcFolder "LICENSE") (Join-Path $tempRel "LICENSE")

Write-Host "=== 4. Staging and Creating Source ZIP Archive ==="
Get-ChildItem -Path $srcFolder -Exclude "bin", "obj", ".vs" | ForEach-Object {
    Copy-Item -Path $_.FullName -Destination $tempSrc -Recurse -Force
}

$zipRel = Join-Path $WorkspaceRoot "AutomaticIndustry-$Version.zip"
$zipSrc = Join-Path $WorkspaceRoot "AutomaticIndustry-src-$Version.zip"

if (Test-Path $zipRel) { Remove-Item -Force $zipRel }
if (Test-Path $zipSrc) { Remove-Item -Force $zipSrc }

Compress-Archive -Path (Join-Path $tempRel "*") -DestinationPath $zipRel
Compress-Archive -Path (Join-Path $tempSrc "*") -DestinationPath $zipSrc

# Clean up temp staging directory so no uncompressed release/src folders remain
Remove-Item -Recurse -Force $tempDir

Write-Host "=== Packaging Completed Successfully (Version $Version) ==="
Write-Host "Release zip: $zipRel ($((Get-Item $zipRel).Length) bytes)"
Write-Host "Source zip : $zipSrc ($((Get-Item $zipSrc).Length) bytes)"
Write-Host "Archived folder: $archiveDir"

