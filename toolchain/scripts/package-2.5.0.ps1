param(
    [string]$WorkspaceRoot = (Resolve-Path "$PSScriptRoot\..\..").Path,
    [string]$Version = "2.5.0",
    [string[]]$SecondaryRoots = @()
)

$ErrorActionPreference = "Stop"

# Default secondary roots resolved dynamically via USERPROFILE without hardcoding personal usernames
if (-not $SecondaryRoots -or $SecondaryRoots.Length -eq 0) {
    $userProf = $env:USERPROFILE
    $detected = @()
    if ($userProf) {
        $candidate1 = Join-Path $userProf "downloadtemp\Downloads\AutomaticIndustry"
        $candidate2 = Join-Path $userProf "Downloads\AutomaticIndustry"
        if (Test-Path $candidate1) { $detected += $candidate1 }
        if (Test-Path $candidate2) { $detected += $candidate2 }
    }
    $SecondaryRoots = $detected
}

$srcFolder = Join-Path $WorkspaceRoot "AutomaticIndustry-src-$Version"
if (-not (Test-Path $srcFolder)) {
    $srcFolder = Join-Path $WorkspaceRoot "AutomaticIndustry-src-2.4.41"
}
if (-not (Test-Path $srcFolder)) {
    $srcFolder = Join-Path $WorkspaceRoot "AutomaticIndustry-src-2.4.0\AutomaticIndustry-src-2.4.0"
}
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

# Archive historical ModMenu zips (keep only latest 1.4.13)
Get-ChildItem -Path $WorkspaceRoot -Filter "ModMenu-*.zip" -File | ForEach-Object {
    if ($_.Name -ne "ModMenu-1.4.13.zip" -and $_.Name -ne "ModMenu-src-1.4.13.zip") {
        $dest = Join-Path $archiveDir $_.Name
        Move-Item -Path $_.FullName -Destination $dest -Force
        Write-Host "  Archived historical ModMenu zip: $($_.Name) -> zip_src_archived/"
    }
}

# Clean up any leftover uncompressed release/src folders from older releases
Get-ChildItem -Path $WorkspaceRoot -Directory | Where-Object {
    ($_.Name -match '^AutomaticIndustry-\d+\.\d+\.\d+$' -or $_.Name -match '^AutomaticIndustry-src-\d+\.\d+\.\d+$' -or $_.Name -match '^ModMenu-\d+\.\d+\.\d+$' -or $_.Name -match '^ModMenu-src-\d+\.\d+\.\d+$') -and
    $_.Name -ne "AutomaticIndustry-src-2.4.0" -and
    $_.Name -ne "AutomaticIndustry-$Version" -and
    $_.Name -ne "AutomaticIndustry-src-$Version" -and
    $_.Name -ne "ModMenu-1.4.13" -and
    $_.Name -ne "ModMenu-src-1.4.13" -and
    $_.Name -ne "ModMenu-src-1.0.0"
} | ForEach-Object {
    Write-Host "  Removing older uncompressed folder: $($_.Name)"
    Remove-Item -Path $_.FullName -Recurse -Force
}

Write-Host "=== 2. Building Release Assembly with ILRepack ($Version) ==="
& dotnet build $csproj -c Release --no-incremental -p:ONIManaged=$managedDir
if ($LASTEXITCODE -ne 0) {
    throw "Build failed!"
}

$builtDll = Join-Path $srcFolder "bin\Release\netstandard2.1\AutomaticIndustry.dll"
if (-not (Test-Path $builtDll)) {
    $builtDll = Join-Path $srcFolder "bin\Release\net48\AutomaticIndustry.dll"
}
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
if (Test-Path (Join-Path $srcFolder "assets")) {
    Copy-Item -Path (Join-Path $srcFolder "assets") -Destination (Join-Path $tempRel "assets") -Recurse -Force
    Write-Host "  Bundled assets/ into release package"
}

$releaseZip = Join-Path $WorkspaceRoot "AutomaticIndustry-$Version.zip"
if (Test-Path $releaseZip) {
    Remove-Item $releaseZip -Force
}
Compress-Archive -Path "$tempRel\*" -DestinationPath $releaseZip -CompressionLevel Optimal
Write-Host "Created release zip: $releaseZip"

Write-Host "=== 4. Creating Source ZIP Archive and Staging Source ==="
Copy-Item -Path (Join-Path $srcFolder "src") -Destination (Join-Path $tempSrc "src") -Recurse
Copy-Item -Path (Join-Path $srcFolder "AutoMachineRebuilt.csproj") -Destination (Join-Path $tempSrc "AutoMachineRebuilt.csproj")
Copy-Item -Path (Join-Path $srcFolder "mod.yaml") -Destination (Join-Path $tempSrc "mod.yaml")
Copy-Item -Path (Join-Path $srcFolder "mod_info.yaml") -Destination (Join-Path $tempSrc "mod_info.yaml")
Copy-Item -Path (Join-Path $srcFolder "CHANGELOG.md") -Destination (Join-Path $tempSrc "CHANGELOG.md")
Copy-Item -Path (Join-Path $srcFolder "README.md") -Destination (Join-Path $tempSrc "README.md")
Copy-Item -Path (Join-Path $srcFolder "ILRepack.targets") -Destination (Join-Path $tempSrc "ILRepack.targets")
if (Test-Path (Join-Path $srcFolder "preview.png")) {
    Copy-Item (Join-Path $srcFolder "preview.png") -Destination (Join-Path $tempSrc "preview.png")
}
if (Test-Path (Join-Path $srcFolder "assets")) {
    Copy-Item -Path (Join-Path $srcFolder "assets") -Destination (Join-Path $tempSrc "assets") -Recurse -Force
    Write-Host "  Bundled assets/ into source package"
}

$srcZip = Join-Path $WorkspaceRoot "AutomaticIndustry-src-$Version.zip"
if (Test-Path $srcZip) {
    Remove-Item $srcZip -Force
}
Compress-Archive -Path "$tempSrc\*" -DestinationPath $srcZip -CompressionLevel Optimal
Write-Host "Created src zip: $srcZip"

Write-Host "=== 5. Deploying Mod and Src Folders to Workspace Root ==="
$workspaceModDir = Join-Path $WorkspaceRoot "AutomaticIndustry-$Version"
if (Test-Path $workspaceModDir) {
    Remove-Item -Recurse -Force $workspaceModDir
}
Copy-Item -Path $tempRel -Destination $workspaceModDir -Recurse -Force
Write-Host "Deployed mod release folder: $workspaceModDir"

$workspaceSrcDir = Join-Path $WorkspaceRoot "AutomaticIndustry-src-$Version"
$srcResolved = if (Test-Path $srcFolder) { (Resolve-Path $srcFolder).Path } else { "" }
$destResolved = if (Test-Path $workspaceSrcDir) { (Resolve-Path $workspaceSrcDir).Path } else { "" }
if ($srcResolved -ne $destResolved) {
    if (Test-Path $workspaceSrcDir) {
        Remove-Item -Recurse -Force $workspaceSrcDir
    }
    Copy-Item -Path $tempSrc -Destination $workspaceSrcDir -Recurse -Force
    Write-Host "Deployed mod src folder: $workspaceSrcDir"
} else {
    Write-Host "Mod src folder is current source directory: $workspaceSrcDir"
}

Remove-Item -Recurse -Force $tempDir

if ($SecondaryRoots) {
    Write-Host "`n=== 6. Synchronizing to Secondary Workspaces ==="
    foreach ($sec in $SecondaryRoots) {
        if ((Test-Path $sec) -and ((Resolve-Path $sec).Path -ne (Resolve-Path $WorkspaceRoot).Path)) {
            Write-Host "  -> Syncing to: $sec"
            $secArchive = Join-Path $sec "zip_src_archived"
            if (-not (Test-Path $secArchive)) { New-Item -ItemType Directory -Path $secArchive | Out-Null }

            # Archive old zips in secondary workspace
            Get-ChildItem -Path $sec -Filter "AutomaticIndustry-*.zip" -File | ForEach-Object {
                if ($_.Name -ne "AutomaticIndustry-$Version.zip" -and $_.Name -ne "AutomaticIndustry-src-$Version.zip") {
                    $dest = Join-Path $secArchive $_.Name
                    Move-Item -Path $_.FullName -Destination $dest -Force
                }
            }

            # Copy release and src zips
            Copy-Item -Path $releaseZip -Destination (Join-Path $sec "AutomaticIndustry-$Version.zip") -Force
            Copy-Item -Path $srcZip -Destination (Join-Path $sec "AutomaticIndustry-src-$Version.zip") -Force

            # Copy mod release folder
            $secModDir = Join-Path $sec "AutomaticIndustry-$Version"
            if (Test-Path $secModDir) { Remove-Item -Recurse -Force $secModDir }
            Copy-Item -Path $workspaceModDir -Destination $secModDir -Recurse -Force

            # Copy mod src folder
            $secSrcDir = Join-Path $sec "AutomaticIndustry-src-$Version"
            if (Test-Path $secSrcDir) { Remove-Item -Recurse -Force $secSrcDir }
            Copy-Item -Path $workspaceSrcDir -Destination $secSrcDir -Recurse -Force

            Write-Host "     Synchronized $Version artifacts to $sec"
        }
    }
}

Write-Host "`n=== Packaging Complete for v$Version ==="
Get-ChildItem -Path $WorkspaceRoot -Filter "*$Version*" | Select-Object Name, Length, LastWriteTime
