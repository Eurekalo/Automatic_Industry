$WorkspaceRoot = (Resolve-Path "$PSScriptRoot\..").Path
$managed = "$WorkspaceRoot\Managed\Managed\Assembly-CSharp.dll"
$outDir = "$WorkspaceRoot\toolchain\decompiled\VanillaHarvest"

$result = & ilspycmd $managed -t SolidTransferArm
$result | Out-File -FilePath (Join-Path $outDir "SolidTransferArm.cs") -Encoding utf8
Write-Host "Decompiled SolidTransferArm ($(($result | Measure-Object -Line).Lines) lines)"

