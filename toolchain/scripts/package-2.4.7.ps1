param(
    [string]$WorkspaceRoot = (Resolve-Path "$PSScriptRoot\..\..").Path,
    [string]$Version = "2.4.7"
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
if (Test-Path (Join-Path $srcFolder "preview.png")) {
    Copy-Item (Join-Path $srcFolder "preview.png") (Join-Path $tempRel "preview.png")
}

$releaseZip = Join-Path $WorkspaceRoot "AutomaticIndustry-$Version.zip"
if (Test-Path $releaseZip) {
    Remove-Item $releaseZip -Force
}
Compress-Archive -Path "$tempRel\*" -DestinationPath $releaseZip -CompressionLevel Optimal
Write-Host "Created release zip: $releaseZip"

Write-Host "=== 4. Creating Source ZIP Archive ==="
Copy-Item -Path (Join-Path $srcFolder "src") -Destination (Join-Path $tempSrc "src") -Recurse
Copy-Item -Path (Join-Path $srcFolder "AutoMachineRebuilt.csproj") -Destination (Join-Path $tempSrc "AutoMachineRebuilt.csproj")
Copy-Item -Path (Join-Path $srcFolder "mod.yaml") -Destination (Join-Path $tempSrc "mod.yaml")
Copy-Item -Path (Join-Path $srcFolder "mod_info.yaml") -Destination (Join-Path $tempSrc "mod_info.yaml")
Copy-Item -Path (Join-Path $srcFolder "CHANGELOG.md") -Destination (Join-Path $tempSrc "CHANGELOG.md")
Copy-Item -Path (Join-Path $srcFolder "README.md") -Destination (Join-Path $tempSrc "README.md")
Copy-Item -Path (Join-Path $srcFolder "MOD_MENU_ARCHITECTURE.md") -Destination (Join-Path $tempSrc "MOD_MENU_ARCHITECTURE.md")
if (Test-Path (Join-Path $srcFolder "preview.png")) {
    Copy-Item -Path (Join-Path $srcFolder "preview.png") -Destination (Join-Path $tempSrc "preview.png")
}

$srcZip = Join-Path $WorkspaceRoot "AutomaticIndustry-src-$Version.zip"
if (Test-Path $srcZip) {
    Remove-Item $srcZip -Force
}
Compress-Archive -Path "$tempSrc\*" -DestinationPath $srcZip -CompressionLevel Optimal
Write-Host "Created src zip: $srcZip"

Remove-Item -Recurse -Force $tempDir

Write-Host "`n=== Packaging Complete for v$Version ==="
Get-ChildItem -Path $WorkspaceRoot -Filter "AutomaticIndustry-*$Version.zip" | Select-Object Name, Length, LastWriteTime

