param(
    [string]$WorkspaceRoot = (Resolve-Path "$PSScriptRoot\..\..").Path,
    [string]$Version = "1.4.4"
)

$ErrorActionPreference = "Stop"

$srcFolder = Join-Path $WorkspaceRoot "ModMenu-src-1.0.0\ModMenu-src-1.0.0"
$csproj = Join-Path $srcFolder "ModMenu.csproj"
$managedDir = Join-Path $WorkspaceRoot "Managed\Managed"
$archiveDir = Join-Path $WorkspaceRoot "zip_src_archived"

if (-not (Test-Path $archiveDir)) {
    New-Item -ItemType Directory -Path $archiveDir | Out-Null
}

Write-Host "=== 1. Archiving Previous ModMenu Release and Source Zips ==="
Get-ChildItem -Path $WorkspaceRoot -Filter "ModMenu-*.zip" -File | ForEach-Object {
    if ($_.Name -ne "ModMenu-$Version.zip" -and $_.Name -ne "ModMenu-src-$Version.zip") {
        $dest = Join-Path $archiveDir $_.Name
        Move-Item -Path $_.FullName -Destination $dest -Force
        Write-Host "  Archived: $($_.Name) -> zip_src_archived/"
    }
}

# Clean up any leftover uncompressed ModMenu folders
Get-ChildItem -Path $WorkspaceRoot -Directory | Where-Object {
    ($_.Name -match '^ModMenu-\d+\.\d+\.\d+$' -or $_.Name -match '^ModMenu-src-\d+\.\d+\.\d+$') -and
    $_.Name -ne "ModMenu-src-1.0.0"
} | ForEach-Object {
    Write-Host "  Removing uncompressed folder: $($_.Name)"
    Remove-Item -Path $_.FullName -Recurse -Force
}

Write-Host "=== 2. Building Release Assembly with ILRepack ($Version) ==="
& dotnet build $csproj -c Release --no-incremental -p:ONIManaged=$managedDir
if ($LASTEXITCODE -ne 0) {
    throw "Build failed!"
}

$builtDll = Join-Path $srcFolder "bin\Release\net48\ModMenu.dll"
if (-not (Test-Path $builtDll)) {
    throw "Output DLL not found: $builtDll"
}

$dllSize = (Get-Item $builtDll).Length
Write-Host "Output DLL size: $dllSize bytes"

Write-Host "=== 3. Staging and Creating Release ZIP Archive ==="
$tempDir = Join-Path $WorkspaceRoot "toolchain\temp_staging_modmenu"
if (Test-Path $tempDir) {
    Remove-Item -Recurse -Force $tempDir
}
$tempRel = Join-Path $tempDir "release"
$tempSrc = Join-Path $tempDir "src"
New-Item -ItemType Directory -Path $tempRel -Force | Out-Null
New-Item -ItemType Directory -Path $tempSrc -Force | Out-Null

Copy-Item $builtDll (Join-Path $tempRel "ModMenu.dll")
Copy-Item (Join-Path $srcFolder "mod.yaml") (Join-Path $tempRel "mod.yaml")
Copy-Item (Join-Path $srcFolder "mod_info.yaml") (Join-Path $tempRel "mod_info.yaml")
if (Test-Path (Join-Path $srcFolder "preview.png")) {
    Copy-Item (Join-Path $srcFolder "preview.png") (Join-Path $tempRel "preview.png")
}

$releaseZip = Join-Path $WorkspaceRoot "ModMenu-$Version.zip"
if (Test-Path $releaseZip) {
    Remove-Item $releaseZip -Force
}
Compress-Archive -Path "$tempRel\*" -DestinationPath $releaseZip -CompressionLevel Optimal
Write-Host "Created release zip: $releaseZip"

Write-Host "=== 4. Creating Source ZIP Archive ==="
Copy-Item -Path (Join-Path $srcFolder "src") -Destination (Join-Path $tempSrc "src") -Recurse
Copy-Item -Path (Join-Path $srcFolder "ModMenu.csproj") -Destination (Join-Path $tempSrc "ModMenu.csproj")
Copy-Item -Path (Join-Path $srcFolder "mod.yaml") -Destination (Join-Path $tempSrc "mod.yaml")
Copy-Item -Path (Join-Path $srcFolder "mod_info.yaml") -Destination (Join-Path $tempSrc "mod_info.yaml")
Copy-Item -Path (Join-Path $srcFolder "CHANGELOG.md") -Destination (Join-Path $tempSrc "CHANGELOG.md")
Copy-Item -Path (Join-Path $srcFolder "README.md") -Destination (Join-Path $tempSrc "README.md")
if (Test-Path (Join-Path $srcFolder "preview.png")) {
    Copy-Item (Join-Path $srcFolder "preview.png") -Destination (Join-Path $tempSrc "preview.png")
}

$srcZip = Join-Path $WorkspaceRoot "ModMenu-src-$Version.zip"
if (Test-Path $srcZip) {
    Remove-Item $srcZip -Force
}
Compress-Archive -Path "$tempSrc\*" -DestinationPath $srcZip -CompressionLevel Optimal
Write-Host "Created src zip: $srcZip"

Remove-Item -Recurse -Force $tempDir

Write-Host "`n=== Packaging Complete for ModMenu v$Version ==="
Get-ChildItem -Path $WorkspaceRoot -Filter "ModMenu-*$Version.zip" | Select-Object Name, Length, LastWriteTime

