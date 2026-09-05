$WorkspaceRoot = (Resolve-Path "$PSScriptRoot\..").Path
$dll = "$WorkspaceRoot\ForCompatibility\2995311574\AutoCompost.dll"
$managed = "$WorkspaceRoot\Managed\Managed"
$outDir = "$WorkspaceRoot\toolchain\decompiled\AutoCompost"

if (-not (Test-Path $outDir)) { New-Item -ItemType Directory -Path $outDir -Force | Out-Null }

$result = & ilspycmd $dll --referencepath:$managed
$result | Out-File -FilePath (Join-Path $outDir "AutoCompost_all.cs") -Encoding utf8
Write-Host "Decompiled AutoCompost.dll ($(($result | Measure-Object -Line).Lines) lines)"

