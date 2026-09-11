# Automated Buildings Code Logic & Function Reference

This document is the definitive technical reference for every automated building in **Automatic Industry (Auto Machine Rebuilt)**. It explains the exact C# controller classes, interfaces, lifecycle methods (`OnPrefabInit`, `OnSpawn`, `Step`, `StopAutomation`), state machine hooks, chore suppressions, progress bars, and operational logic.

---

## 📑 Category Navigation

- [1. Power Category](#1-power-category) (Manual Generator, Manual Radbolt Generator)
- [2. Food & Cooking Category](#2-food--cooking-category) (Cooking Fabricators, Spice Grinder, Food Dehydrator, Food Smoker)
- [3. Plumbing & Ventilation Category](#3-plumbing--ventilation-category) (Liquid & Gas Valves, Bottle & Canister Fillers)
- [4. Refinement Category](#4-refinement-category) (Oil Refinery, Desalinator, Compost, Bleach Stone Hopper / Gleaner, Ice Liquefier, Crafting Fabricators)
- [5. Stations & Ranching Category](#5-stations--ranching-category) (Ranching, Farm Station, Power Control Station, Mission Control, Geotuner, Biobot Builder)
- [6. Research & Science Category](#6-research--science-category) (Research Centers, Materials Study Terminal, Botanical Analyzer, Telescopes)
- [7. Utilities Category](#7-utilities-category) (Oil Well Cap, Ice-E Fan, Reset Skills, Campfire)
- [8. Auto-Sweeper Crop Harvesting](#8-auto-sweeper-crop-harvesting) (Universal Plant Harvesting Engine)

---

## 1. Power Category

### Manual Generator (Hamster Wheel)
- **Prefab**: `ManualGenerator`
- **Controller Class**: [`AutoManualGeneratorController`](https://github.com/Eurekalo/Automatic_Industry/blob/main/AutomaticIndustry-src-2.4.0/AutomaticIndustry-src-2.4.0/src/Components/AutoManualGeneratorController.cs)
- **Base Class**: `AutoWorkControllerBase` (implements `ISim200ms` cadence)

#### Code Function Logic:
1. **Lifecycle (`Prepare()`)**:
   - Caches `ManualGenerator`, `Operational`, and `KBatchedAnimController`.
2. **Work Assessment (`Step(float dt)`)**:
   - Checks if a Duplicant is already manually running on the wheel (`generator.worker != null`). If so, yields immediately.
   - Evaluates connected power grid capacity: if connected batteries require charging (`generator.JoulesToGenerate > 0f` or circuit not saturated):
     - Sets building active: `SetActive(true)`.
     - Starts hamster wheel animation: `StartAnimation()` (`working_loop`).
     - Calls `generator.GenerateJoules(dt)` directly into the electrical network.
     - Calls `ChoreSuppression.CancelOperateChores(gameObject)` to stop Duplicants from queueing run errands.
3. **Safety Shutdown (`Reset()`)**:
   - When the battery bank reaches 100% or the grid is disconnected: stops animation, clears active operational state, and idles.

---

### Manual Radbolt Generator (Manual High Energy Particle Spawner)
- **Prefab**: `ManualHighEnergyParticleSpawner`
- **Controller Class**: Handled via [`AutoFabricatorController`](https://github.com/Eurekalo/Automatic_Industry/blob/main/AutomaticIndustry-src-2.4.0/AutomaticIndustry-src-2.4.0/src/Components/AutoFabricatorController.cs)
- **Design Rationale**: The Manual Radbolt Generator is built on ONI's `ComplexFabricator` architecture. Running it via `AutoFabricatorController` preserves the vanilla radbolt recipe duration, storage progress meter, and wheel animation without manual Workable hacking.

---

## 2. Food & Cooking Category

### ComplexFabricator Cooking Stations
- **Prefabs**: `MicrobeMusher`, `CookingStation` (Electric Grill), `GourmetCookingStation` (Gas Range), `Deepfryer`
- **Controller Class**: [`AutoFabricatorController`](https://github.com/Eurekalo/Automatic_Industry/blob/main/AutomaticIndustry-src-2.4.0/AutomaticIndustry-src-2.4.0/src/Components/AutoFabricatorController.cs)

#### Code Function Logic:
1. **Chore Detachment (`Configure(string optionKey)`)**:
   - Flips `ComplexFabricator.duplicantOperated = false`.
   - By setting `duplicantOperated` to false, the vanilla state machine suppresses publishing `WorkChore` to the Duplicant colony brain, while fetch and delivery errands remain active for Auto-Sweepers and Duplicants.
2. **Execution Loop (`Step(float dt)`)**:
   - Checks `fabricator.CurrentWorkingOrder`. If ingredients are deposited and recipe is queued:
     - Advances cooking recipe: `workable.WorkTick(null, dt)`.
     - Drives the station's `working_loop` animation.
     - Updates the cooking progress meter.
     - When work reaches 100%, vanilla fabricator triggers `CompleteWorkingOrder()`, deducting ingredients and spawning the cooked meal.
3. **Restoration on Manual Mode (`StopAutomation()`)**:
   - If the player toggles the building back to manual in `AutoBuildingCustomizer`:
     - Resets `duplicantOperated = true`.
     - Recreates `WorkChore` on `ComplexFabricatorWorkable`.
     - Dispatches `Game.Instance.Trigger((int)GameHashes.FabricatorOrdersUpdated)`.

---

### Spice Grinder
- **Prefab**: `SpiceGrinder`
- **Controller Class**: [`AutoSpiceGrinderController`](https://github.com/Eurekalo/Automatic_Industry/blob/main/AutomaticIndustry-src-2.4.0/AutomaticIndustry-src-2.4.0/src/Components/AutoSpiceGrinderController.cs) & [`SpiceGrinderPatches`](https://github.com/Eurekalo/Automatic_Industry/blob/main/AutomaticIndustry-src-2.4.0/AutomaticIndustry-src-2.4.0/src/Patches/SpiceGrinderPatches.cs)

#### Code Function Logic:
1. **Deadlock Prevention via Headroom Expansion (`ConfigurePrefabStorage`)**:
   - In vanilla, `seedStorage.capacityKg = totalKg * 10f` (only 31 kg for Preserving Spice). Delivering 30 kg of salt left <1.0 kg capacity, which deadlocked delivery because seed items have a minimum 1.0 kg discrete mass.
   - Automatic Industry expands capacity to `Mathf.Max(totalKg * 20f, 100f)`, guaranteeing salt and seeds can always be delivered in parallel.
2. **Auto-Sweeper Delivery Conversion (`EnsureIngredientFetches`)**:
   - Intercepts spice ingredient delivery errands and sets their chore type to `Db.Get().ChoreTypes.FabricateFetch` instead of vanilla `CookFetch`.
   - Allows Auto-Sweepers (`SolidTransferArm`) to load seeds, salt, sucrose, and iron without Duplicant intervention.
3. **Advance Stocking & Fetch Self-Healing**:
   - Pre-stocks up to 10 batches of ingredients even when no food dish is currently placed in the grinder.
   - Cleans dead/cancelled chore handles in `SpiceFetches` via `OnFetchEndedSafe`, permanently resolving the vanilla bug where cancelling a delivery froze the grinder's state machine.

---

### Food Dehydrator & Food Smoker
- **Prefabs**: `FoodDehydrator`, `Smoker`
- **Controllers**: [`AutoStorageReleaseController`](https://github.com/Eurekalo/Automatic_Industry/blob/main/AutomaticIndustry-src-2.4.0/AutomaticIndustry-src-2.4.0/src/Components/AutoStorageReleaseController.cs) & [`AutoFoodSmoker`](https://github.com/Eurekalo/Automatic_Industry/blob/main/AutomaticIndustry-src-2.4.0/AutomaticIndustry-src-2.4.0/src/Components/AutoFoodSmoker.cs)

#### Code Function Logic:
- Both machines dehydrate or smoke rations automatically, but vanilla requires a Duplicant to walk over and take out the finished product.
- **Dehydrator Dual Mechanism**:
  - `AutoFabricatorController` drives the dehydration process.
  - `AutoStorageReleaseController` checks the output storage every second: as soon as dried food packets are created, it calls `storage.DropAll()`, clearing the machine for the next batch.
- **Food Smoker**:
  - Monitors smoked food products and drops them to the floor for conveyor loader delivery.

---

## 3. Plumbing & Ventilation Category

### Liquid Valve & Gas Valve
- **Prefabs**: `LiquidValve`, `GasValve`
- **Controller Class**: [`AutoValveController`](https://github.com/Eurekalo/Automatic_Industry/blob/main/AutomaticIndustry-src-2.4.0/AutomaticIndustry-src-2.4.0/src/Components/AutoValveController.cs)

#### Code Function Logic:
1. **Flow Rate Synchronization (`Sim200ms`)**:
   - Compares `Valve.desiredFlow` with `Valve.currentFlow`.
   - If the player modifies the slider:
     - Sets `ValveBase.CurrentFlow = Valve.desiredFlow` immediately in memory.
     - Cancels pending Duplicant wrench errands: `valve.CancelPendingChore()`.
     - Refreshes the valve's visual needle meter.

---

### Bottle Filler & Canister Filler (Bottler Automation)
- **Prefabs**: `LiquidBottler` (Bottle Filler), `GasBottler` (Canister Filler), `LiquidPumpingStation` (Pitcher Pump)
- **Controller Class**: [`AutoBottler`](https://github.com/Eurekalo/Automatic_Industry/blob/main/AutomaticIndustry-src-2.4.0/AutomaticIndustry-src-2.4.0/src/Components/AutoBottler.cs)
- **Interface**: `ISim1000ms`

#### Code Function Logic:
1. **Direct Auto-Sweeper Pickup**:
   - In vanilla, bottled liquids/gases stored inside a filler are locked to Duplicant fetching only.
   - `AutoBottler.Sim1000ms` checks stored bottles:
     - Sets `storage.allowItemRemoval = true`.
     - Re-targets `pickupable.targetWorkable = pickupable` on stored bottle items.
   - **Benefit**: Auto-Sweepers (`SolidTransferArm`) can reach into the filler and load bottles directly onto Conveyor Loaders without dumping liquids onto the ground!
2. **Dynamic Progress Bar**:
   - When `ProgressBarBottler` is enabled in options, renders a real-time filling progress bar (0% -> 100%) tracking accumulated mass against `storage.capacityKg`.

---

## 4. Refinement Category

### Oil Refinery
- **Prefab**: `OilRefinery`
- **Controller Class**: [`AutoOilRefinery`](https://github.com/Eurekalo/Automatic_Industry/blob/main/AutomaticIndustry-src-2.4.0/AutomaticIndustry-src-2.4.0/src/Components/AutoOilRefinery.cs)
- **Interface**: `ISim200ms`

#### Code Function Logic:
1. **Null-Safe Component Binding**:
   - Uses Unity `[MyCmpGet]` for `refinery` and `operational`, preventing crashes if other mods inject components or alter the hierarchy.
2. **State Machine Interception (`Sim200ms`)**:
   - Checks `refinery.smi.GetCurrentState()`.
   - When the refinery enters `ready` state (power connected, crude oil input > 0 kg, ambient gas pressure < 5.0 kg):
     - Activates building: `operational.SetActive(true)`.
     - Loops refining animation (`working_loop`).
     - Cancels Duplicant operate errands via `ChoreSuppression.CancelOperateChores(gameObject)`.
3. **Efficiency Synchronizer (`SyncEfficiency`)**:
   - Dynamically reconfigures `ElementConverter` output rates based on player settings:
     - **Vanilla 50% Rate**: `5 kg/s Petroleum` + `0.09 kg/s Natural Gas`
     - **Full 100% Rate**: `10 kg/s Petroleum` + `0.18 kg/s Natural Gas`

---

### Desalinator
- **Prefab**: `Desalinator`
- **Logic Components**: [`HardThresholdRelease`](https://github.com/Eurekalo/Automatic_Industry/blob/main/AutomaticIndustry-src-2.4.0/AutomaticIndustry-src-2.4.0/src/Components/HardThresholdRelease.cs) & [`VanillaEmptyPaths`](https://github.com/Eurekalo/Automatic_Industry/blob/main/AutomaticIndustry-src-2.4.0/AutomaticIndustry-src-2.4.0/src/Components/VanillaEmptyPaths.cs)

#### Code Function Logic:
1. **Dedicated Threshold Emptying**:
   - Vanilla mixes salt, input saltwater, and output clean water in the same storage. A blunt `DropAll()` would dump saltwater onto the floor.
   - `HardThresholdRelease.TryReleaseDesalinator` monitors accumulated salt.
   - When salt mass reaches threshold (>= 90% capacity or 945 kg):
     - Invokes `DesalinatorWorkableEmpty.CompleteWork(null)` cleanly.
     - Drops only the solid Salt items.
     - Calls `VanillaEmptyPaths.RestoreDroppedInteractions(buffer)`: restores pickup tags and lifts salt out of foundation tiles if embedded.
     - Resets state machine to `empty` and resumes liquid filtration immediately.

---

### Compost
- **Prefab**: `Compost`
- **Controller Class**: [`CompostAutomationComponent`](https://github.com/Eurekalo/Automatic_Industry/blob/main/AutomaticIndustry-src-2.4.0/AutomaticIndustry-src-2.4.0/src/Components/CompostAutomationComponent.cs)
- **Interface**: `ISim200ms`

#### Code Function Logic:
1. **Primary Conversion Bar**:
   - Tracks transformation of polluted dirt into clean dirt (0% -> 100%).
2. **Automated Pitchfork Flip**:
   - When the pile enters `inert` state (waiting for a Duplicant to turn the compost):
     - Starts a 10-second automated flipping timer (`AutoFlipDuration = 10f`).
     - Activates secondary **Sky Blue flip progress bar** directly under the conversion bar.
     - Plays pitchfork shovel animation (`working_loop`).
     - When timer reaches 10s, smoothly transitions state machine back to `composting`.
     - Displays time remaining until next flip in the Status panel.

---

### Bleach Stone Hopper / Gleaner (Milk Fat Separator)
- **Prefab**: `MilkFatSeparator`
- **Components**: [`GleanerProgressBarComponent`](https://github.com/Eurekalo/Automatic_Industry/blob/main/AutomaticIndustry-src-2.4.0/AutomaticIndustry-src-2.4.0/src/Components/GleanerProgressBarComponent.cs) & [`AutoStorageReleaseController`](https://github.com/Eurekalo/Automatic_Industry/blob/main/AutomaticIndustry-src-2.4.0/AutomaticIndustry-src-2.4.0/src/Components/AutoStorageReleaseController.cs)

#### Code Function Logic:
- Operates automatically, rendering a custom progress bar during separation and dropping solid outputs (Bleach Stone / Brackwax) without spilling liquid buffers.

---

### Ice Liquefier (Ice Kettle)
- **Prefab**: `IceKettle`
- **Controller Class**: [`VanillaEmptyPaths.ReleaseIceKettle`](https://github.com/Eurekalo/Automatic_Industry/blob/main/AutomaticIndustry-src-2.4.0/AutomaticIndustry-src-2.4.0/src/Components/VanillaEmptyPaths.cs)

#### Code Function Logic:
- The Ice Liquefier has 3 storages: fuel (lumber), ice, and melted liquid.
- The mod inspects `smi.LiquidTankHasCapacityForNextBatch`.
- When the liquid output tank is completely full, drops the bottled liquid only (`storages[2]`), leaving fuel and unmelted ice intact inside the machine.

---

## 5. Stations & Ranching Category

### Ranching Stations (Grooming, Shearing, Milking)
- **Prefabs**: `RanchStation`, `ShearingStation`, `MilkingStation`, plus Underwater DLC variants
- **Controller Class**: [`AutoRanchStation`](https://github.com/Eurekalo/Automatic_Industry/blob/main/AutomaticIndustry-src-2.4.0/AutomaticIndustry-src-2.4.0/src/Components/AutoRanchStation.cs) & `RanchCompletionGuard`

#### Code Function Logic:
1. **Critter Room Scan (`Sim1000ms`)**:
   - Checks room cavity boundaries for eligible critters.
   - Filters critters missing the ranching buff (`Groomed`, `Sheared`, `Milked`).
2. **Automated Ranching Errand**:
   - Tends 1 eligible critter per interval.
   - Applies the 6-cycle ranching effect directly to the creature's `Effects` component.
   - Dispenses resource outputs (wool, reed fiber, brackish milk) to the station floor.
   - `RanchCompletionGuard` ensures that if a critter moves or despawns during the operation, state machines unbind cleanly without memory leaks.
   - Duplicant ranching errands are cancelled via `ChoreSuppression`.

---

### Farm Station & Power Control Station
- **Prefabs**: `FarmStation`, `PowerControlStation`
- **Controller Class**: [`AutoTinkerStationController`](https://github.com/Eurekalo/Automatic_Industry/blob/main/AutomaticIndustry-src-2.4.0/AutomaticIndustry-src-2.4.0/src/Components/AutoTinkerStationController.cs)

#### Code Function Logic:
1. **Demand Evaluation**:
   - **Farm Station**: Checks if plants on the asteroid require Micronutrient Fertilizer (waivable via "Ignore Crop Demand" setting).
   - **Power Station**: Checks if generators require Microchips (waivable via "Ignore Power Demand" setting).
2. **Material Consumption & Tool Fabrication**:
   - Consumes `station.massPerTinker` of refined metal or phosphorite from internal storage.
   - Spawns the tool item at `transform.GetPosition() + Vector3.up` with matched temperature.
   - Plays production animation and updates optional fabrication progress bar.
3. **Precondition Chore Suppression**:
   - Uses `StationChoreSuppressionPatches` to return `false` on chore availability, preventing Duplicants from claiming the station while avoiding `NullReferenceException` in `SetupChore`.

---

### Mission Control Station (Rockets)
- **Prefabs**: `MissionControl`, `MissionControlCluster`
- **Controller Class**: [`AutoMissionControlController`](https://github.com/Eurekalo/Automatic_Industry/blob/main/AutomaticIndustry-src-2.4.0/AutomaticIndustry-src-2.4.0/src/Components/AutoMissionControlController.cs)

#### Code Function Logic:
1. **Assertion-Safe Architecture**:
   - Vanilla `MissionControlWorkable` asserts on `TargetSpacecraft` and crashes if ticked without an active Duplicant worker.
   - `AutoMissionControlController` **never touches the Workable**.
2. **State Machine Query & Buff Application**:
   - Queries `smi.sm.WorkableRocketsAreInRange.Get(smi)`.
   - If a boostable rocket is orbiting or in range:
     - Waits the vanilla duration (90 seconds).
     - Calls `planetary.ApplyEffect(craft)` or `cluster.ApplyEffect(clustercraft)`, applying the 10-minute speed boost.

---

### Geotuner & Biobot Builder
- **Prefabs**: `GeoTuner`, `MorbRoverMaker`
- **Controllers**: [`AutoGeoTuner`](https://github.com/Eurekalo/Automatic_Industry/blob/main/AutomaticIndustry-src-2.4.0/AutomaticIndustry-src-2.4.0/src/Components/AutoGeoTuner.cs) & [`AutoMorbRoverMaker`](https://github.com/Eurekalo/Automatic_Industry/blob/main/AutomaticIndustry-src-2.4.0/AutomaticIndustry-src-2.4.0/src/Components/AutoMorbRoverMaker.cs)

#### Code Function Logic:
- **Geotuner**: Automates scientist geyser study and amplification interaction; displays tuning progress and geyser linkage on building meters.
  - **Audio & Static Constructor Safety (v2.4.39)**: Features dynamic sound path auto-healing and defensive event prefix via `GeoTunerSoundSafetyPatch` to completely eliminate vanilla/FMOD null pointer Black Hole crashes when tuning geysers.
  - **Delivery Fetch Stability & Priority Persistence (v2.4.40)**: Strictly guards `ManualDeliveryKG` tags and capacities with inequality checks to prevent fetch chore cancellation loops and priority flickering on 200ms simulation cadence.
- **Biobot Builder**: Consumes steel and zombie spore biomass automatically to construct Morb Rovers.

---

## 6. Research & Science Category

### Basic & Advanced Research Centers
- **Prefabs**: `ResearchCenter`, `AdvancedResearchCenter`, `CosmicResearchCenter`, `DLC1CosmicResearchCenter`
- **Controller Class**: [`AutoResearchController`](https://github.com/Eurekalo/Automatic_Industry/blob/main/AutomaticIndustry-src-2.4.0/AutomaticIndustry-src-2.4.0/src/Components/AutoResearchController.cs)

#### Code Function Logic:
1. **Dual Delivery Injection (Advanced Research Center)**:
   - Vanilla flags bottled water deliveries as `ResearchFetch`, which Auto-Sweepers cannot perform.
   - `BuildingPrefabInjection.InjectResearchDelivery` attaches a secondary `ManualDeliveryKG` configured with `MachineFetch`.
   - **Result**: Duplicants can still deliver water, but Auto-Sweepers can also deliver bottled water directly from reservoirs or bottle fillers!
2. **Point Generation (`Step(float dt)`)**:
   - Reads active tech project from `Research.Instance.GetActiveResearch()`.
   - Consumes dirt/water/data banks from storage.
   - Calls `Research.Instance.AddResearchPoints(researchTypeId, points)`.
   - Spawns research floating FX popups and advances research progress bar.

---

### Materials Study Terminal (Nuclear Research)
- **Prefab**: `NuclearResearchCenter`
- **Controller Class**: [`AutoNuclearResearchCenterController`](https://github.com/Eurekalo/Automatic_Industry/blob/main/AutomaticIndustry-src-2.4.0/AutomaticIndustry-src-2.4.0/src/Components/AutoNuclearResearchCenterController.cs)

#### Code Function Logic:
1. **Radbolt Tracking**:
   - Monitors `HighEnergyParticleStorage.Particles`.
   - Verifies whether current active research requires nuclear research points.
2. **Particle Consumption & Progress**:
   - Consumes radbolts at `materialPerPoint` (10 particles/point).
   - Generates nuclear research points and displays `sprite_Research` popup FX.
   - Suppresses Duplicant researcher errands.

---

### Botanical Analyzer (Genetic Analysis Station)
- **Prefab**: `GeneticAnalysisStation`
- **Controller Class**: [`AutoGeneticAnalysisStationController`](https://github.com/Eurekalo/Automatic_Industry/blob/main/AutomaticIndustry-src-2.4.0/AutomaticIndustry-src-2.4.0/src/Components/AutoGeneticAnalysisStationController.cs)

#### Code Function Logic:
1. **Seed Inspection**:
   - Confirms an unidentified seed (`GameTags.UnidentifiedSeed`) with `MutantPlant` component is present in storage.
2. **Analysis Progression**:
   - Ticks `workable.WorkTimeRemaining` until zero.
   - Invokes `workable.CompleteWork(null)` cleanly inside a `SafeInvoke` block, discovering the plant's genetic traits.

---

### Telescopes (Planetary & Enclosed)
- **Prefabs**: `Telescope`, `ClusterTelescope`, `ClusterTelescopeEnclosed`
- **Controller Class**: `AutoTelescopeController`

#### Code Function Logic:
- Ticks celestial scanning progress as long as line-of-sight to space is unobstructed and oxygen/power requirements are fulfilled.

---

## 7. Utilities Category

### Oil Well Cap
- **Prefab**: `OilWellCap`
- **Controller Class**: [`AutoOilWellCap`](https://github.com/Eurekalo/Automatic_Industry/blob/main/AutomaticIndustry-src-2.4.0/AutomaticIndustry-src-2.4.0/src/Components/AutoOilWellCap.cs)
- **Interface**: `ISim1000ms`

#### Code Function Logic:
1. **Pressure Monitoring**:
   - Queries `smi.GetPressurePercent()`.
   - Compares with player slider threshold (`wellCap.GetSliderValue(0)`).
2. **Depressurization Cycle**:
   - When pressure >= threshold:
     - Sets `smi.sm.working.Set(true, smi)`.
     - Vents natural gas until pressure drops to 0%.
     - Suppresses manual Duplicant release errands.
3. **Meters & Status**:
   - Renders Backpressure progress bar at the base of the building scaled to the threshold.
   - Displays a countdown timer in the Status item showing seconds remaining until venting.

---

### Ice-E Fan (Ice Cooled Fan)
- **Prefab**: `IceCooledFan`
- **Controller Class**: [`AutoIceCooledFanController`](https://github.com/Eurekalo/Automatic_Industry/blob/main/AutomaticIndustry-src-2.4.0/AutomaticIndustry-src-2.4.0/src/Components/AutoIceCooledFanController.cs)

#### Code Function Logic:
1. **Cooling Step**:
   - Calls `fan.DoCooling(dt)` and consumes internal ice mass.
2. **Freeze Protection Waiver**:
   - Honors vanilla 5°C ambient shutoff to prevent freezing, or cools continuously if "Ignore Too Cold" is enabled.

---

## 8. Auto-Sweeper Crop Harvesting

### Universal Plant Harvesting Engine
- **Target**: `SolidTransferArm`
- **Controller Class**: [`AutoSweeperHarvestController`](https://github.com/Eurekalo/Automatic_Industry/blob/main/AutomaticIndustry-src-2.4.0/AutomaticIndustry-src-2.4.0/src/Components/AutoSweeperHarvestController.cs)

#### Code Function Logic:
1. **Dual-Cell Reachability Check**:
   - Scans plants in arm range.
   - Verifies reachability for **both the plant coordinate cell and the planter box / hydroponic tile foundation cell**.
   - Dynamically adapts to custom range boundaries from **Zoned Solid Transfer Arm** and **Adjustable Transfer Arm** mods.
2. **Automated Harvesting**:
   - When plant maturity reaches 100%, triggers crop harvest without Duplicant farmer errand.
   - Drops food and seeds directly into the sweep area for Conveyor Loader delivery.
3. **Robotic Worker Safety & Animation Override Guard**:
   - Under third-party delivery mods (e.g. *No Manual Delivery*), Auto-Sweepers execute item pickup and transfer errands.
   - Vanilla `StandardWorker.AttachOverrideAnims` attempts to bind Duplicant multi-tool animation symbols to the sweeper's `KAnimControllerBase`. Because sweepers lack a `SymbolOverrideController`, this throws an engine assert crash:
     ```
     Assert failed: Anim overrides containing additional symbols require a symbol override controller.
     ```
   - **`StandardWorkerAttachOverrideAnimsPatch`** suppresses attaching override animations whenever `worker.UsesMultiTool() == false` or the worker lacks `SymbolOverrideController`.
   - **`BuildingPrefabInjection`** attaches `SymbolOverrideController` to `SolidTransferArm` completed prefabs on spawn, guaranteeing dual-layer stability across all automated logistics mods.

