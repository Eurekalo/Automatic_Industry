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
| **Multithreaded Sim** | `3779276157` | Race conditions against Rust native worker threads. | Automation loops execute strictly on Unity main thread (`ISim200ms`, `ISim1000ms`). | 🟢 Thread-Safe |
| **EmptyStorage** | `1748202748` | Dropped items re-picked in infinite loops; entity ID loss. | `VanillaEmptyPaths` exempts player-ejected items from auto-recycling. | 🛡️ Active Shim |
| **Adjustable / Zoned Arm** | `3745253371` | Hardcoded 4-cell range limits sweep and crop harvesting. | `AutoSweeperHarvestController` dynamically queries custom cell zones. | 🟢 Integrated |
| **Mod Menu (v1.4.13)** | `3789353358` | UI occlusion; button position misalignment across languages. | Multilingual Options locator + canvas `sortingOrder = 350` layering. | 🎨 Full UI Sync |
| **FastTrack** | — | High-frequency reflective overhead causing framerate dips. | Full component caching in `Prepare()`; zero reflection in 5Hz tick loops. | ⚡ Optimized |
| **ONI Together** | — | State desynchronization across multiplayer clients. | State changes broadcast via standard game events and sync packets. | 🟢 Synced |
| **Ronivan's Legacy Suite** | `3557584850` | Missing UI canvas on `BuildingEditor`; `usingNewSymbolOverrideSystem` assert. | Safe UI parenting resolver + `SymbolOverrideController` auto-healing. | 🛡️ Active Shim |
| **GeoTuner Audio Safety** | — | Early mod reflection touches `GeoTuner`, freezing sound paths to `null`. | Dynamic sound path auto-healing + prefix null-path muting. | 🛡️ Active Shim |

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
Bilingual descriptions and long localization strings can push options controls outside the visible dialog window. Upstream PLib parameter signatures also vary between `PDialog dialog` and `OptionsDialog optionsDialog`.

### Engineering Solution:
- **`OptionsDialogLayoutFixPatch.cs`**:
  - Dynamically enforces minimum dialog dimensions (1020x720) with horizontal two-column layout.
  - Postfix parameters bind cleanly to `object dialog`.
  - Registered via `SafeInvoke.Try` in `AutoMachineMod.OnLoad()` so future PLib updates load cleanly without crashing.

---

## 3. Customize Buildings (Steam ID 1818138009)

```mermaid
flowchart TD
    A["Customize Buildings Harmony Patches"] --> B{"Patches Conflicting Structure?"}
    B -->|Oil Refinery / Oil Well Cap| C["CustomizeBuildingsCompatibility: Return False"]
    B -->|Compost States Patch| D["Suppress inert.GoTo composting recursion"]
    B -->|Desalinator Patch| E["Suppress Destructive Component Removal"]
    
    C --> F["Preserve Vanilla Component Architecture"]
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
    C -->|Multilingual Keywords| F["Options / 設定 / 설정 / 选项"]
    
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
