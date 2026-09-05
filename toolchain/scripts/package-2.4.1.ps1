param(
    [string]$WorkspaceRoot = (Resolve-Path "$PSScriptRoot\..\..").Path
)

$ErrorActionPreference = "Stop"

$srcFolder = Join-Path $WorkspaceRoot "AutomaticIndustry-src-2.4.0\AutomaticIndustry-src-2.4.0"
$csproj = Join-Path $srcFolder "AutoMachineRebuilt.csproj"
$managedDir = Join-Path $WorkspaceRoot "Managed\Managed"

Write-Host "=== 1. Building Release Assembly with ILRepack ==="
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

Write-Host "=== 2. Creating Release Folder (AutomaticIndustry-2.4.1) ==="
$relFolder = Join-Path $WorkspaceRoot "AutomaticIndustry-2.4.1"
if (Test-Path $relFolder) {
    Remove-Item -Recurse -Force $relFolder
}
New-Item -ItemType Directory -Path $relFolder | Out-Null

Copy-Item $builtDll (Join-Path $relFolder "AutomaticIndustry.dll")
Copy-Item (Join-Path $srcFolder "mod.yaml") (Join-Path $relFolder "mod.yaml")
Copy-Item (Join-Path $srcFolder "mod_info.yaml") (Join-Path $relFolder "mod_info.yaml")
Copy-Item (Join-Path $srcFolder "LICENSE") (Join-Path $relFolder "LICENSE")

Write-Host "=== 3. Creating Source Folder (AutomaticIndustry-src-2.4.1) ==="
$srcPkgFolder = Join-Path $WorkspaceRoot "AutomaticIndustry-src-2.4.1"
if (Test-Path $srcPkgFolder) {
    Remove-Item -Recurse -Force $srcPkgFolder
}
New-Item -ItemType Directory -Path $srcPkgFolder | Out-Null

Get-ChildItem -Path $srcFolder -Exclude "bin", "obj", ".vs" | ForEach-Object {
    Copy-Item -Path $_.FullName -Destination $srcPkgFolder -Recurse -Force
}

Write-Host "=== 4. Creating ZIP Archives ==="
$zipRel = Join-Path $WorkspaceRoot "AutomaticIndustry-2.4.1.zip"
$zipSrc = Join-Path $WorkspaceRoot "AutomaticIndustry-src-2.4.1.zip"

if (Test-Path $zipRel) { Remove-Item -Force $zipRel }
if (Test-Path $zipSrc) { Remove-Item -Force $zipSrc }

Compress-Archive -Path (Join-Path $relFolder "*") -DestinationPath $zipRel
Compress-Archive -Path (Join-Path $srcPkgFolder "*") -DestinationPath $zipSrc

Write-Host "=== Packaging Completed Successfully ==="
Write-Host "Release folder: $relFolder"
Write-Host "Source folder : $srcPkgFolder"
Write-Host "Release zip   : $zipRel ($((Get-Item $zipRel).Length) bytes)"
Write-Host "Source zip    : $zipSrc ($((Get-Item $zipSrc).Length) bytes)"

