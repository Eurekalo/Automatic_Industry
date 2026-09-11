## 2.4.40

- **GeoTuner Delivery Fetch Abort Loop & Priority Flickering Fix (地质调谐仪运送材料任务中断循环与优先级闪烁修复)**:
  - **Material Delivery & Programming Errands Blockage Elimination**:
    - Resolved the critical issue where switching a Geotuner's target geyser left the building unable to receive tuning materials (50kg Bleach Stone, Fertilizer, Abyssalite, etc.), caused priority adjustments to flicker and vanish, and permanently stalled subsequent automated/manual research.
    - **Root Cause**: In vanilla ONI, `ManualDeliveryKG.RequestedItemTag`'s property setter unconditionally executes `AbortDelivery("Requested Item Tag Changed")` on every assignment without checking if `value == requestedItemTag`. Because `AutoGeoTuner.Step()` runs on an `ISim200ms` cadence (5 times per second), assigning `RequestedItemTag` repeatedly cancelled the active `FetchList2` chore 5 times every second, wiping out duplicant errand queues and resetting UI priority displays.
  - **Strict Inequality Guards & Non-Destructive Delivery Maintenance**:
    - Replaced unconditional property writes in `AutoGeoTuner` with `EnsureDeliveryConfigured()`, strictly guarding `RequestedItemTag`, `capacity`, `refillMass`, `MinimumMass`, and pause states with inequality checks.
    - Fetch chores now remain active and uninterrupted, allowing Auto-Sweepers and Duplicants to deliver the 50kg tuning material immediately upon geyser selection.
  - **Seamless Tuning Broadcast Loop**:
    - Hardened `AutoCompleteResearch()` to cleanly buffer the next 50kg in storage while a tuning broadcast is running, and automatically consume the material and re-trigger broadcast the instant the previous broadcast expires (`remaining <= 0s`), achieving 100% tuning uptime with zero downtime.

## 2.4.39

- **GeoTuner Premature Class Constructor Sound NRE Fix & Auto-Healing (地热调谐器静态构造函数提前求值致空引用黑洞崩溃修复)**:
  - **Black Hole Game Crash Elimination**:
    - Fixed a fatal game-crashing Black Hole error (`NullReferenceException` inside `StateMachine.ExecuteActions` -> `GeoTuner.TriggerSoundsForGeyserChange` -> `SoundEvent.PlayOneShot` -> `FMODUnity.RuntimeManager.PathToGUID`) triggered when assigning or tuning geysers/volcanoes.
    - **Root Cause**: During mod initialization (`OnLoad`), reflection inspection of `typeof(GeoTuner)` in station suppression patches prematurely triggered `GeoTuner`'s static constructor (`.cctor`) before `GlobalAssets` had loaded sound banks. Consequently, `liquidGeyserTuningSoundPath`, `gasGeyserTuningSoundPath`, and `metalGeyserTuningSoundPath` were permanently evaluated to `null`.
  - **Dynamic Audio Auto-Healing & Defensive Prefix**:
    - Implemented `GeoTunerSoundSafetyPatch`:
      - `EnsureSoundPathsPopulated()` checks if static sound paths are null or empty, automatically querying `GlobalAssets.GetSound(...)` once sound assets are ready.
      - Intercepts `GeoTuner.TriggerSoundsForGeyserChange` with a defensive Harmony prefix, guarding against null/empty event strings before passing to FMOD and returning `false` to skip vanilla's unguarded invocation.
      - Proactively triggers `EnsureSoundPathsPopulated()` during `BuildingPrefabInjection` and `AutoGeoTuner.Prepare()`.
  - **Project & Build Configurations**:
    - Added `FMODUnity.dll` reference to `AutoMachineRebuilt.csproj`.
  - **Sandbox Automated Verification**:
    - Added Test #39 to `SandboxTests.cs` simulating premature static constructor wipeout, path auto-healing, and defensive null guards (246/246 tests passing).

## 2.4.38

- **SymbolOverrideController SaveLoad & Initialization Auto-Healing Safety (存档加载与实体反序列化符号覆盖控制器断言崩溃自愈修复)**:
  - **Save Load & Deserialization Crash Prevention**:
    - Fixed fatal assertion crashes during save game loading when Ronivan's mods (Metallurgy, Chemical Processing, Nuclear) or custom buildings are present:
      `Assert failed: SymbolOverrideController requires usingNewSymbolOverrideSystem to be set to true.`
    - Deserialized save entities or prefabs instantiated via `SaveLoadRoot.Load` had `usingNewSymbolOverrideSystem` default to `false` because the field is non-serialized. When `GameObject.SetActive(true)` triggers `Awake` -> `SymbolOverrideController.OnPrefabInit`, the assertion crashed under LogCatcher / FT.
  - **Harmony Prefix Auto-Healing Layer (`SymbolOverrideControllerCompatibility`)**:
    - Harmony Prefix on `SymbolOverrideController.OnPrefabInit` auto-heals `usingNewSymbolOverrideSystem = true` and verifies `KBatchedAnimController` before original assertions execute.
    - Harmony Prefix on `SymbolOverrideControllerUtil.AddToPrefab` ensures `usingNewSymbolOverrideSystem = true` before `AddComponent` triggers `Awake()`.
  - **Sandbox Automated Verification**:
    - Added Test #38 simulating save/load deserialization and Ronivan building loading resilience (245/245 tests passing).

## 2.4.37

- **Chemical Processing In-Game Mod Config Safe UI Parenting (化工模组 BuildingEditor 窗口父级 NullReference 崩溃修复)**:
  - **In-Game Dialog Parenting Resilience**:
    - Fixed `NullReferenceException` when opening mod configuration dialogs (e.g., Chemical Processing BuildingEditor `ShowWindow`) while in-game or paused.
    - When `FrontEndManager.Instance` is null during active simulation, safely resolved parenting to `GameScreenManager.Instance.ssOverlayCanvas` or `GetTargetWidget()`, preventing UI initialization failures.
  - **Multilingual Community Text Pack Compatibility**:
    - Verified Japanese / CJK community text pack formatting under NotoSansCJKjp-Regular.

## 2.4.36

- **ModMenu Direct Lifecycle & Snapshot Decoupling (ModMenu 原生生命周期解耦与无冲突配置)**:
  - Decoupled mod enablement workflows from external JSON profile snapshots; mod activation and deactivation cleanly route through `KMod.Manager`.
  - Eliminated mod disablement loops, restart wipeout prompts, and unnecessary profile synchronization overhead.

## 2.4.35

- **Comprehensive Code Audit & Runtime Robustness Hardening (模组核心代码安全与稳健性审计修复)**:
  - **Case-Insensitive Prefab Lookup in AutoMachineOptions**:
    - Replaced case-sensitive dictionary instantiation in `ToggleByPrefabId` with `new Dictionary<string, Func<AutoMachineOptions, bool>>(StringComparer.OrdinalIgnoreCase)`.
    - Deduplicated canonical PascalCase keys (e.g. `AlgaeTerrarium`, `CO2Scrubber`, `Desalinator`, `Electrolyzer`, `OilRefinery`, `Polymerizer`, `WaterPurifier`, etc.) to prevent duplicate key `ArgumentException`, ensuring robust configuration lookups regardless of identifier casing.
  - **Deconstruction Footprint Cache Cleanup in AutoBuildingCustomizer**:
    - Enhanced `AutoBuildingCustomizer.OnCleanUp()` to unregister and purge cached building footprint cell mappings from `CustomizersByCell` upon deconstruction.
    - Guarded cleanup with `!App.IsExiting && !KMonoBehaviour.isLoadingScene` to avoid redundant state manipulation during scene teardown and eliminate memory leaks over long colony lifecycles.
  - **AutoManualGenerator Battery State Synchronization**:
    - Refactored `AutoManualGeneratorController` battery query logic to discover connected batteries via `ICircuitConnected` on `Generator` instead of directly searching local components.
    - Accurately respects power network battery capacities and user-configured charge thresholds (refill threshold vs. full threshold), eliminating chore thrashing and task oscillation.
  - **LiquidReservoir Sweeper vs. Duplicant Disambiguation**:
    - In `LiquidReservoirFetchPatches`, integrated `SolidTransferArm_FindFetchTarget_Patch.IsInsideArmFetch` thread-local state to cleanly distinguish Auto-Sweepers (`SolidTransferArm`) from Duplicant deliveries.
    - Sweepers can smoothly deliver bottled liquids to liquid reservoirs without interfering with Duplicant manual delivery errands.
  - **Option Guards for AutoEmptyTriggerPatches**:
    - Wrapped `AssetsIsTagSolidTransferArmConveyablePatch` with option checks so transfer arm conveyability patches only activate when auto empty / bottle sweep features are enabled.
  - **PLib OptionsDialog Layout Fix & Mod Load Safety**:
    - Fixed critical startup `HarmonyException` ("Parameter optionsDialog not found") by aligning postfix parameter name with `OptionsDialog.AddModInfoScreen(PDialog dialog)`.
    - Converted `OptionsDialogLayoutFixPatch` from auto-discovery to defensive manual registration inside `OnLoad()`, guaranteeing that upstream PLib changes will never prevent Automatic Industry from initializing.
  - **No Manual Delivery & SolidTransferArm Anim Override Assert Fix**:
    - Resolved `Assert failed: Anim overrides containing additional symbols require a symbol override controller` during robotic arm pickup errands (`FetchAreaChore` / `DoPickup`).
    - Added `StandardWorkerAttachOverrideAnimsPatch` to suppress duplicant animation overrides on workers lacking a `SymbolOverrideController` or with `UsesMultiTool == false`.
    - Proactively attached `SymbolOverrideController` to `SolidTransferArm` completed prefabs in `BuildingPrefabInjection`, ensuring dual-layer resilience across third-party fetch mods.

## 2.4.34

- **Customize Buildings Mod Compatibility (与 Customize Buildings 模组崩溃与冲突兼容性修复)**:
  - Fixed severe runtime crashes and broken automation when **Customize Buildings** (Steam ID 1818138009) is enabled alongside Automatic Industry.
  - **Oil Refinery & Oil Well Cap Component Resilience**:
    - Converted `[MyCmpReq]` to `[MyCmpGet]` on both `AutoOilRefinery` and `AutoOilWellCap`. Even if another mod forcibly destroys `OilRefinery` or `OilWellCap`, Unity/KMonoBehaviour will never log missing component engine errors or crash on building spawn.
    - Added defensive null guards throughout `AutoOilRefinery.Sim200ms`, `StopAutomation`, and `AutoOilWellCap.Sim1000ms`, `UpdateProgressBar`, safely returning early without throwing `NullReferenceException`.
  - **Harmony Prefix Shim (`CustomizeBuildingsCompatibility`)**:
    - Dynamically intercepts and cancels destructive patches from Customize Buildings by prefixing target methods with `return false`:
      - Suppressed `CustomizeBuildings.OilRefineryConfig_ConfigureBuildingTemplate.Postfix`, preventing the destruction of `OilRefinery` and unwanted attachment of `WaterPurifier`.
      - Suppressed `CustomizeBuildings.OilWellCapConfig_ConfigureBuildingTemplate.Postfix`, preventing the destruction of `OilWellCap` and unwanted attachment of `WaterPurifier`.
      - Suppressed `CustomizeBuildings.Compost_States_Patch.Postfix`, preventing `inert.GoTo(composting)` from triggering infinite state recursion and freezing game threads.
      - Suppressed `CustomizeBuildings.Desalinator_Patch.Postfix`, preserving Desalinator state machine transitions and threshold dumping.
      - Suppressed `CustomizeBuildings.NoDupeHelper.SetAutomatic`, preventing fabricators from having `duplicantOperated` wiped to false at the BuildingDef level.
      - Suppressed `CustomizeBuildings.NoDupeMods.EditGO` and `NoDupe_IceCooledFan.EditGO`, preventing removal of `IceCooledFan` and `IceCooledFanWorkable`.
  - **Dynamic State Options Neutralization**:
    - Via reflection, dynamically disables conflicting options (`NoDupeOilRefinery`, `NoDupeOilWellCap`, `NoDupeCompost`, `NoDupeDesalinator`, `NoDupeIceCooledFan`) in `CustomizeBuildingsState.Instance`.
  - **ComplexFabricator Decoupling in `BuildingPrefabInjection`**:
    - Decoupled `AutoFabricatorController` injection from `fabricatorCmp.duplicantOperated`, checking `prefab.GetComponent<ComplexFabricatorWorkable>() != null` directly. All duplicant-workable fabricators receive full automation and UserMenu customizer toggles regardless of mod load order.
  - **Defensive Injection Safeguards**:
    - Added null checks before attaching `AutoOilRefinery`, `AutoOilWellCap`, and `AutoIceCooledFanController` in `BuildingPrefabInjection`.

## 2.4.33

- **Spice Grinder Universal Multi-Ingredient Automated Delivery (香料研磨器多材料全自动输送与运送修复)**:
  - Fixed an issue where the automated Spice Grinder failed to receive multi-ingredient supplies (such as "Preserving Spice" / 清新香料: Meal Lice Seed 0.1 units + Salt 3 kg), leaving both Duplicants and Auto-Sweepers unable to deliver and halting machine operation.
  - **Redirected Ingredient Chores to `FabricateFetch`**: Intercepted vanilla `SpiceGrinder.StatesInstance.CreateFetchChore` to dispatch errands as `Db.Get().ChoreTypes.FabricateFetch` instead of vanilla `CookFetch`. This allows Auto-Sweepers (`SolidTransferArm`) to recognize and deliver all spice ingredients while maintaining full compatibility with Duplicants.
  - **Resolved Deadlocks with "No Manual Delivery"**: Because the Spice Grinder is automatable, "No Manual Delivery" holds manual Duplicant deliveries when covered by an Auto-Sweeper. Converting chores to `FabricateFetch` allows the Auto-Sweeper to fulfill them smoothly, completely eliminating the deadlock where neither Duplicants nor sweepers could deliver.
  - **Expanded Storage Capacity Headroom**: Increased `seedStorage.capacityKg` to a generous headroom threshold (`Mathf.Max(TotalKG * 20f, 100f)`). In vanilla, setting capacity to exactly 10 batches (31.0 kg) resulted in 30 kg salt filling the storage and leaving < 1.0 kg remaining capacity, which permanently blocked discrete 1.0 kg seeds (`BasicSingleHarvestPlantSeed`) from being accepted.
  - **Advance & Continuous Ingredient Pre-Stocking**: Implemented `AutoSpiceGrinderController.EnsureIngredientFetches()` to actively request up to 10 batches of all required recipe ingredients immediately upon recipe selection, eliminating the vanilla requirement of waiting for food to arrive before creating ingredient chores.
  - **Self-Healing Interrupted Chores**: Fixed vanilla deadlock where cancelled or interrupted fetch chores permanently remained in `SpiceFetches[i]` and locked `HasOpenFetches` to `true` forever. Added safe completion and end handlers (`ClearFetchChoreSafe`, `OnFetchEndedSafe`) to clear abandoned chores and dynamically reschedule needed deliveries.
  - **Comprehensive Storage Filters**: Expanded `seedStorage.storageFilters` to include all spice ingredient tags (`GameTags.Seed`, `GameTags.CropSeed`, `Salt`, `Sucrose`, `Iron`, `SlimeMold`, `IndustrialIngredient`, `Solid`), preventing external storage probes and filtering routines from rejecting salt or mineral deliveries.

## 2.4.32

- **Compatibility Fix for "No Manual Delivery" Mod (与 No Manual Delivery 模组崩溃与冲突兼容性修复)**:
  - Fixed an unhandled `NullReferenceException` crash when "No Manual Delivery" (Steam ID 2047308624) is installed alongside Automatic Industry.
  - Implemented `NoManualDeliveryCompatibility`: dynamically shims `TransferArmGroupProber.Get()` to fall back safely to `MinionGroupProber.Get()`, preventing fatal null-dereference crashes during save loading, pre-game initialization, or when HoldMode is disabled in settings.
  - Implemented `WorkableWorkerSafetyPatch`: intercepts `Workable.GetAnim(worker)` for non-multitool workers (`worker.UsesMultiTool() == false` such as `SolidTransferArm`), preventing unhandled `NullReferenceException` inside `MultitoolController` when transfer arms interact with workables (such as bottled liquids and pumps).
  - Refined `AssetsIsTagSolidTransferArmConveyablePatch` to precisely whitelist bottled liquids, gases, canisters, medicines, and element tags while explicitly blocking creature, minion, and non-pickupable system tags.
  - Guarded against duplicate `MachineFetch` delivery component injections on `AdvancedResearchCenter`.

## 2.4.31

- **Fix Kiln Ceramic Crash & ComplexFabricator Safety Guards (窑炉陶瓷制作崩溃与无人值守建筑防护修复)**:
  - Fixed an unhandled `NullReferenceException` crash when the Kiln produces Ceramic (or any recipe), caused by injecting `AutoFabricatorController` into naturally unattended fabricators and forcing `duplicantOperated = true` with a null workable.
  - Added strict workable filtering in `BuildingPrefabInjection`: only fabricators that are duplicant-operated and have a `ComplexFabricatorWorkable` (such as Metal Refinery, Electric Grill, Rock Crusher, etc.) receive the automated controller. Naturally unattended fabricators (`Kiln`, `Chlorinator`, `DataMiner`, `RubberMaker`, `Smoker`, `UraniumCentrifuge`) remain vanilla unattended.
  - Added safety checks in `AutoFabricatorController.Prepare`, `Step`, and `StopAutomation` as well as `AutoBuildingCustomizer.SyncBuildingState` to ensure `fabricator.Workable != null` before manipulating chore or duplicantOperated states.
  - Fixed `FOODDEHYDRATOR` mechanism in `AutomationRegistry` to `AutomationMechanism.Release`, as Food Dehydrator is natively unattended and only requires automated finished-packet ejection.
  - Added self-healing guard in `ComplexFabricatorOnSpawnPatch` and null check in `ComplexFabricatorSim1000msPatch` to guarantee robust operation across all mods and existing save files.

## 2.4.30

- **ComplexFabricator Manual Mode Switch & Chore Restoration (制造类建筑切回手动制造任务恢复与自愈修复)**:
  - Fixed an issue where switching the Metal Refinery (and other `ComplexFabricator` machines) from automated to manual in-game caused duplicants to only supply materials without performing manual fabrication tasks.
  - Implemented automatic chore recreation upon toggling to manual in `AutoBuildingCustomizer.SyncBuildingState` and `AutoFabricatorController.StopAutomation`.
  - Added self-healing postfix in `ComplexFabricator.Sim1000ms` that detects waiting working orders in manual mode and automatically issues the required `WorkChore`.
- **TinkerStation & Workstation Animation and Duplicant Chore Suppression (电控站及工作站自动化抑制与动画修复)**:
  - Intercepted duplicant chore generation across `TinkerStation`, `SpiceGrinder`, `ResearchCenter`, `NuclearResearchCenter`, `GeneticAnalysisStation`, `GeoTuner`, `Telescope`, and `FoodSmoker` during unattended operation.
  - Fixed duplicant operate animation freezing and ensured uninterrupted automatic microchip crafting.

## 2.4.29

- **Universal Station Chore Suppression & ONI Together Integration (工作站通用抑制与联机API支持)**:
  - Added multi-building operate chore suppression rules to prevent Duplicant competition on automated stations.
  - Added full compatibility with `ONI_Together_API` v0.7.2.

## 2.4.28

- **Per-Building Independent Automation & Dual-Track Coexistence (单建筑独立控制与双轨状态机并存)**:
  - **In-Game UI Toggle Action**: Added dynamic `Enable Automation` / `Revert to Manual` action buttons to the UserMenu card of every automated building, allowing players to run mixed setups (e.g. 7 automated grills and 3 manual grills) seamlessly within the same game colony.
  - **Immersive Duplicant Wrench Modification**: Standard toggle dispatches a `Toggle` chore where a duplicant approaches with a wrench and performs a ~2-second tuning interaction (`anim_interacts_wrench_kanim`) before switching states.
  - **Zero-Dupe & Debug Fast-Path**: Clicking in Debug/Sandbox mode, pressing `Shift + Click`, or operating in colonies with 0 duplicants instantly completes the toggle without queuing unreachable chores, preventing testing or sterile map deadlocks.
  - **Copy Settings Integration**: Players can click `Copy Settings` on an upgraded machine and drag-box select multiple buildings across the colony to batch-apply individual automation states.
  - **Multi-tier Recovery & Global Sync**: Individual `Follow Global Setting` reset button on building cards, paired with colony-wide and category-wide reset options in `ColonyAutomationMasterRegistry` attached to `SaveGame`.
  - **Full 5-Language Localization**: Complete translations in English, 简体中文, 繁體中文, 한국어, and 日本語 for all UI buttons, tooltips, status items, and mod options.

## 2.4.27

- **Oil Refinery (原油精炼器) Dynamic Conversion Ratio Hot-Reload**:
  - Automatically synchronizes conversion ratio (50% vanilla vs 100% full efficiency) dynamically on `AutoOilRefinery` spawn and `Sim200ms` ticks.
  - Changes made via Mod Menu configuration UI immediately take effect across all existing and newly constructed Oil Refineries without requiring game reload or restart.
- **Auto-Sweeper (自动清扫机) Universal Crop Harvesting & Conveyor Delivery Fix**:
  - Expanded `AutoSweeperHarvestController.PlantVisitor` to match all plant entities (`GameObject`, `Component`, `KMonoBehaviour`, `KPrefabID`, `Harvestable`) registered across spatial partitioning layers, resolving an issue where domestic crops (e.g. Cold Wheat, Mealwood, Bristle Blossom) on Hydroponic Farm tiles were skipped.
  - Implemented multi-crop batch harvesting and reachability checks covering both crop coordinates and farm tile foundation cells.
  - Harvested seeds and produce immediately drop as conveyable pickupables, allowing Auto-Sweepers to deliver them to nearby filtered Conveyor Loaders (Solid Conduit Inboxes).

## 2.4.26

- **FastTrack & WorkAnim Safety Hardening**:
  - Added null-safe fallback and exception guards in `WorkAnim.cs` to prevent `NullReferenceException` crashes when `KBatchedAnimController` is in deferred initialization under FastTrack optimizations.
- **Zoned Solid Transfer Arm Compatibility**:
  - Integrated dynamic bounding box expansion and item filter respect for Zoned Transfer Arms.

## 2.4.25

- **Compost (堆肥堆) Dual Progress Bars, Loop Animations & Status Countdown**:
  - **Neat Bottom Alignment**: Placed the decomposition conversion progress bar (0% -> 100%) at the building's bottom tile. Aligned the dedicated light blue flip progress bar directly underneath it.
  - **Turning Shovel Loop Animation**: Explicitly triggers the pitchfork shovel turning loop animation during the flip phase and returns to resting state upon completion.
  - **Live Flip Countdown in Status Panel**: Added multilingual Status item (`STRINGS.BUILDING.STATUSITEMS.COMPOSTFLIPCOUNTDOWN`) displaying next flip countdown and flipping state in English, Simplified Chinese, Traditional Chinese, Korean, and Japanese.
- **Oil Well (油井 / OilWellCap) Threshold Progress Bar & Release Countdown**:
  - **Threshold-Relative Progress Bar**: Positioned progress bar cleanly at the building base, scaled relative to the player's configured release threshold (`currentPressure / threshold`).
  - **Live Depressurize Countdown in Status Panel**: Added multilingual Status item (`STRINGS.BUILDING.STATUSITEMS.OILWELLPRESSURECOUNTDOWN`) estimating remaining seconds until venting threshold is reached.
- **Ranching Stations (照料站、剪毛站、挤奶站及水下版本) Work Progress Bars**:
  - Attached live 0% -> 100% work progress bars to all 6 ranch stations (`RanchStation`, `ShearingStation`, `MilkingStation`, `UnderwaterRanchStation`, `UnderwaterShearingStation`, `UnderwaterMilkingStation`) while tending critters.
- **Sushi Bar (寿司台 / 寿司吧) Automation & Progress Bar**:
  - Registered `SUSHIBAR` in `AutomationRegistry` as a fabricator with automated recipe execution, unattended work animations, and floating food preparation progress bar.
- **Manual Generator (人力发电机) Duplicant Chore Suppression**:
  - Injected `AlwaysFalse` precondition on `ManualGenerator` operate chores in `EnergySim200ms` when automated, preventing duplicants from ever claiming or running on the generator wheel during unattended power generation.
- **Auto-Sweeper (自动清扫机) Bottled Water & Research Delivery**:
  - Patched `Assets.IsTagSolidTransferArmConveyable` to allow Auto-Sweepers to recognize and convey bottled water, bottled liquids, and bottled gases directly from Bottle Fillers (LiquidBottler), Pitcher Pump areas, and floor bottles to the Super Computer (Advanced Research Center).

## 2.4.24

- **Compost (堆肥堆) Visual & State Machine Restoration**:
  - **Option OFF (Vanilla Mode)**: Restored 100% vanilla Compost behavior. Completed 300kg polluted dirt delivery properly holds the static full pile state (`"on"`). Duplicants perform manual pitchfork turning chores with the turning animation, returning to the resting decomposition state between intervals.
  - **Option ON (Automated Mode)**: Synchronized visual model state on save/load so the full compost pile (`"on"`) never erroneously displays as an empty box (`"off"`). Automated flips quietly cycle into active decomposition, outputting dirt without requiring duplicant chores.
- **Gleaner (榨脂机 / Milk Fat Separator) Solid Output & Progress Bar**:
  - **Open-Air Output Placement**: Replaced vanilla `DropMilkFat` coordinate logic to place dropped solids (Caviar / Brackwax) directly into the open-air cell above foundation tiles with instant visibility and pickup reachability, eliminating subterranean entombment and save/reload delays.
  - **Progress Bar**: Restored dedicated `GleanerProgressBarComponent` dynamically tracking solid product capacity (0% - 100% / 15kg).
- **Ice-E Fan (冰冷风扇 / IceCooledFan) Duplicant Chore Suppression**:
  - **Chore Precondition Gating**: Implemented `IceCooledFanCreateUseChorePatch` injecting an `AlwaysFalse` precondition onto the fan operate chore when automated, preventing duplicants from ever queuing or being summoned for the Ice-E Fan while automation is active.
  - **Operate Chore Registry**: Registered `IceCooledFan`, `LiquidCooledFan`, `AnalyzeSeed`, `GeneratePower`, and `Depressurize` in `ChoreSuppression.OperateChoreIds` to cancel any existing active duplicate chores upon load/spawn.

## 2.4.9

- **Mod Menu Architecture Decoupling**:
  - Extracted the in-game Mod Menu subsystem into an independent standalone project and release (`ModMenu`).
  - `AutomaticIndustry` focuses cleanly on industrial automation while maintaining seamless bidirectional runtime option compatibility with the standalone `ModMenu`.
  - Added missing Japanese localization entry for `RESEARCHCENTER`.

## 2.4.8

- **PauseScreen Harmony Patch Fix**:
  - Replaced non-existent `OnActivate` patch target with ONI's native `PauseScreen.ConfigureButtonInfos` hook.
  - Injects `KButtonMenu.ButtonInfo` for "Mod Menu" directly between "Options" and "Colony Summary" natively, eliminating the load-time `Undefined target method` crash.
- **Ranching Station Claim Isolation**:
  - Ensures automated ranch stations (Grooming, Shearing, Milking, land & aquatic) respect other active stations' critter assignments in the same room.

## 2.4.7

- **Ranching Station Same-Room Multi-Station Conflict Fix**:
  - Resolved critter state jitter (rapid oscillation between `Excited` and `Being Groomed/Normal`) when multiple stations (Grooming, Shearing, Milking, land or aquatic) share the same stable room.
  - Implemented `RanchStationClaimPatch`: Prevents automated stations from stealing critters that are already claimed by an active, running ranch station in the room.
  - Refined `CheckArrivalWatchdog` in `AutoRanchStation`: Selectively evicts only the stalled critter at index 0 rather than wiping the entire queue.
- **Mod Menu Pause Screen Position & Stability**:
  - Updated `PauseScreenPatch` to explicitly insert the "Mod Menu" button right below "Options" and above "Colony Summary" (`siblingIndex = optionsIndex + 1`).
  - Utilizes `Util.KInstantiateUI` and hooks `OnPrefabInit`, `OnSpawn`, `OnActivate`, and `OnShow` to ensure persistent visibility.
- **World Warning Icon Multi-Skill Coverage**:
  - Added `Db.Get().BuildingStatusItems.ColonyLacksDupeWithMultiSkillPerk` to `HideWorldStatusIconsPatch` to support aquatic stations (`UnderwaterShearingStation`, `UnderwaterRanchStation`, `UnderwaterMilkingStation`).

## 2.4.6

- **Geotuner Material Delivery Gating (5% Remaining Threshold)**:
  - Material delivery requests (`ManualDeliveryKG`) are now automatically suppressed while tuning data broadcast is active and remaining duration is above 5%.
  - When the broadcast timer drops to 5% or less (or research is required), material requests open to pre-buffer the next batch just in time.
  - Automatic research completion seamlessly consumes the 50kg batch and restarts the broadcast upon expiration.
- **Mod Menu Pause Screen Injection Fix**:
  - Injected Mod Menu button across `OnPrefabInit`, `OnSpawn`, and `OnKeyDown` lifecycle events.
  - Cleared `LocText.key` to prevent ONI localization from reverting the button text.
  - Restored clean `KButton` click handlers to reliably open the in-game Mod Menu and live options editor.
- **Shearing Station & Aquatic Shearing Station Overhaul**:
  - Fixed critter queue congestion caused by vanilla `ValidateTargetRanchables` skipping validation when no duplicant is present.
  - Implemented proactive queue purging: continually verifies critter existence, `IShearable` scale growth status, room/cavity matching, and navigation path costs.
  - Added an Arrival Watchdog (15s timeout): automatically evicts stalled/blocked critters so other queued critters and diverse species in the same ranch can proceed immediately.
  - Added aquatic pathfinding reachability checks for `UnderwaterShearingStation`.

## 2.4.5

- **Geotuner Material Consumption Bugfix**:
  - Fixed infinite material consumption loop where Geotuner consumed freshly delivered materials during active broadcasting. Research completion now strictly triggers only when the Geotuner is in `researcherInteractionNeeded` state and not broadcasting.
- **Fabricator & Cooking Station Progress Bars**:
  - Restored dynamic progress bars on `SushiBar`, `GourmetCookingStation` (Gas Range), `MilkPress` (Plant Pulverizer), `MissileFabricator` (Blastshot Maker), and all unattended complex fabricators.
- **Power Control Station Progress Bar Fix**:
  - Fixed progress bar staying at 0% during Microchip fabrication by actively calculating and rendering production elapsed percentage over production duration.
- **Geyser Tuning Count Progress Bar**:
  - Added native floating progress bar beneath/over tuned Geysers indicating the number of Geotuners currently assigned (20% per Geotuner up to 5 = 100%).
  - Added dedicated toggle `ProgressBarGeyserTuning` under the Progress Bars category.
- **World Warning Icon Visibility Toggles**:
  - Added `HideWorldIconSkillRequirement` to hide the floating red skill requirement warning icon beneath buildings.
  - Added `HideWorldIconRoomRequirement` to hide the floating red room requirement warning icon beneath buildings.
  - Both options preserve full descriptions and tooltip warnings inside the selection side screen.
- **In-Game Mod Menu in Pause Screen**:
  - Injected "Mod Menu" button into the game Pause Screen.
  - Added in-game Mod Browser and real-time Options Editor allowing players to modify Automatic Industry options live during pause without returning to the main menu.
  - Decoupled Mod Menu architecture into `AutoMachineRebuilt.ModMenu` with dedicated `MOD_MENU_ARCHITECTURE.md` documentation for future standalone extraction.

## 2.4.4

- **Bottle Filler / Research Logistics Overhaul**:
  - Reverted aggressive ground dropping of bottled liquids/gases from `VanillaEmptyPaths.cs`.
  - Introduced `AutoBottler` for `LiquidBottler` and `GasBottler`: stored bottles set `targetWorkable = pickupable` and `allowItemRemoval = true`, allowing Auto-Sweepers to fetch directly from Bottle Fillers and containers into `AdvancedResearchCenter` without flooding floors.
  - Duplicants retain full fallback supply capability if no Auto-Sweeper is in range.
- **Research Stations Animation & Progress Bar**:
  - Added native floating `ProgressBar` to research buildings (`ResearchCenter`, `AdvancedResearchCenter`, `NuclearResearchCenter`, `CosmicResearchCenter`, `OrbitalResearchCenter`) reflecting real-time research points completion percentage.
  - Synchronized full 3-stage animation cycle (`working_pre` -> `working_loop` -> `working_pst`).
- **Geotuner Consumption Progress Bar**:
  - Added dedicated depletion progress bar on the Geotuner reflecting Amplification Data remaining (100% down to 0%) during broadcast, or material delivery progress.
  - Verified and preserved exact vanilla consumption rates (50kg consumed on research completion to produce 600s data decaying at -0.17%/s).
- **Comprehensive Building Progress Bars**:
  - Created dedicated "Progress Bars" category in mod options with individual toggles for Research Stations, Geotuner, Bottle Fillers, Fabricators / Cooking Stations, Telescopes / Enclosed Telescopes, Spice Grinder, Gleaner, and Power Control Station.
- **Power Control Station Microchip Automation**:
  - Enabled unattended Microchip fabrication on `PowerControlStation` via `AutoTinkerStationController`.
  - Added `IgnorePowerDemandPowerControlStation` option to continuously manufacture Microchips from refined metal even when no immediate power plant errand is pending.
- **Biobot Builder (Morb Rover Maker) Auto-Release**:
  - Created dedicated `AutoMorbRoverMaker` component: when crafting progress reaches 100% and germs reach required levels, the biobot is automatically released and deployed, cancelling the Duplicant doctor errand.
  - Added `UnmannedMorbRoverMaker` option toggle.

## 2.4.3

- **Super Computer (Advanced Research Center) & Water Logistics**:
  - Automated Bottle Filler (`LiquidBottler`), Canister Filler (`GasBottler`), and Pitcher Pump (`LiquidPumpingStation`) output release: filled bottles/canisters are automatically dispensed as genuine `Pickupables` on the ground/platform with `GameTags.LiquidSource` stripped and `targetWorkable = pickupable`.
  - Auto-Sweepers can now automatically pick up released water bottles and deliver them via the parallel `MachineFetch` delivery to the Super Computer.
  - Resolved research stalling by ensuring `AutoResearchController` detects delivered water, consumes water via `ElementConverter`, awards research points, and plays the `working_loop` animation.
- **Geotuner Automation & Room Waiver**:
  - Enabled `UnmannedGeoTuner` by default in options.
  - Added `IgnoreRoomGeoTuner` waiver option and registered `"GEOTUNER"` / `"GeoTuner"` in `RoomOverrideByKey`.
  - `AutoGeoTuner` automatically switches and assigns target geysers upon player side-screen selection, consumes delivered tuning material, invokes `GeoTuner.OnResearchCompleted`, suppresses duplicant research chores, and transitions into `broadcasting` (active during eruption, on-hold during dormancy).
- **EmptyStorage Mod Compatibility**:
  - Verified and confirmed 100% compatibility with the `EmptyStorage` mod. `ChoreSuppression` explicitly preserves `EmptyStorage` chores (`Db.Get().ChoreTypes.EmptyStorage`), and dropped items remain fully conveyable by Auto-Sweepers.
- **Packaging & Archiving Overhaul**:
  - Output artifacts are strictly delivered as `.zip` files (no uncompressed release/src folders left in root).
  - Previous release zips are archived into `zip_src_archived/`, keeping only the latest version in the root workspace.

## 2.4.2

- **Shared Mod Config Path**:
  - Migrated configuration storage to the standard shared ONI config folder: `C:\Users\{Username}\Documents\Klei\OxygenNotIncluded\mods\config\AutomaticIndustry\config.json` (`UseSharedConfigLocation = true`, staticID `AutomaticIndustry` without spaces).
- **Chore Suppression Safe Iteration**:
  - Resolved `System.InvalidOperationException: Collection was modified` in `LocalChoreProbe.CancelLocalOperateChores` by snapshotting state machine instances and gathering candidate operate chores in a list before cancellation.
- **Locale Reading Ambiguity Fix**:
  - Resolved `AmbiguousMatchException` in `OptionTextBinder.GetGameLocaleCode` on ONI U59+ by explicitly specifying parameterless method signature for `Localization.GetLocale()`.
- **Localization Coverage & Registry Parity**:
  - Normalized option key casing for `FABRICATEDWOODMAKER` and registered missing `RESEARCHCENTER` entries across all game localization tables (EN, ZH-Hans, ZH-Hant, KO) to pass localization audit cleanly.

## 2.4.1

- **Spice Grinder (Dedicated Controller)**:
  - Rebuilt with a dedicated `AutoSpiceGrinderController` hooking into `SpiceGrinder.StatesInstance` and `SpiceGrinderWorkable`.
  - Food delivery fetch chore changed from `CookFetch` to `FabricateFetch` on the spice grinder storage filter so Auto-Sweepers can supply food alongside Duplicants without mutating global chore types.
  - Spiced completed food is cleanly released as standard ground `Pickupable` (compatible with Auto-Sweeper transport to Refrigerators, MiniFridges, and Solid Conveyor Inboxes).
  - `IgnoreRoomSpiceGrinder` option is now enabled by default and sets the operational room flag.
- **Advanced Research Center (Dual-Path Supply)**:
  - Dual delivery paths for water: native `ResearchFetch` is fully preserved for Duplicants, while a parallel `MachineFetch` delivery path allows Auto-Sweepers to deliver bottled water.
  - Runtime toggling of `SweeperResearchDelivery` dynamically synchronizes the parallel sweeper delivery.
- **Operate Chore Suppression**:
  - Precise cancellation of machine operation chores (`Fabricate`, `Cook`, `Work`, `Research`, `MachineTinker`, `PowerTinker`, `Art`, `EmptyDesalinator`, `Spice`) on automated buildings while keeping all logistics, supply, empty, and maintenance chores intact.
- **Manual Radbolt Generator & Sushi Bar**:
  - Full synchronization of work progress, work time remaining, operational active states, progress bar (`showProgressBar = true`), and radiation emitter toggling.

## 2.4.0

- Gleaner (Milk Fat Separator) and Desalinator: the emptying is now hooked into
  the vanilla decision point itself (`MilkSeparator.RequiresEmptying` and
  `Desalinator.StatesInstance.CreateEmptyChore`). The moment the base game
  concludes the machine is full, the mod drops Brackwax, Caviar or salt and the
  machine keeps producing, so nothing can be swallowed at the 15 kg limit and
  no hidden salt stack survives a reload.
- New option "Auto-Sweeper delivery to research buildings": re-tags the supply
  delivery of Research Station, Supercomputer, Material Study Terminal, Virtual
  Planetarium and Orbital Data Collection Lab as a machine delivery, so an
  Auto-Sweeper can hand them bottled water and Data Banks. Requires a restart.
- Manual Radbolt Generator: the progress bar is enabled for unattended
  production and the building plays its own working loop while a batch runs.

## 2.3.8

- Aquatic and land Shearing Station: restored the 2.3.6 invitation flow (the
  serial invitation timer of 2.3.7 is gone), so critters keep using vanilla
  pathing and the station queue behaves like the base game again.
- Both Shearing Stations (and the other four ranch stations) now play a work
  animation while the automation tends a critter: the station kanim work loop
  plus the vanilla critter shearing/grooming/milking pre, loop and post
  animations.
- Gleaner: after the vanilla `DropMilkFat` routine, any solid item still left
  in the machine is dropped generically by element state. Caviar can no longer
  be lost at the 15 kg limit and future recipe products are covered too;
  liquid ingredients (Fish Milk, Mucus) and the piped output are never touched.
- Desalinator: salt is now released on two independent triggers - the hard mass
  threshold (90% of the vanilla 945 kg capacity) and the vanilla
  "no salt capacity left" counter - so the machine can never accumulate salt
  indefinitely.

## 2.3.6

- Renamed the mod title to **Automatic Industry**.
- Gleaner and Desalinator now complete their actual vanilla empty Workables
  instead of forcing an internal state or private callback. This preserves the
  full chore lifecycle and prevents hidden over-storage across save reloads.
- Aquatic Shearing Station follows the vanilla completion callback order for
  SeaTurtle: spawn the scale product, call `Shear()` to reset scale growth, then
  clear the station's displayed product symbol.

## 2.3.5

- The "Enable every automation" switch now works like a button: it ticks every
  building checkbox, clears itself and saves the configuration, so the options
  screen shows the real state afterwards.
- Gleaner now runs the vanilla `emptyComplete` state instead of dropping the
  storage directly, so Brackwax and Caviar are ejected at the capacity limit
  instead of getting stuck or lost.
- Desalinator now runs the vanilla empty callback while the building waits in
  `fullWaitingForEmpty`; salt is dropped, the pending Duplicant chore is
  cancelled and the vanilla state machine restores the salt capacity.
- Plywood Press option title uses the correct string key again.

## 2.3.4

- Gleaner, Desalinator and Ice Liquefier now release through the vanilla
  "empty me" callbacks only when their internal storage is actually full,
  so input material and piped output are never dropped.
- Research buildings (Research Station, Super Computer, Virtual Planetarium,
  Data Collection Lab, Plant Analyzer) no longer deadlock on a vanilla chore
  that never gets recreated; the pending chore is cancelled on takeover.
- Geotuner supports the room requirement waiver and checks its tuning
  material before it completes a tuning cycle.
- Shearing Stations keep their critter queue between cycles, which removes
  the pathing loop around the station.
- New automation: Plywood Press (Plywood Press recipes) and Power Control
  Station, the latter with its own "ignore room" and "ignore skill" waivers.
- New "Enable every automation" master switch in the General category.
- Localization: all new labels, tooltips and building descriptions in
  English, Simplified Chinese, Traditional Chinese, Korean and Japanese,
  with vanilla building names taken from the game's own translations.

## 2.3.3

- Fixed the missing controller integration for the dedicated vanilla emptying paths. Ice Liquefier,
  Gleaner and Desalinator are now intercepted before the generic storage scan, including while they
  are not full, so their shared storages can never fall through to `DropAll`.
- Ice Liquefier keeps every partial melt in its output tank and releases bottled liquid only when
  the vanilla capacity check says another batch no longer fits. Lumber and ice inputs remain intact.
- Gleaner waits for `operational.full` and enters the vanilla `emptyComplete` state, which releases
  only Milk Fat/Caviar while preserving Fish Milk/Milk Ice and Mucus/Brine conduit products.
- Desalinator automation now responds only to `fullWaitingForEmpty` and releases only Salt. The
  player-only `earlyWaitingForEmpty` path remains manual and Salt Water, Brine and Water stay stored.
- Generic release paths no longer request liquid dumping and restore pickup interactions on dropped
  bottles as a final safety barrier against floor spills.

## 2.3.2

- Ice Liquefier: automation follows the vanilla work cycle again. The building keeps melting ice
  batch by batch and is emptied only once its liquid tank can no longer hold another batch, which
  is the exact moment vanilla asks a Duplicant to carry the liquid out. The finished liquid leaves
  as bottles, never as a puddle, and lumber and ice inputs are never touched.
- Gleaner (Milk Fat Separator): the release now waits for the vanilla "needs emptying" state, so it
  fires only when the solid output reached the building capacity. Only Brackwax and Caviar are
  dropped; FishMilk, MilkIce, Brine and Mucus stay inside the machine and in the pipes.
- Desalinator: only the accumulated salt is released, and only while the building sits in its
  vanilla full/early-empty state. Salt water, Brine and Water are no longer dropped.
- The dedicated per-building release paths are now actually used by the release controller; the
  generic storage drop is skipped for those buildings and never dumps free-flowing liquid.

## 2.3.1

- Gleaner (Milk Fat Separator): the automatic emptying now runs the building's own vanilla
  release state instead of dropping its storage. Only the solid products (Brackwax and Caviar)
  leave the machine; Ovolene, Brine and Mucus stay in the liquid output as in the base game, so
  the two recipes can no longer spill or mix on the floor.
- Ice Liquefier and every other released building: liquids are handed over as bottles instead of
  being poured onto the ground. Dropped bottles get their normal pickup behaviour restored, so
  Duplicants can fetch them and the colony resource statistics account for them.

## 2.3.0

- Localization audit: every translated table is now checked against the English table at load.
  Missing rows, unknown rows and values written in the wrong script are reported in Player.log
  instead of surfacing as a raw key or a wrongly localized label on screen.
- Building names for English, Simplified Chinese, Korean and Traditional Chinese are generated
  from the official Klei string files, so every name matches the vanilla term. Traditional
  Chinese is derived from the official Simplified text; Japanese uses the game's own strings
  when a Japanese language pack is active and a corrected table otherwise.
- Options screen: the language and bilingual toggles are tracked globally, so switching back to
  "Auto (follow game)" also rebuilds the dialog immediately.
- Crash fix: a controller no longer calls into the simulation while its building is being torn
  down, which removed the "Accessing mismatched handle version" crash on the Ice-Cooled Fan.
- The automatic output release only logs when something was actually released, at most once per
  minute and per building, keeping Player.log readable.

## 2.2.1

- Ice Liquefier: the automatic release now only empties the finished liquid storage. Delivered
  lumber and ice are recognised as delivery buffers and are never dropped on the floor.
- Dehydrator: the finished dehydrated food is released automatically, so the building can start
  the next batch without a Duplicant.
- Options screen: switching the language (or the bilingual toggle) rebuilds the dialog right
  away instead of only taking effect the next time it is opened.
- Options screen: the mod now ships its own building names for Simplified Chinese, Traditional
  Chinese, Korean and Japanese, so the selected options language is used even when the game
  itself runs in a different one.
- Every automated building has its own tooltip describing the vanilla mechanic it reproduces and
  what the automation adds, in all five languages, instead of a shared generic text.

## 2.1.0

- Replaced the single generic work driver with one audited controller per vanilla mechanism:
  fabricators, output release, manual power, research, telescopes and compost.
- Complex fabricators (Apothecary, Nuclear Apothecary, Sushi Bar, Spice Grinder, Dehydrator,
  Biobot Builder, ...) now run through the vanilla unattended production path, which restores
  their original working animation, timing and recipe handling.
- Manual Generator follows the vanilla circuit decision, so a swapped or emptied battery
  restarts it immediately and a full circuit stops it.
- Research buildings are driven through the element converter instead of the work tick, which
  removes the Duplicant attribute lookup that could throw.
- Telescopes only work when the building itself reports a valid analysis target, removing the
  "no target assigned" assertion crash.
- Compost is turned with a vanilla state jump instead of a work tick.
- New options: Desalinator and Gleaner automatic output release.
- Every controller keeps the per building circuit breaker: three failures disable the automation
  of that one building and leave it fully playable with Duplicants.

# Changelog

All notable changes to this project are documented here.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)
and this project adheres to [Semantic Versioning](https://semver.org/).

## 2.0.7

- Fixed: the mod options button disappeared from the mods screen. `PLib.dll`
  was shipped as a separate file next to the mod, so the game loaded it as a
  second mod DLL and PLib attributed the registered options to itself
  (`[PLib/PLib] ...`) instead of this mod. PLib is now merged into
  `AutoMachineRebuilt.dll` with ILRepack (`/internalize`) and no longer shipped
  separately.
- `build.sh` now requires `ILREPACK` and performs the merge automatically.

## 2.0.6

- Set `minimumSupportedBuild` to 744825 (current U59 build) so the game no longer
  flags the mod as "requires update" on every launch. No code changes.

## [2.0.5] - 2026-08-14

### Fixed

- Game could crash while loading with `TypeInitializationException` on
  `MorbRoverMakerConfig` / `SpiceGrinderConfig`, followed by
  `Exception in RegisterBuilding` for every building in the game.
  Cause: Harmony patching a building config class forces its static
  constructor to run at mod load time, and several vanilla configs call
  `Db.Get()` there, before the game database exists. The failed type
  initializer is cached by the runtime and poisons building registration.
- Automation components are now attached by scanning `Assets.BuildingDefs`
  after `GeneratedBuildings.LoadGeneratedBuildings`, so no building config
  class is ever touched during mod load. Prefab identifiers are read from the
  `ID` constant through metadata (`GetRawConstantValue`), which also does not
  trigger a static constructor.
- The Oil Refinery ratio option and the Oil Well / Smoker / Geotuner /
  ranching components use the same late injection path.

## [2.0.4] - 2026-08-14

### Added
- Generic automation driver `AutoWorkDriver` (`src/Components/AutoWorkDriver.cs`).
  It never bypasses vanilla conditions: it waits until the building itself
  publishes a work chore (meaning power, ingredients, cooldown and environment
  are all satisfied), then advances the vanilla `Workable` with
  `WorkTick(null)` and finishes with `CompleteWork(null)`.
- `ChoreIndex` (`src/Automation/ChoreIndex.cs`): Harmony patches on
  `ChoreProvider.AddChore` / `RemoveChore` and the global provider build a
  GameObject -> chore index, which is how the driver knows a building wants to
  work.
- Table-driven target list `AutomationRegistry` (`src/Automation/`), mapping
  building config type names to option keys. Types are resolved by name, so a
  missing DLC simply skips the entry.
- New automated buildings, all **disabled by default**:
  Manual Generator, Telescope / Enclosed Telescope / Cluster Telescope,
  Manual Radbolt Generator, Skill Scrubber, Ice Kettle, Campfire, Ice-E Fan,
  Compost, Dehydrator, Super Computer, Virtual Planetarium (base + Spaced Out),
  Materials Study Terminal, Orbital Research Center, Botanical Analyzer,
  Biobot Builder. Apothecary / Advanced Apothecary / Sushi Bar go through the
  existing `duplicantOperated = false` fabricator path.
- Geyser first study: optional automation of `Studyable` on geysers and POI
  features (`GEYSERSTUDY` toggle).
- Room-bound group: Mission Control (base + Spaced Out cluster), Farm Station,
  Spice Grinder — each with its own **Ignore room requirement** switch that
  downgrades the tracker requirement from `Required` to `Recommended`. Off by
  default, so vanilla room rules stay intact.

### Fixed
- Working animations. Previously `AutoOilRefinery` and `AutoRanchStation`
  invoked completion callbacks directly, so buildings stayed in their idle
  animation. `src/Util/WorkAnim.cs` now plays the working animation while an
  automated cycle runs and returns to idle afterwards; the Oil Refinery and all
  six ranching stations (land and aquatic) use it, as does `AutoWorkDriver`.

### Changed
- Options screen gained three categories — **Manual**, **Research**,
  **Room-Required** — with full translations (en, zh-CN, zh-TW, ko, ja).
  Building names are still read from the running game's own strings.
- Documentation: added this changelog and `docs/DEVELOPMENT.md`.

## [2.0.3] - 2026-08-13
- `mod_info.yaml` reformatted for Klei's strict parser: `staticID` first,
  `supportedContent` ids separated by comma + space, `minimumSupportedBuild`
  set to 642695, legacy `supported_version` removed.

## [2.0.2] - 2026-08-13
- Replaced the unsupported `supportedContent: ALL` token with the explicit id
  list `VANILLA_ID, EXPANSION1_ID, DLC2_ID, DLC3_ID, DLC4_ID, DLC5_ID`.

## [2.0.1] - 2026-08-13
- Registered `LocString` keys during `OnLoad`, fixing raw keys such as
  `STRINGS.AUTOMACHINEREBUILT.CATEGORY.FABRICATORS` and the resulting options
  screen overflow.
- Added an options language selector (Auto / en / zh-CN / zh-TW / ko / ja) and
  a bilingual label mode.

## [2.0.0] - 2026-08-13
- Full rebuild of the original *Auto Machine* mod: per-building toggles via
  PLib, vanilla-faithful Oil Refinery ratio (with a 100% legacy option),
  automatic Oil Well pressure release, Geotuner, Smoker emptying and the six
  ranching stations, MIT licensed with original attribution preserved.
