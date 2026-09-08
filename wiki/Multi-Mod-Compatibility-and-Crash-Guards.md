# Multi-Mod Ecosystem Compatibility & Crash Guard Systems

**Automatic Industry (Auto Machine Rebuilt)** is engineered to operate seamlessly within heavily modded *Oxygen Not Included* colonies (100+ active Workshop mods). 

This technical reference details the dedicated compatibility layers, runtime shims, defensive exception containments, and engine-level crash guards built into the mod.

---

## 📑 Index of Supported Mod Integrations

1. [No Manual Delivery (2047308624)](#1-no-manual-delivery-steam-id-2047308624)
2. [PLib UI Layout & Mod Options Dialog](#2-plib-ui-layout--options-dialog-resilience)
3. [Customize Buildings (1818138009)](#3-customize-buildings-steam-id-1818138009)
4. [I_实用系统 (3300147615)](#4-i_实用系统-practical-systems-steam-id-3300147615)
5. [Multithreaded Simulation (SimDLL_Rust / 3779276157)](#5-multithreaded-simulation-simdll_rust)
6. [EmptyStorage (1748202748)](#6-emptystorage-steam-id-1748202748)
7. [Adjustable Transfer Arm & Zoned Solid Transfer Arm](#7-adjustable-transfer-arm--zoned-solid-transfer-arm)
8. [Mod Menu (v1.4.9) & In-Game Pause Screen](#8-mod-menu-v149--in-game-pause-screen-integration)
9. [FastTrack Engine Optimization](#9-fasttrack-engine-optimization)
10. [ONI Together (Multiplayer)](#10-oni-together-multiplayer)

---

## 1. No Manual Delivery (Steam ID 2047308624)

### Challenges:
*No Manual Delivery* dynamically disables Duplicant deliveries to designated storage containers and fabricators, redirecting chores exclusively to Auto-Sweepers (`SolidTransferArm`). This introduces two critical edge cases:
1. **Uninitialized Prober on Save Game Load**: In early scene initialization or when Duplicant hold mode is inactive, `TransferArmGroupProber.Get()` returns `null`. Downstream pathing calls throw `NullReferenceException` during chore generation.
2. **Animation Override Assertion Crash on Robotic Pickups**: When `No Manual Delivery` assigns fetch chores to `SolidTransferArm`, the robotic arm executes `StandardWorker.AttachOverrideAnims(worker_controller)`. Because `SolidTransferArm` lacks a `SymbolOverrideController`, Unity's `KAnimControllerBase` throws an assertion failure:
   ```
   Assert failed: Anim overrides containing additional symbols require a symbol override controller.
   ```

### Engineering Solutions:
- **`NoManualDeliveryCompatibility.cs`**:
  - Intercepts `TransferArmGroupProber.Get()` via a Harmony postfix:
  ```csharp
  if (__result == null)
  {
      __result = MinionGroupProber.Get();
      if (__result == null)
      {
          __result = GetFallbackProber();
      }
  }
  ```
  - Provides a dual-tiered non-null prober guarantee, completely preventing `NullReferenceException` on save load.
- **`StandardWorkerAttachOverrideAnimsPatch`**:
  - Intercepts `StandardWorker.AttachOverrideAnims(KAnimControllerBase worker_controller)`:
  ```csharp
  if (!worker.UsesMultiTool() || worker_controller == null || worker_controller.GetComponent<SymbolOverrideController>() == null)
  {
      return false; // Skip attaching duplicant override animations on robotic arms
  }
  ```
- **Proactive Symbol Controller Injection**:
  - In `BuildingPrefabInjection.cs`, `SolidTransferArm` completed building prefabs automatically receive a `SymbolOverrideController` if absent, providing defense-in-depth across all third-party fetch mods.

---

## 2. PLib UI Layout & Options Dialog Resilience

### Challenge:
When customizing mod options, bilingual descriptions and localized strings can push checkboxes, sliders, and color preview pickers outside the dialog's visible rect. Furthermore, upstream PLib version differences can alter method parameters on `OptionsDialog.AddModInfoScreen(PDialog dialog)`.

### Engineering Solution:
- **`OptionsDialogLayoutFixPatch.cs`**:
  - Hooks `PeterHan.PLib.Options.OptionsDialog.AddModInfoScreen`:
  - Dynamically enforces minimum dialog dimensions (1020x720) with horizontal 2-column layout and preview margins.
  - Postfix parameter explicitly binds to `object dialog` (matching `PDialog dialog`).
  - Patch application is decoupled from Harmony auto-discovery and registered manually inside `SafeInvoke.Try` in `AutoMachineMod.OnLoad()`. Even if future PLib updates alter upstream internal signatures, the mod loads cleanly without aborting.

---

## 3. Customize Buildings (Steam ID 1818138009)

### Challenge:
*Customize Buildings* forcefully removes vanilla workable components (such as `OilRefinery`, `OilWellCap`, `IceCooledFan`) and alters recipe parameters at the `BuildingDef` level. This previously caused missing component exceptions (`[MyCmpReq]` crashes) and state machine deadlocks (e.g. `Compost` infinite recursive composting loop).

### Engineering Solution:
- **`CustomizeBuildingsCompatibility.cs`**:
  - Applies runtime Harmony prefix shims returning `false` to cancel destructive modifications:
    - Suppresses `OilRefineryConfig_ConfigureBuildingTemplate.Postfix` and `OilWellCapConfig_ConfigureBuildingTemplate.Postfix`.
    - Suppresses `Compost_States_Patch.Postfix` (preventing `inert.GoTo(composting)` loop).
    - Suppresses `Desalinator_Patch.Postfix`.
    - Suppresses `NoDupeHelper.SetAutomatic`.
  - Reflectively neutralizes conflicting state options (`NoDupeOilRefinery`, `NoDupeOilWellCap`, etc.) in `CustomizeBuildingsState.Instance`.
  - Upgraded component accessors from `[MyCmpReq]` to `[MyCmpGet]` with defensive null checks throughout simulation loops.

---

## 4. I_实用系统 (Practical Systems / Steam ID 3300147615)

### Mod Overview & Integration:
*I_实用系统* introduces specialized utility structures, high-throughput piping networks, and advanced automation logic gates. 

### Architecture Coexistence:
- **Zero Port Collisions**: Automatic Industry does not bind custom logic ports or override port definitions on vanilla structures. Buildings automated by Automatic Industry cleanly respect incoming green/red automation signals from *I_实用系统* logic gates.
- **Operational Event Synchronization**: State transitions in Automatic Industry hook into vanilla `GameHashes.OperationalChanged` and `Operational.SetActive()`, ensuring full state consistency across *I_实用系统* sensor networks.

---

## 5. Multithreaded Simulation (SimDLL_Rust)

### Architecture Coexistence:
*SimDLL_Rust* (Multithreaded Simulation) accelerates element state changes, thermal diffusion, and liquid/gas flow across secondary native threads.
- **Strict Main-Thread Cadence**: Automatic Industry controllers (`AutoWorkControllerBase`, `ISim200ms`, `ISim1000ms`) execute exclusively on the Unity engine main thread.
- **Safe Element Querying**: Recipe ingredient validation and output spawning interface with the standard `Storage` and `ElementConsumer` APIs without touching asynchronous Rust worker threads, guaranteeing thread-safe, race-free operation.

---

## 6. EmptyStorage (Steam ID 1748202748)

### Integration:
*EmptyStorage* allows players to manually eject stored resources from buildings.
- **`VanillaEmptyPaths.cs`**:
  - Identifies items scheduled for player-requested ejection.
  - Automatically exempts these items from automated batch recycling, preventing infinite pickup-drop loops and protecting item entity IDs.

---

## 7. Adjustable Transfer Arm & Zoned Solid Transfer Arm

### Integration:
*Adjustable Transfer Arm* and *Zoned Solid Transfer Arm* extend the reach radius and assign custom zone filters to Auto-Sweepers (`SolidTransferArm`).
- **`AutoSweeperHarvestController.cs`**:
  - Dynamically inspects the active `SolidTransferArm` bounding volume rather than hardcoding vanilla's 4-cell radius.
  - Queries `ZonedArm` component boundaries and pick filters, allowing automated crop harvesting across user-defined zones and through pneumatic door setups.

---

## 8. Mod Menu (v1.4.9) & In-Game Pause Screen Integration

### Features:
*Mod Menu* allows inspecting active mods and editing live options directly during gameplay.
- **Language-Agnostic "Options" Locator**:
  - Locates the game's "Options" button via callback delegate reflection (`bi.onClick.Method.Name == "OnOptions"`), ONI localized string constants (`STRINGS.UI.FRONTEND.PAUSE_SCREEN.OPTIONS`), and multilingual keyword matching ("选项", "選項", "OPTION", "設定", "설정", "НАСТРОЙК", "EINSTELLUNG").
  - Inserts the "Mod Menu" button directly below "Options" (`siblingIndex = optionsIndex + 1`), above "Colony Summary".
  - Failsafe guards ensure the button is never appended to the bottom of the pause screen below "Quit to Desktop".
- **Dynamic Attribution & Tag Shielding**:
  - In `UserMenuModAttributionPatch.cs`, when Mod Menu is active, Automatic Industry yields button tag rendering to Mod Menu to prevent duplicate `[Mod: Automatic Industry]` badges.

---

## 9. FastTrack Engine Optimization

### Integration:
*FastTrack* performs deep caching of Unity GameObjects and skips redundant component queries.
- Controllers cache component references during `Prepare()` (`OnPrefabInit` / `OnSpawn`).
- Avoids reflective `GetComponent` and `Find` invocations in 200ms simulation loops, maintaining 60+ FPS in late-game colonies.

---

## 10. ONI Together (Multiplayer)

### Integration:
- Automated state transitions generate standard game events (`Trigger`, `Operational.SetActive`).
- Multiplayer packet synchronization via `BuildingAutomationSyncPacket` coordinates player automation overrides across client sessions without desync.
