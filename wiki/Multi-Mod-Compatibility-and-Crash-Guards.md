# Multi-Mod Ecosystem Compatibility & Crash Guard Systems

**Automatic Industry (Auto Machine Rebuilt)** is engineered to operate seamlessly within heavily modded *Oxygen Not Included* colonies (100+ active Workshop mods). 

This technical reference details the dedicated compatibility layers, runtime shims, defensive exception containments, and engine-level crash guards built into the mod.

---

## 🛡️ Compatibility Matrix at a Glance

| Mod | Steam ID | Challenge / Crash Risk | Automatic Industry Solution | Status |
| :--- | :---: | :--- | :--- | :---: |
| **No Manual Delivery** | `2047308624` | Null prober crash on load; robotic arm multi-tool anim assert. | Dual-tier fallback prober + `StandardWorkerAttachOverrideAnimsPatch`. | 🛡️ Active Shim |
| **PLib Mod Options** | — | Layout collapse; method signature differences (`dialog` vs `optionsDialog`). | `OptionsDialogLayoutFixPatch` enforces 1020x720 layout via defensive reflection. | 🛡️ Active Shim |
| **Customize Buildings** | `1818138009` | Strips vanilla components; infinite composting state recursion. | Dynamic prefix shims return `false` on conflicting patches; `[MyCmpGet]` safety. | 🛡️ Active Shim |
| **I_实用系统** | `3300147615` | Potential logic port collisions and state synchronization errors. | Clean `GameHashes.OperationalChanged` hooks; zero custom port overrides. | 🟢 Coexistence |
| **Multithreaded Sim** | `3779276157` | Race conditions against Rust native worker threads. | Automation loops execute strictly on Unity main thread (`ISim200ms`, `ISim1000ms`). | 🟢 Fully Compatible (v59-744825-RC-3.1) |
| **EmptyStorage** | `1748202748` | Dropped items re-picked in infinite loops; entity ID loss. | `VanillaEmptyPaths` exempts player-ejected items from auto-recycling. | 🛡️ Active Shim |
| **Adjustable / Zoned Arm** | `3745253371` | Hardcoded 4-cell range limits sweep and crop harvesting. | `AutoSweeperHarvestController` dynamically queries custom cell zones. | 🟢 Integrated |
| **Mod Menu (v1.4.13)** | `3789353358` | UI occlusion; button position misalignment across languages. | Multilingual Options locator + canvas `sortingOrder = 350` layering. | 🎨 Full UI Sync |
| **FastTrack** | — | High-frequency reflective overhead and ReportManager note inflation. | Full component caching in `Prepare()`; dynamic precondition chore suppression. | 🟢 Fully Compatible (Beta 0.18.6.0) |
| **ONI Together** | — | State desynchronization across multiplayer clients. | State changes broadcast via standard game events and sync packets. | 🟢 Synced |
| **Ronivan's Legacy Suite** | `3557584850` | Missing UI canvas on `BuildingEditor`; `usingNewSymbolOverrideSystem` assert. | Safe UI parenting resolver + `SymbolOverrideController` auto-healing. | 🛡️ Active Shim |
| **GeoTuner Audio Safety** | — | Early mod reflection touches `GeoTuner`, freezing sound paths to `null`. | Dynamic sound path auto-healing + prefix null-path muting. | 🛡️ Active Shim |
| **Long-Cycle Memory Safety** | — | High-frequency chore cancellation inflates `ReportManager` by gigabytes in cycle 200–300 saves. | Dynamic precondition suppression + native ProgressBar reuse + zero-alloc 5 Hz loops. | 🛡️ Active Guard |

---

## 1. No Manual Delivery (Steam ID 2047308624)

```mermaid
sequenceDiagram
    autonumber
    participant SaveLoad as SaveGame Loader
    participant NMD as NoManualDelivery Prober
    participant AI as NoManualDeliveryCompatibility
    participant Worker as StandardWorker
    participant Patch as StandardWorkerAttachOverrideAnimsPatch

    Note over SaveLoad: World Generation / Scene Load
    SaveLoad->>NMD: TransferArmGroupProber.Get()
    alt Prober Uninitialized
        NMD-->>AI: Returns null
        AI->>AI: Fallback 1: MinionGroupProber.Get()
        AI->>AI: Fallback 2: GetFallbackProber()
        AI-->>SaveLoad: Returns Guaranteed Non-Null Prober (Zero Crashes!)
    end

    Note over Worker: Auto-Sweeper Picks Up Item
    Worker->>Patch: AttachOverrideAnims(worker_controller)
    alt Worker is Robotic Arm (SolidTransferArm)
        Patch->>Patch: Check worker.UsesMultiTool() == false
        Patch-->>Worker: Return false (Skip Attaching Multi-Tool Symbols)
        Note over Worker: Bypasses symbol override controller assert crash
    end
```

### Engineering Solutions:
1. **Dual-Tiered Fallback Prober**:
   - `NoManualDeliveryCompatibility.cs` intercepts `TransferArmGroupProber.Get()`. If `null`, it substitutes `MinionGroupProber.Get()` or an internal fallback prober, completely eliminating `NullReferenceException` during early scene loading.
2. **Robotic Multi-Tool Animation Guard**:
   - `StandardWorkerAttachOverrideAnimsPatch` checks `worker.UsesMultiTool()`. If false or if the worker lacks `SymbolOverrideController`, it aborts attaching Duplicant tool animations, eliminating the engine assertion crash.
3. **Proactive Symbol Controller Injection**:
   - `BuildingPrefabInjection` automatically attaches a `SymbolOverrideController` to `SolidTransferArm` prefabs on spawn for defense-in-depth.

---

## 2. PLib UI Layout & Options Dialog Resilience

### Challenge:
Bilingual descriptions and long localization strings can push checkboxes, sliders, and color preview pickers outside the visible rect in default PLib dialog layouts. Upstream PLib updates also alter method signatures.

### Engineering Solution:
- **`OptionsDialogLayoutFixPatch.cs`**:
  - Dynamically enforces minimum dialog dimensions (1020x720) with horizontal 2-column layout and preview margins.
  - Patch hooks parameter types safely (`object dialog`), decoupled from Harmony auto-discovery and registered inside `SafeInvoke.Try` in `AutoMachineMod.OnLoad()`.

---

## 3. Customize Buildings (Steam ID 1818138009)

```mermaid
flowchart TD
    A["Customize Buildings Patches BuildingDef"] --> B{"Destructive Modifications"}
    B -->|Removes Workable| C["Auto Machine Harmony Prefix Interception"]
    B -->|Inert to Composting Loop| C
    C --> D["Return false: Neutralize Destructive Patches"]
    C --> E["Fallback to [MyCmpGet] Safe Component Architecture"]
    D --> F
    E --> F
    F --> G["Automatic Industry Controllers Run Safely"]

    style A fill:#742a2a,stroke:#e53e3e,color:#fff
    style C fill:#2b6cb0,stroke:#1a365d,color:#fff
    style F fill:#22543d,stroke:#38a169,color:#fff
```

### Engineering Solution:
- Applies runtime Harmony prefix shims that return `false` to cancel destructive modifications.
- Suppresses `Compost_States_Patch.Postfix` to prevent infinite composting loops.
- Upgrades component accessors from `[MyCmpReq]` to `[MyCmpGet]` with defensive null checks throughout simulation loops.

---

## 4. Mod Menu (v1.4.13) & In-Game Screen Integration

```mermaid
flowchart TD
    A["Pause Screen Opened"] --> B["Mod Menu Locates Options Button"]
    B --> C{"Multilingual Matching"}
    C -->|Delegate Reflection| D["bi.onClick.Method.Name == 'OnOptions'"]
    C -->|Localized Constants| E["STRINGS.UI.FRONTEND.PAUSE_SCREEN.OPTIONS"]
    C -->|Multilingual Keywords| F["Options in All Languages"]
    
    D --> G["Insert Mod Menu Button at optionsIndex + 1"]
    E --> G
    F --> G
    
    G --> H["Open Configuration Screen"]
    H --> I["Canvas sortingOrder = 350: Render Above All Dialogs"]

    style A fill:#2d3748,stroke:#4a5568,color:#fff
    style C fill:#1a365d,stroke:#2b6cb0,color:#fff
    style G fill:#22543d,stroke:#38a169,color:#fff
    style I fill:#44337a,stroke:#805ad5,color:#fff
```

### Engineering Solutions:
1. **Multilingual Options Button Locator**:
   - Locates the pause screen's "Options" button via delegate reflection and multilingual strings.
   - Places the "Mod Menu" button directly below "Options", never appending it below "Quit to Desktop".
2. **Canvas Sorting Order (`sortingOrder = 350`)**:
   - Ensures child configuration dialogs render above both the Pause Menu and ModMenu windows without raycast blocking or visual clipping.

---

## 5. Chemical Processing & BuildingEditor Safe UI Parenting

### Challenge:
Ronivan's *Chemical Processing* includes a `BuildingEditor` tool that calls `ShowWindow()`. In active gameplay or pause states, `FrontEndManager.Instance` can be `null`, crashing the game when attempting to attach to the front-end canvas.

### Engineering Solution:
- **`ChemicalProcessingCompatibility.cs`**:
  - Dynamically intercepts `BuildingEditor.ShowWindow()` with a safe UI parenting resolver.
  - Automatically redirects parenting to `GameScreenManager.Instance.ssOverlayCanvas` or `GetTargetWidget()` when `FrontEndManager.Instance` is unavailable.

---

## 6. SymbolOverrideController Deserialization Auto-Healing

```mermaid
flowchart TD
    A["SaveLoadRoot.Load / Util.KInstantiate"] --> B["Instantiate Building / Entity"]
    B --> C["GameObject.SetActive: True"]
    C --> D["SymbolOverrideController.OnPrefabInit"]
    D --> E{"usingNewSymbolOverrideSystem == true?"}
    E -->|No: False by Default in Vanilla| F["SymbolOverrideControllerCompatibility Prefix"]
    F --> G["Auto-Heal: set usingNewSymbolOverrideSystem = true"]
    G --> H["Safe Initialization: Zero Assertion Crashes"]
    E -->|Yes| H

    style A fill:#2d3748,stroke:#4a5568,color:#fff
    style E fill:#742a2a,stroke:#e53e3e,color:#fff
    style G fill:#22543d,stroke:#38a169,color:#fff
    style H fill:#1a365d,stroke:#2b6cb0,color:#fff
```

### Engineering Solution:
- Pre-emptively inspects the associated `KBatchedAnimController`. If `usingNewSymbolOverrideSystem` is `false`, auto-heals it to `true` before vanilla's assert evaluates, ensuring 100% crash-free save loading for Ronivan's industrial suite.

---

## 7. GeoTuner Audio Path Auto-Healing (Black Hole Fix)

```mermaid
flowchart TD
    A["Early Mod Loading: Class Scans Touch GeoTuner"] --> B["Static Constructors Run Before Sound Assets Loaded"]
    B --> C["liquidGeyserTuningSoundPath Frozen to NULL"]
    
    subgraph SubVanilla ["Vanilla Behavior: CRASH"]
        C --> D["Geyser Tuned In-Game"]
        D --> E["SoundEvent.PlayOneShot: null"]
        E --> F["FMOD PathToGUID: NullReferenceException"]
        F --> G["Black Hole Error Screen / Game Crash"]
    end

    subgraph SubHealed ["Automatic Industry Solution: HEALED"]
        C --> H["GeoTunerSoundSafetyPatch.EnsureSoundPathsPopulated"]
        H --> I["Re-query GlobalAssets.GetSound When Audio Assets Ready"]
        I --> J["Prefix Guard: Check soundPath != null"]
        J --> K["Smooth Audio Playback / Safe Mute: 100% Stable"]
    end

    style A fill:#2d3748,stroke:#4a5568,color:#fff
    style G fill:#742a2a,stroke:#e53e3e,color:#fff
    style H fill:#1a365d,stroke:#2b6cb0,color:#fff
    style K fill:#22543d,stroke:#38a169,color:#fff
```

### Engineering Solution:
- **`GeoTunerSoundSafetyPatch.cs`**:
  - **Auto-Healing**: Re-resolves static sound paths against `GlobalAssets.GetSound()` once audio banks load.
  - **Defensive Harmony Prefix**: Verifies `!string.IsNullOrEmpty(soundPath)` prior to triggering playback, safely muting if audio is unavailable.

---

## 8. Chore Suppression & Long-Cycle Memory Leak Elimination

```mermaid
flowchart TD
    A["Building Driven by Automation: 5 Hz Evaluation Cadence"] --> B{"Chore Handling Strategy"}

    subgraph SubLegacy ["Legacy Mechanism: Severe Memory Explosion"]
        B -->|Every 0.2s| C["Chore.Cancel() Polling"]
        C --> D["State Machine Exit Callback Aborts Chore"]
        D --> E["State Machine Immediately Re-enters Ready State"]
        E --> F["New Chore Created Instantly on Next Frame"]
        F --> G["ReportManager Logs Chore Creation & Cancellation"]
        G --> H["ReportEntry.notes & NoteStorage.StringTable Explode"]
        H --> I["Multi-Gigabyte Heap Bloat & Freeze on Save Load at Cycles 200-300"]
    end

    subgraph SubFixed ["Automatic Industry v2.5.0: Precondition Suppression"]
        B -->|Attached Once| J["ChoreSuppression.SuppressOperateChores"]
        J --> K["Attach DisabledByModPrecondition: Idempotent"]
        K --> L{"Automation Active?"}
        L -->|Yes| M["Precondition returns False: Duplicants Completely Ignore Chore"]
        L -->|No: Manual Mode| N["Precondition returns True: Duplicants Immediately Take Chore"]
        M --> O["Chore Stays Intact: Zero Cancellations, Zero Creations, Zero Report Notes"]
        O --> P["Flat Managed Heap: Zero Memory Growth Over Hundreds of Cycles"]
    end

    style A fill:#2d3748,stroke:#4a5568,color:#fff
    style C fill:#742a2a,stroke:#e53e3e,color:#fff
    style I fill:#742a2a,stroke:#e53e3e,color:#fff
    style J fill:#2b6cb0,stroke:#1a365d,color:#fff
    style P fill:#22543d,stroke:#38a169,color:#fff
```

### Challenges:
In long-running colonies (cycles 200–300+), enabling building automation previously caused rapid and unbounded memory growth (several gigabytes), especially when running refinery suites, food kitchens, ranching stations, and plant pulverizers (`MilkPress`).
1. **Re-entrant Chore Recreation Loop & Note Storage Explosion**:
   - In previous versions, `AutoWorkControllerBase` and `AutoOilRefinery` polled every 200 ms and called `Chore.Cancel()` on Duplicant operate chores to prevent Duplicants from competing with automation.
   - In ONI, when state machines define `.ToggleChore(...)` (`OilRefinery`, `RanchStation`, `Compost`, etc.), cancelling the chore immediately triggers the state machine exit callback, causing the state machine to re-enter the ready state and instantiate a new chore immediately on the next frame.
   - Crucially, ONI's `ReportManager.ReportValue()` logs every chore creation and cancellation as a `Note` object stored inside `ReportManager.reports`. Reports accumulate across hundreds of cycles without eviction and are serialized into the save file. At 5 Hz across dozens of machines, millions of `Note` objects accumulated, bloating heap memory into gigabytes.
2. **Duplicate Canvas ProgressBar Instantiation**:
   - `AutoFabricatorController` was creating a secondary `ProgressBar` instance via `ProgressBar.CreateProgressBar(gameObject, ...)` and deleting/recreating it whenever orders changed, causing Canvas UI churn and retaining graphic objects in memory.
3. **Idle Fabricator Queue Dirtiness Thrash**:
   - `ComplexFabricatorSim1000msPatch` was forcibly setting `SetQueueDirty()` every 1000ms on idle machines, forcing constant material needs re-computation.
4. **Critter Navigation Cost Eviction Thrash**:
   - In `AutoRanchStation`, checking `GetNavigationCost == -1` every 200 ms immediately evicted critters undergoing transient turn/jump animations, causing them to be continuously evicted and re-invited every tick.

### Engineering Solutions:
- **`ChoreSuppression.cs` & `StationChoreSuppressionPatches.cs` (Precondition Suppression)**:
  - Replaced destructive chore cancellations with dynamic preconditions: attaching `DisabledByModPrecondition` to candidate chores.
  - While automation is active, the precondition dynamically evaluates to `false`: Duplicants completely ignore the chore. The chore remains stably idle in memory with zero cancellations, zero re-entrant creations, and zero `ReportManager` note spam.
  - When the player switches the building to manual in-game, the precondition dynamically evaluates to `true`: Duplicants immediately take the chore without needing state machine restarts or reloads.
- **`AutoFabricatorController.cs` (Native ProgressBar Delegation)**:
  - Removed duplicate `ProgressBar` creation; delegates directly to native `ComplexFabricator.showProgressBar`.
- **`ComplexFabricatorPatches.cs`**:
  - Removed unconditional `SetQueueDirty()` on idle fabricators in `Sim1000ms`.
- **`AutoRanchStation.cs`**:
  - Replaced transient `-1` navigation evictions with a 10-second stuck watchdog; cached reflection parameter arrays.
- **Zero-Closure 5 Hz Automation Loop**:
  - Replaced delegate allocations in `AutoWorkControllerBase.Update()` and `AutoOilRefinery.Sim200ms()` with direct inlined execution, eliminating transient GC allocations at 5 Hz.
