$WorkspaceRoot = (Resolve-Path "$PSScriptRoot\..").Path
$plib = "$WorkspaceRoot\ModMenu-src-1.0.0\ModMenu-src-1.0.0\lib\PLib.dll"
$managed = "$WorkspaceRoot\Managed\Managed"
$outDir = "$WorkspaceRoot\toolchain\decompiled\PLib"

if (-not (Test-Path $outDir)) { New-Item -ItemType Directory -Path $outDir -Force | Out-Null }

$result = & ilspycmd $plib --referencepath:$managed -t PeterHan.PLib.UI.PDialog
$result | Out-File -FilePath (Join-Path $outDir "PDialog.cs") -Encoding utf8
Write-Host "Decompiled PDialog.cs ($(($result | Measure-Object -Line).Lines) lines)"

