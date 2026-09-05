param(
    [string]$PrimaryRoot = (Resolve-Path "$PSScriptRoot\..\..").Path,
    [string]$SecondaryRoot = "",
    [string]$Version = "2.4.30"
)

$ErrorActionPreference = "Stop"

Write-Host "============================================================"
Write-Host " Automated Packaging & Archiving Tool (Primary: $PrimaryRoot)"
Write-Host " Target Version: $Version"
Write-Host "============================================================"

# Ensure source is synced if SecondaryRoot is provided
if ($SecondaryRoot -and (Test-Path $SecondaryRoot)) {
    Write-Host "`n>>> Step 1: Synchronizing active source code Secondary -> Primary ..."
    $cSrc = Join-Path $SecondaryRoot "AutomaticIndustry-src-2.4.0"
    $dSrc = Join-Path $PrimaryRoot "AutomaticIndustry-src-2.4.0"
    if (Test-Path $cSrc) {
        Copy-Item -Path "$cSrc\*" -Destination $dSrc -Recurse -Force
        Write-Host "  Synchronized source tree to $dSrc"
    }
}

# Define roots to clean and archive
$roots = @($PrimaryRoot)
if ($SecondaryRoot -and (Test-Path $SecondaryRoot)) {
    $roots += $SecondaryRoot
}

foreach ($root in $roots) {
    if (-not (Test-Path $root)) { continue }
    Write-Host "`n>>> Step 2: Archiving old packages & src zips in $root ..."
    $archiveDir = Join-Path $root "zip_src_archived"
    if (-not (Test-Path $archiveDir)) {
        New-Item -ItemType Directory -Path $archiveDir -Force | Out-Null
    }

    # Archive historical release zips
    Get-ChildItem -Path $root -Filter "AutomaticIndustry-*.zip" -File | ForEach-Object {
        if ($_.Name -ne "AutomaticIndustry-$Version.zip" -and $_.Name -ne "AutomaticIndustry-src-$Version.zip") {
            $dest = Join-Path $archiveDir $_.Name
            Move-Item -Path $_.FullName -Destination $dest -Force
            Write-Host "  Archived: $($_.Name) -> zip_src_archived/"
        }
    }

    # Archive all historical src zips into zip_src_archived
    Get-ChildItem -Path $root -Filter "*-src-*.zip" -File | ForEach-Object {
        $dest = Join-Path $archiveDir $_.Name
        Move-Item -Path $_.FullName -Destination $dest -Force
        Write-Host "  Archived src zip: $($_.Name) -> zip_src_archived/"
    }

    # Archive older ModMenu packages (keep only 1.4.8)
    Get-ChildItem -Path $root -Filter "ModMenu-*.zip" -File | ForEach-Object {
        if ($_.Name -ne "ModMenu-1.4.8.zip") {
            $dest = Join-Path $archiveDir $_.Name
            Move-Item -Path $_.FullName -Destination $dest -Force
            Write-Host "  Archived ModMenu zip: $($_.Name) -> zip_src_archived/"
        }
    }

    # Clean up obsolete uncompressed folders
    $obsoleteFolders = @("ModMenu-1.4.7", "AutomaticIndustry-2.4.28", "AutomaticIndustry-2.4.29")
    foreach ($folderName in $obsoleteFolders) {
        $targetFolder = Join-Path $root $folderName
        if (Test-Path $targetFolder) {
            Remove-Item -Path $targetFolder -Recurse -Force
            Write-Host "  Removed obsolete folder: $folderName"
        }
    }
}

Write-Host "`n>>> Step 3: Compiling Assembly with ILRepack ($Version) on D: ..."
$srcFolder = Join-Path $PrimaryRoot "AutomaticIndustry-src-2.4.0\AutomaticIndustry-src-2.4.0"
$csproj = Join-Path $srcFolder "AutoMachineRebuilt.csproj"
$managedDir = Join-Path $PrimaryRoot "Managed\Managed"

& dotnet build $csproj -c Release --no-incremental -p:ONIManaged=$managedDir
if ($LASTEXITCODE -ne 0) {
    throw "Build failed on D:!"
}

$builtDll = Join-Path $srcFolder "bin\Release\netstandard2.1\AutomaticIndustry.dll"
if (-not (Test-Path $builtDll)) {
    throw "Output DLL not found: $builtDll"
}
$dllSize = (Get-Item $builtDll).Length
Write-Host "  Compiled DLL successfully: $builtDll ($dllSize bytes)"

Write-Host "`n>>> Step 4: Staging release folder and archives on $PrimaryRoot ..."
$tempDir = Join-Path $PrimaryRoot "toolchain\temp_staging"
if (Test-Path $tempDir) { Remove-Item -Recurse -Force $tempDir }
$tempRel = Join-Path $tempDir "release"
$tempSrc = Join-Path $tempDir "src"
New-Item -ItemType Directory -Path $tempRel -Force | Out-Null
New-Item -ItemType Directory -Path $tempSrc -Force | Out-Null

Copy-Item $builtDll (Join-Path $tempRel "AutomaticIndustry.dll")
Copy-Item (Join-Path $srcFolder "mod.yaml") (Join-Path $tempRel "mod.yaml")
Copy-Item (Join-Path $srcFolder "mod_info.yaml") (Join-Path $tempRel "mod_info.yaml")

# Release ZIP
$releaseZip = Join-Path $PrimaryRoot "AutomaticIndustry-$Version.zip"
if (Test-Path $releaseZip) { Remove-Item $releaseZip -Force }
Compress-Archive -Path "$tempRel\*" -DestinationPath $releaseZip -CompressionLevel Optimal
Write-Host "  Created release zip: $releaseZip"

# Source ZIP
Copy-Item -Path (Join-Path $srcFolder "src") -Destination (Join-Path $tempSrc "src") -Recurse
Copy-Item -Path (Join-Path $srcFolder "AutoMachineRebuilt.csproj") -Destination (Join-Path $tempSrc "AutoMachineRebuilt.csproj")
Copy-Item -Path (Join-Path $srcFolder "mod.yaml") -Destination (Join-Path $tempSrc "mod.yaml")
Copy-Item -Path (Join-Path $srcFolder "mod_info.yaml") -Destination (Join-Path $tempSrc "mod_info.yaml")
Copy-Item -Path (Join-Path $srcFolder "CHANGELOG.md") -Destination (Join-Path $tempSrc "CHANGELOG.md")
Copy-Item -Path (Join-Path $srcFolder "README.md") -Destination (Join-Path $tempSrc "README.md")
Copy-Item -Path (Join-Path $srcFolder "ILRepack.targets") -Destination (Join-Path $tempSrc "ILRepack.targets")

$srcZip = Join-Path $PrimaryRoot "AutomaticIndustry-src-$Version.zip"
if (Test-Path $srcZip) { Remove-Item $srcZip -Force }
Compress-Archive -Path "$tempSrc\*" -DestinationPath $srcZip -CompressionLevel Optimal
Write-Host "  Created source zip: $srcZip"

# Move source zip to zip_src_archived as per clean archiving standard
$archiveDst = Join-Path $PrimaryRoot "zip_src_archived\AutomaticIndustry-src-$Version.zip"
Copy-Item -Path $srcZip -Destination $archiveDst -Force
Write-Host "  Archived source zip: $archiveDst"

# Deploy release mod folder
$workspaceModDir = Join-Path $PrimaryRoot "AutomaticIndustry-$Version"
if (Test-Path $workspaceModDir) { Remove-Item -Recurse -Force $workspaceModDir }
Copy-Item -Path $tempRel -Destination $workspaceModDir -Recurse -Force
Write-Host "  Deployed mod folder: $workspaceModDir"

Remove-Item -Recurse -Force $tempDir

Write-Host "`n>>> Step 5: Mirroring clean state from D: (Primary) to C: (Secondary) ..."
$excludeList = @("scratch", ".git")
Get-ChildItem -Path $PrimaryRoot | Where-Object { $excludeList -notcontains $_.Name } | ForEach-Object {
    $targetPath = Join-Path $SecondaryRoot $_.Name
    Copy-Item -Path $_.FullName -Destination $targetPath -Recurse -Force
}

# Also remove obsolete items from C:
$cObsolete = @(
    (Join-Path $SecondaryRoot "ModMenu-1.4.7"),
    (Join-Path $SecondaryRoot "AutomaticIndustry-2.4.28.zip"),
    (Join-Path $SecondaryRoot "AutomaticIndustry-2.4.29.zip"),
    (Join-Path $SecondaryRoot "AutomaticIndustry-src-2.4.28.zip"),
    (Join-Path $SecondaryRoot "AutomaticIndustry-src-2.4.29.zip"),
    (Join-Path $SecondaryRoot "ModMenu-1.4.7.zip"),
    (Join-Path $SecondaryRoot "ModMenu-src-1.4.7.zip")
)
foreach ($item in $cObsolete) {
    if (Test-Path $item) {
        Remove-Item -Path $item -Recurse -Force
        Write-Host "  Cleaned obsolete item from C: $item"
    }
}

Write-Host "`n============================================================"
Write-Host " Summary of Primary Directory ($PrimaryRoot):"
Write-Host "============================================================"
Get-ChildItem -Path $PrimaryRoot | Select-Object Mode, Name, Length, LastWriteTime
