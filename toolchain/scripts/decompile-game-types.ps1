# decompile-game-types.ps1
# Decompiles the specific game types referenced by AutomaticIndustry mod using ilspycmd.
# This script extracts C# source for every type the mod interacts with, organised by
# category, enabling offline reverse engineering without dnSpy GUI.
#
# Usage:   .\decompile-game-types.ps1
# Prereq:  dotnet tool install --global ilspycmd

param(
    [string]$ManagedDir = "$PSScriptRoot\..\..\Managed\Managed",
    [string]$OutputDir  = "$PSScriptRoot\..\decompiled"
)

$ErrorActionPreference = "Stop"
$ManagedDir = Resolve-Path $ManagedDir
$OutputDir  = [IO.Path]::GetFullPath($OutputDir)

$asmCS  = Join-Path $ManagedDir "Assembly-CSharp.dll"
$asmFP  = Join-Path $ManagedDir "Assembly-CSharp-firstpass.dll"
$asmUE  = Join-Path $ManagedDir "UnityEngine.CoreModule.dll"

if (!(Test-Path $asmCS))  { throw "Assembly-CSharp.dll not found at $ManagedDir" }
if (!(Test-Path $asmFP))  { throw "Assembly-CSharp-firstpass.dll not found at $ManagedDir" }

# ---------- types referenced by the mod, grouped by category ----------

# Core building / component types
$coreTypes = @(
    "KMonoBehaviour"
    "Workable"
    "WorkerBase"
    "Operational"
    "ComplexFabricator"
    "ComplexFabricatorWorkable"
    "ElementConverter"
    "ManualDeliveryKG"
    "Storage"
    "BuildingDef"
    "GeneratedBuildings"
    "Assets"
    "IBuildingConfig"
    "BuildingConfigManager"
    "Constructable"
    "Deconstructable"
    "PrimaryElement"
    "Pickupable"
)

# State machine infrastructure
$smTypes = @(
    "StateMachine"
    "StateMachineController"
    "StateMachineComponent"
    "GameStateMachine"
)

# Specific building state machines the mod drives via reflection
$buildingSMTypes = @(
    "OilRefinery"
    "GeoTuner"
    "RanchStation"
    "RanchedStates"
    "RanchableMonitor"
    "RancherChore"
    "IShearable"
    "IMilkable"
    "ResearchCenter"
    "ManualGenerator"
    "Valve"
    "SpiceGrinder"
    "SpiceGrinderWorkable"
    "MissionControlWorkable"
    "MissionControlClusterWorkable"
    "Studyable"
    "IceCooledFan"
    "Compost"
    "FoodSmoker"
    "FoodDehydrator"
    "MilkFatSeparator"
    "Desalinator"
    "OilWellCap"
    "Tinkerable"
    "Telescope"
    "ClusterTelescope"
    "ClusterTelescopeEnclosed"
    "ManualHighEnergyParticleSpawner"
    "ResetSkillsStation"
    "IceKettle"
    "Campfire"
    "GeneticAnalysisStation"
    "MorbRoverMaker"
    "UnderwaterShearingStaion"
)

# Config classes (for prefab ID resolution)
$configTypes = @(
    "OilRefineryConfig"
    "GeoTunerConfig"
    "RanchStationConfig"
    "ShearingStationConfig"
    "MilkingStationConfig"
    "UnderwaterRanchStationConfig"
    "UnderwaterShearingStationConfig"
    "UnderwaterMilkingStationConfig"
    "SpiceGrinderConfig"
    "MissionControlConfig"
    "MissionControlClusterConfig"
    "FarmStationConfig"
    "PowerControlStationConfig"
    "ManualGeneratorConfig"
    "TelescopeConfig"
    "ClusterTelescopeConfig"
    "ClusterTelescopeEnclosedConfig"
    "ManualHighEnergyParticleSpawnerConfig"
    "ResearchCenterConfig"
    "AdvancedResearchCenterConfig"
    "CosmicResearchCenterConfig"
    "DLC1CosmicResearchCenterConfig"
    "NuclearResearchCenterConfig"
    "OrbitalResearchCenterConfig"
    "DesalinatorConfig"
    "MilkFatSeparatorConfig"
    "OilWellCapConfig"
    "IceKettleConfig"
    "FoodDehydratorConfig"
    "CampfireConfig"
    "IceCooledFanConfig"
    "CompostConfig"
    "FabricatedWoodMakerConfig"
    "SushiBarConfig"
)

# Chore system types
$choreTypes = @(
    "Chore"
    "ChoreProvider"
    "ChoreType"
    "FetchChore"
    "FetchAreaChore"
    "WorkChore`1"
)

# Animation & UI types
$animTypes = @(
    "KAnimControllerBase"
    "KBatchedAnimController"
    "KAnim"
)

# Game system types
$gameTypes = @(
    "Game"
    "Db"
    "RoomTracker"
    "Grid"
    "SimHashes"
    "GameComps"
    "Geyser"
    "LocString"
)

# Klei.AI types
$kleiAITypes = @(
    "Effects"
    "AmountInstance"
)

function Decompile-Type {
    param(
        [string]$AssemblyPath,
        [string]$TypeName,
        [string]$Category
    )

    $outDir  = Join-Path $OutputDir $Category
    if (!(Test-Path $outDir)) { New-Item -ItemType Directory -Path $outDir -Force | Out-Null }
    $outFile = Join-Path $outDir "$TypeName.cs"

    if (Test-Path $outFile) {
        Write-Host "  [SKIP]  $TypeName (already decompiled)" -ForegroundColor DarkGray
        return
    }

    Write-Host "  [DECOMPILE] $TypeName ..." -NoNewline
    $refPaths = @(
        "--referencepath:$ManagedDir"
    )

    try {
        $result = & ilspycmd $AssemblyPath -t $TypeName $refPaths 2>&1
        if ($LASTEXITCODE -eq 0 -and $result) {
            $result | Out-File -FilePath $outFile -Encoding utf8
            $lines = ($result | Measure-Object -Line).Lines
            Write-Host " OK ($lines lines)" -ForegroundColor Green
        } else {
            Write-Host " NOT FOUND" -ForegroundColor Yellow
        }
    } catch {
        Write-Host " ERROR: $_" -ForegroundColor Red
    }
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " ONI Game Assembly Decompiler for AutomaticIndustry" -ForegroundColor Cyan
Write-Host " Assembly-CSharp:          $asmCS" -ForegroundColor Cyan
Write-Host " Assembly-CSharp-firstpass: $asmFP" -ForegroundColor Cyan
Write-Host " Output:                   $OutputDir" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "`n--- Core Types ---" -ForegroundColor Yellow
foreach ($t in $coreTypes) { Decompile-Type -AssemblyPath $asmCS -TypeName $t -Category "core" }

Write-Host "`n--- State Machine Infrastructure ---" -ForegroundColor Yellow
foreach ($t in $smTypes) { Decompile-Type -AssemblyPath $asmCS -TypeName $t -Category "statemachine" }

Write-Host "`n--- Building State Machines ---" -ForegroundColor Yellow
foreach ($t in $buildingSMTypes) { Decompile-Type -AssemblyPath $asmCS -TypeName $t -Category "buildings" }

Write-Host "`n--- Building Config Classes ---" -ForegroundColor Yellow
foreach ($t in $configTypes) { Decompile-Type -AssemblyPath $asmCS -TypeName $t -Category "configs" }

Write-Host "`n--- Chore System ---" -ForegroundColor Yellow
foreach ($t in $choreTypes) { Decompile-Type -AssemblyPath $asmCS -TypeName $t -Category "chores" }

Write-Host "`n--- Animation Types ---" -ForegroundColor Yellow
foreach ($t in $animTypes) { Decompile-Type -AssemblyPath $asmCS -TypeName $t -Category "animation" }

Write-Host "`n--- Game Systems ---" -ForegroundColor Yellow
foreach ($t in $gameTypes) { Decompile-Type -AssemblyPath $asmCS -TypeName $t -Category "game" }

Write-Host "`n--- Klei.AI Types ---" -ForegroundColor Yellow
foreach ($t in $kleiAITypes) { Decompile-Type -AssemblyPath $asmCS -TypeName $t -Category "klei_ai" }

# Also extract KMonoBehaviour from firstpass if not found in main assembly
Write-Host "`n--- Assembly-CSharp-firstpass Types ---" -ForegroundColor Yellow
$firstpassTypes = @("KMonoBehaviour", "KAnimControllerBase", "KBatchedAnimController", "KAnim", "StateMachine", "StateMachineController", "StateMachineComponent", "GameStateMachine")
foreach ($t in $firstpassTypes) { Decompile-Type -AssemblyPath $asmFP -TypeName $t -Category "firstpass" }

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host " Decompilation complete!" -ForegroundColor Cyan
$totalFiles = (Get-ChildItem -Path $OutputDir -Recurse -Filter "*.cs" | Measure-Object).Count
Write-Host " Total decompiled files: $totalFiles" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
