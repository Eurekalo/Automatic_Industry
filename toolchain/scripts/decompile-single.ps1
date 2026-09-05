# decompile-single.ps1
# Quick helper to decompile a single type on demand for debugging.
#
# Usage:
#   .\decompile-single.ps1 SpiceGrinder
#   .\decompile-single.ps1 SpiceGrinder -Assembly firstpass
#   .\decompile-single.ps1 ComplexFabricator -List   # list all types matching pattern

param(
    [Parameter(Mandatory=$true, Position=0)]
    [string]$TypeName,

    [ValidateSet("main", "firstpass")]
    [string]$Assembly = "main",

    [switch]$List,
    [switch]$Force
)

$ErrorActionPreference = "Stop"
$ManagedDir = Resolve-Path "$PSScriptRoot\..\..\Managed\Managed"
$OutputDir  = "$PSScriptRoot\..\decompiled\adhoc"

$dll = if ($Assembly -eq "firstpass") {
    Join-Path $ManagedDir "Assembly-CSharp-firstpass.dll"
} else {
    Join-Path $ManagedDir "Assembly-CSharp.dll"
}

if (!(Test-Path $dll)) { throw "DLL not found: $dll" }

if ($List) {
    Write-Host "Searching for types matching '*$TypeName*' in $($Assembly)..." -ForegroundColor Cyan
    $result = & ilspycmd $dll --list -t "$TypeName" 2>&1
    $result | ForEach-Object { Write-Host "  $_" }
    exit 0
}

if (!(Test-Path $OutputDir)) { New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null }

$outFile = Join-Path $OutputDir "$TypeName.cs"
if ((Test-Path $outFile) -and !$Force) {
    Write-Host "Already decompiled: $outFile (use -Force to overwrite)" -ForegroundColor Yellow
    Get-Content $outFile | Select-Object -First 5
    Write-Host "... ($(Get-Content $outFile | Measure-Object -Line | Select-Object -ExpandProperty Lines) total lines)"
    exit 0
}

Write-Host "Decompiling $TypeName from $Assembly..." -ForegroundColor Cyan
$result = & ilspycmd $dll -t $TypeName --referencepath:$ManagedDir 2>&1

if ($LASTEXITCODE -eq 0 -and $result) {
    $result | Out-File -FilePath $outFile -Encoding utf8
    $lines = ($result | Measure-Object -Line).Lines
    Write-Host "Saved to $outFile ($lines lines)" -ForegroundColor Green
} else {
    Write-Host "Type '$TypeName' not found in $Assembly assembly" -ForegroundColor Red
    Write-Host "Try: .\decompile-single.ps1 $TypeName -List" -ForegroundColor Yellow
}
