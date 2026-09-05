$WorkspaceRoot = (Resolve-Path "$PSScriptRoot\..").Path
$dll = "$WorkspaceRoot\ForAutomaticIndustryDev\3566906492\AutoSweeperHarvest.dll"
$managed = "$WorkspaceRoot\Managed\Managed"
$outDir = "$WorkspaceRoot\toolchain\decompiled\AutoSweeperHarvest"

if (-not (Test-Path $outDir)) { New-Item -ItemType Directory -Path $outDir -Force | Out-Null }

$result = & ilspycmd $dll --referencepath:$managed
$result | Out-File -FilePath (Join-Path $outDir "AutoSweeperHarvest_decompiled.cs") -Encoding utf8
Write-Host "Decompiled AutoSweeperHarvest ($(($result | Measure-Object -Line).Lines) lines)"

