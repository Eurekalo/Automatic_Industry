# Development guide

This document describes what the mod changes, which mechanisms it introduces,
and where each piece lives, so a third party can audit or extend it.

## Guiding rules

1. **Never relax a vanilla condition.** Automation only starts when the game
   itself considers the work doable (power, ingredients, storage space,
   cooldown, environment, room). The mod does not edit build costs, recipes or
   rates, except the explicitly optional Oil Refinery ratio.
2. **Fail soft.** Every reflective or patched call is wrapped in
   `SafeInvoke.Try` (`src/Util/SafeInvoke.cs`). A failure logs once and disables
   the feature for that building instead of throwing into the simulation.
3. **No save data.** No custom save fields, no new prefabs. Removing the mod
   leaves existing saves loadable.
4. **DLC-safe.** DLC-only types are resolved by name at runtime; missing types
   are skipped silently. Each patch is applied inside its own try/catch.

## Layout

```
src/
  AutoMachineMod.cs              entry point: Harmony, PLib, string registration
  Automation/
    AutomationEntry.cs           metadata for one automation target
    AutomationRegistry.cs        table: config type name -> option key / flags
    ChoreIndex.cs                GameObject -> active chore index
  Components/
    AutoWorkDriver.cs            generic vanilla-Workable driver
    AutoOilRefinery.cs           runs while the state machine reports ready
    AutoOilWellCap.cs            releases pressure at the vanilla threshold
    AutoFoodSmoker.cs            drops finished products
    AutoGeoTuner.cs              performs the tuning interaction
    AutoRanchStation.cs          grooming / shearing / milking, land + aquatic
  Config/
    AutoMachineOptions.cs        PLib options, one property per building
    OilRefineryEfficiency.cs     vanilla 50% vs legacy 100% enum
    UiLanguage.cs                options language enum
  Localization/
    ModStrings.cs                LocString keys, registered in OnLoad
    Translations.cs              en / zh-CN / zh-TW / ko / ja tables
    OptionTextBinder.cs          bilingual label composition
    BuildingNameBinder.cs        reads building names from game STRINGS
  Patches/
    ComplexFabricatorPatches.cs  duplicantOperated = false registry
    ComponentInjectionPatches.cs attaches the specialised components
    AutoWorkInjectionPatches.cs  soft, by-name patching of config classes
    OilRefineryEfficiencyPatch.cs adjusts the ElementConverter outputs
  Util/
    Log.cs, SafeInvoke.cs, WorkAnim.cs
```

## Mechanisms

### Chore-gated automation

`ChoreIndex` patches `ChoreProvider.AddChore` / `RemoveChore` (and the global
provider) and stores chores keyed by the owning `GameObject`. `AutoWorkDriver`
polls on `Sim200ms`; when its building has a matching, non-complete work chore
and the option is enabled, it starts a cycle. This is the safest available
signal that all vanilla preconditions hold, because the game only emits the
chore when they do.

### Driving the vanilla Workable

A cycle calls `OnStartWork`, then `WorkTick(null, dt)` each poll until the
vanilla `workTime` elapses, then `CompleteWork(null)`. The worker argument is
`null`, so every callback is guarded; if a building's callbacks require a real
worker, the exception is caught, the driver falls back to a timer that only
invokes the completion callback, and the building is marked as degraded.

### Animation

`WorkAnim` picks the first existing animation among `working_loop`, `work_loop`,
`working`, `on`, and plays it on the building's `KBatchedAnimController` for the
duration of the cycle, restoring `idle` / `off` afterwards.

### Room and skill overrides

Room-bound buildings keep their vanilla `RoomTracker.requirement`. When the
matching **Ignore room requirement** option is on, the driver temporarily sets
the requirement to `Recommended`, which is the same value the game uses for
soft suggestions; nothing in the room system itself is modified. Skill gates
(Biobot Builder) are only bypassed when their dedicated option is enabled.

### Oil Refinery ratio

`OilRefineryEfficiencyPatch` runs in `DoPostConfigureComplete` and rewrites the
`ElementConverter` output amounts to either the vanilla 50% ratio (default) or
the legacy 100% ratio.

## Adding a building

1. Confirm the config type name and whether the work is a `Workable`.
2. Add a property to `AutoMachineOptions` plus its strings in `ModStrings` and
   `Translations`.
3. Add a row to `AutomationRegistry`.
4. If the building needs bespoke completion logic, add a component under
   `src/Components/` instead of using `AutoWorkDriver`.

## Building

```bash
ONI_MANAGED="/path/to/OxygenNotIncluded_Data/Managed" \
PLIB="/path/to/PLib.dll" ./build.sh
```

Output lands in `dist/` together with `mod.yaml`, `mod_info.yaml`, `LICENSE`
and `PLib.dll`.

## 2.2.0 — Ranching rewrite, Farm Station, Mission Control, live options

- `AutoRanchStation` was rewritten on top of the vanilla `RanchStation.Instance`
  state machine. It invites a critter through the vanilla ranchable monitor,
  waits for the critter to arrive and play its ranched animation, drives the
  work timer itself and then triggers the vanilla completion path. Shearing and
  milking reuse the same choreography, so the critter animations match vanilla.
- `RanchCompletionGuard` wraps the vanilla completion delegates at prefab load
  time so a `null` rancher can never dereference inside Klei code.
- `AutoTinkerStationController` reproduces the Farm Station / Spice Grinder
  production step instead of ticking the workable, and honours the new
  "produce without demand" option.
- `AutoMissionControlController` applies the rocket boost effect directly; the
  crash prone workable is never ticked.
- Circuit breakers now recover: a building that was switched off retries after
  60 seconds, up to five times, before falling back to vanilla behaviour
  permanently.
- Options implement `IOptions.OnOptionsChanged`, so language, bilingual labels
  and every per building toggle apply immediately. Only the Oil Refinery ratio
  still needs a reload, because it rewrites the recipe of the prefab.
