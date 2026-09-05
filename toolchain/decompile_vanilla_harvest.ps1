$WorkspaceRoot = (Resolve-Path "$PSScriptRoot\..").Path
$managed = "$WorkspaceRoot\Managed\Managed\Assembly-CSharp.dll"
$outDir = "$WorkspaceRoot\toolchain\decompiled\VanillaHarvest"

if (-not (Test-Path $outDir)) { New-Item -ItemType Directory -Path $outDir -Force | Out-Null }

$result1 = & ilspycmd $managed -t HarvestDesignatable
$result1 | Out-File -FilePath (Join-Path $outDir "HarvestDesignatable.cs") -Encoding utf8

$result2 = & ilspycmd $managed -t Harvestable
$result2 | Out-File -FilePath (Join-Path $outDir "Harvestable.cs") -Encoding utf8

Write-Host "Decompiled HarvestDesignatable ($(($result1 | Measure-Object -Line).Lines) lines) and Harvestable ($(($result2 | Measure-Object -Line).Lines) lines)"

