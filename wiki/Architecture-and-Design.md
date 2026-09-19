# Architecture & Core Engineering Systems

This document details the architectural foundation, design patterns, and engineering systems that power **Automatic Industry (Auto Machine Rebuilt)**.

---

## 🏛️ 1. Core Engineering Principles

Automatic Industry is engineered from the ground up to guarantee maximum stability, mod compatibility, and game performance:

```mermaid
flowchart TD
    Root["<b>Core Engineering Principles</b>"] --> P1["<b>Passive Component Injection</b><br/>• No brittle Harmony IL transpilers<br/>• Pure Unity KMonoBehaviour components<br/>• Attached to completed prefabs"]
    Root --> P2["<b>Zero Save-Data Footprint</b><br/>• No custom serialized fields in .sav<br/>• No custom entity prefabs<br/>• Safe to install/remove mid-game"]
    Root --> P3["<b>Duplicant Priority</b><br/>• Duplicant always wins<br/>• Instant yield on manual command<br/>• Clean resumption after duplicant finishes"]
    Root --> P4["<b>Circuit Breakers & SafeInvoke</b><br/>• All controller ticks try-catch guarded<br/>• 5Hz evaluation rate (0.2s cadence)<br/>• Graceful degradation to manual on errors"]
    
    style Root fill:#1a365d,stroke:#2b6cb0,color:#fff
    style P1 fill:#2d3748,stroke:#4a5568,color:#fff
    style P2 fill:#22543d,stroke:#38a169,color:#fff
    style P3 fill:#7b341e,stroke:#dd6b20,color:#fff
    style P4 fill:#742a2a,stroke:#e53e3e,color:#fff
```

1. **Passive Component Injection over Brittle Transpilers**:
   - Rather than rewriting vanilla bytecode using Harmony IL transpilers (which frequently break with game updates or conflict with other mods), ~70% of the mod's logic is implemented as clean, standalone Unity `KMonoBehaviour` components attached directly to completed building prefabs.
2. **Zero Save-Data Footprint (100% Save Safe)**:
   - The mod serializes **no custom types, no custom MonoBehaviours, and no custom entity data** into the `.sav` file.
   - Per-building toggle preferences are stored in vanilla `KPrefabID` tags and clean colony-wide key-value dictionaries.
   - Players can safely add, update, or remove the mod at any time without corrupting save files or leaving orphaned data blocks.
3. **Duplicant Priority ("Duplicant Always Wins")**:
   - If a player commands a Duplicant to manually operate an automated station, or if a Duplicant begins working before automation engages, the automation controller immediately steps aside, yields control, and resumes only after the Duplicant finishes.
4. **Safe Exception Containment (`SafeInvoke` & Circuit Breakers)**:
   - All runtime evaluations are protected by circuit breakers. Transient game errors (e.g. critter despawning mid-groom, pipe bursting, or rocket launch) degrade gracefully to manual mode without crashing the game thread.

---

## 🔌 2. Prefab Injection Pipeline

### Why Postfix on `LoadGeneratedBuildings`?
Vanilla *Oxygen Not Included* loads buildings through individual `IBuildingConfig` implementations (e.g., `CookingStationConfig.cs`).

* **The Trap**: Patching `IBuildingConfig.CreateBuildingDef` or `DoPostConfigureComplete` directly forces the static constructor of the config class to execute during early mod loading. Several vanilla configs invoke `Db.Get()` inside their static constructors. If `Db.Get()` runs before the database is initialized, Unity throws a fatal `TypeInitializationException`. The cached exception breaks `BuildingConfigManager.RegisterBuilding` for all subsequent buildings and crashes ONI at the main menu.
* **The Solution**: Automatic Industry hooks into `GeneratedBuildings.LoadGeneratedBuildings` as a `[HarmonyPostfix]`. At this stage:
  1. All vanilla and modded building defs are fully constructed.
  2. The `Database` is completely initialized.
  3. DLC filters are already applied (missing DLC content simply doesn't exist in `Assets.BuildingDefs`, preventing missing asset errors).

### Injection Pipeline Flowchart

```mermaid
graph TD
    A["Game Startup: GeneratedBuildings.LoadGeneratedBuildings"] -->|Harmony Postfix| B["BuildingPrefabInjection.Postfix"]
    B --> C["Run Mod Compatibility Shims"]
    C --> D["Iterate Assets.BuildingDefs"]
    D --> E{"Check Building Mechanism"}
    
    E -->|ComplexFabricator + Workable| F["Attach AutoFabricatorController"]
    E -->|AutomationRegistry Match| G["Attach Specific Controller"]
    E -->|Specialized Prefab ID| H["Attach Custom Controller: OilRefinery, WellCap, Compost"]
    E -->|SolidTransferArm| I["Attach AutoSweeperHarvestController"]
    
    F --> J["Attach AutoBuildingCustomizer"]
    G --> J
    H --> J
    I --> J
    
    J --> K["Attach SymbolOverrideController if Missing"]
    K --> L["ColonyAutomationMasterRegistry on SaveGame"]

    style A fill:#2d3748,stroke:#4a5568,color:#fff
    style B fill:#1a365d,stroke:#2b6cb0,color:#fff
    style E fill:#44337a,stroke:#805ad5,color:#fff
    style J fill:#22543d,stroke:#38a169,color:#fff
```

### Constant Value Reflection Index
To map config classes to registry options without running static constructors, `BuildingPrefabInjection.BuildDriverKeyIndex()` reads the `ID` string literal via:
```csharp
FieldInfo idField = configType.GetField("ID", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
string value = idField.GetRawConstantValue() as string;
```
`GetRawConstantValue()` retrieves the string directly from the assembly's compiled metadata table without triggering class initialization!

---

## 🎛️ 3. Per-Building UserMenu Customizer (`AutoBuildingCustomizer`)

Every automated building receives the `AutoBuildingCustomizer` component, enabling players to configure automation individually for that specific building instance.

### Dual Toggle Modes & Shift+Click Shortcut

```mermaid
sequenceDiagram
    autonumber
    actor Player
    participant UI as Building Details Panel
    participant ABC as AutoBuildingCustomizer
    participant Workable as AutomationToggleWorkable
    participant Dupe as Duplicant Worker
    participant Controller as AutoWorkControllerBase

    alt Normal Left Click (Instant Toggle Enabled)
        Player->>UI: Left Click Automation Button
        UI->>ABC: ToggleMode()
        ABC->>Controller: Apply Enabled/Disabled State
    else Normal Left Click (Instant Toggle Disabled)
        Player->>UI: Left Click Automation Button
        UI->>ABC: QueueToggleErrand()
        ABC->>Workable: Create Chore (Building/Operating)
        Dupe->>Workable: Visit with Wrench & Play Animation
        Workable->>ABC: OnCompleteWork()
        ABC->>Controller: Apply Enabled/Disabled State
    else Shift + Left Click (Instant Override)
        Player->>UI: Shift + Left Click Automation Button
        UI->>ABC: ForceImmediateToggle()
        ABC->>Controller: Instantly Flip State (Bypasses Wrench Errand)
    end
```

### Priority Resolution Chain
When checking whether an automated building should run, the controller queries:
```csharp
AutoMachineOptions.IsEnabledFor(gameObject, optionKey)
```
The decision follows a strict hierarchy:
1. **Per-Building Instance Override**: If the player explicitly toggled this building instance (recorded in `AutoBuildingCustomizer`), that preference takes absolute precedence.
2. **Colony Master Registry**: If set via colony-wide batch tool, uses the colony registry.
3. **Global Mod Options**: Falls back to the global toggle in the mod options menu or Building Configuration Editor.

---

## 🚫 4. Chore Suppression System

When a building is operating automatically, Duplicants should not run across the map to perform unnecessary "Operate" errands. However, they **must still perform delivery, supply, and emptying chores**.

```mermaid
sequenceDiagram
    autonumber
    participant Engine as ONI Chore Solver
    participant Mod as ChoreSuppression System
    participant Dupe as Duplicant Brain
    participant Sweeper as Auto-Sweeper / Arm

    Note over Mod: AutoWorkControllerBase Tick (5Hz)
    Mod->>Engine: CancelOperateChores(gameObject)
    Note right of Mod: Filters & Cancels:<br>• ChoreTypes.Cook<br>• ChoreTypes.Fabricate<br>• ChoreTypes.Operate<br>• RanchStation.Instance
    
    Note over Engine: Logistics Errands Preserved
    Engine-->>Sweeper: Assign FabricateFetch / MachineFetch
    Engine-->>Dupe: Assign EmptyStorage / Supply
    
    Note over Dupe,Sweeper: Duplicants supply raw materials<br>Robotic arms fetch and store goods<br>Zero wasted Duplicant running time
```

### Station Precondition Suppression (`StationChoreSuppressionPatches`)
For complex stations (`PowerControlStation`, `FarmStation`):
- Many mods try to suppress errands by returning `null` from `CreateChore()`. In vanilla ONI, returning `null` causes downstream callers in `TinkerStation.SetupChore()` to crash with a `NullReferenceException`.
- Automatic Industry instead patches chore preconditions:
  ```csharp
  chore.AddPrecondition(ChorePreconditions.instance.IsNotAutomated, null);
  ```
  Returning `false` for the precondition cleanly instructs the Duplicant chore solver that the errand is currently unavailable without destabilizing the state machine.

---

## ⚡ 5. Circuit Breaker (`SafeInvoke` & Controller Lifecycle)

To protect the game simulation from edge cases, every automation controller inherits from `AutoWorkControllerBase`:

### Circuit Breaker Parameters:
- **`EvaluationInterval = 0.2f`**: Evaluates 5 times per second rather than every frame (`Update`), reducing CPU overhead by >90%.
- **`FailureLimit = 3`**: If 3 consecutive exceptions occur while executing a building's `Step(dt)`, the circuit breaker trips (`brokenOut = true`).
- **`RecoveryDelaySeconds = 60f`**: The building stays in vanilla manual mode for 60 seconds before testing recovery.
- **`RecoveryLimit = 5`**: If the building fails recovery 5 times in a row, automation permanently disengages for that instance, leaving it in normal vanilla manual operation.

```mermaid
stateDiagram-v2
    [*] --> ActiveAutomation: Building Spawned / Automation Enabled
    ActiveAutomation --> ActiveAutomation: Step(dt) Successful (5Hz)
    
    state ActiveAutomation {
        [*] --> CheckDupe
        CheckDupe --> YieldDupe: Dupe Operating
        YieldDupe --> CheckDupe: Dupe Steps Away
        CheckDupe --> CheckConditions: No Dupe
        CheckConditions --> DoWork: Power/Input OK
        CheckConditions --> Idle: Lacks Power/Input
    }
    
    ActiveAutomation --> TrippedCircuitBreaker: 3 Consecutive Exceptions (SafeInvoke)
    TrippedCircuitBreaker --> VanillaManualMode: Safe Stop Animation & Release Operational
    VanillaManualMode --> RecoveryWait: 60-Second Cooldown
    RecoveryWait --> ActiveAutomation: Try Recover (Attempt < 5)
    RecoveryWait --> PermanentManualFallback: Attempt >= 5 (Permanent Fallback)

    style ActiveAutomation fill:#22543d,stroke:#38a169,color:#fff
    style TrippedCircuitBreaker fill:#742a2a,stroke:#e53e3e,color:#fff
    style VanillaManualMode fill:#7b341e,stroke:#dd6b20,color:#fff
    style PermanentManualFallback fill:#4a5568,stroke:#a0aec0,color:#fff
```

---

## 🖼️ 6. UI Hierarchy & Dialog Management

In **v2.5.0**, the UI system was refactored to prevent rendering conflicts and clipping with third-party mod menus:

```mermaid
graph TD
    subgraph Layering ["Unity Screen Layering"]
        A["Base HUD / World Canvas (sortingOrder 0-100)"]
        B["Pause Screen / FrontEnd (sortingOrder 200)"]
        C["ModMenu Pause Dialog (sortingOrder 250-300)"]
        D["Automatic Industry Building Configuration Editor (sortingOrder 350)"]
    end

    C -->|Open Configuration| E["BuildingConfigEditorScreen.Show()"]
    E --> F["Push Dialog to DialogStack"]
    F --> G["Temporarily Hide Background ModMenu Window"]
    G --> D
    
    D -->|Click Exit or Close| H["Save Changes to Disk"]
    H --> I["Pop Dialog from DialogStack"]
    I --> J["Restore Background ModMenu Window"]
```

- **Explicit Canvas Sorting (`sortingOrder = 350`)**: Ensures the Building Configuration Editor is always rendered in front of both the game's pause menu and ModMenu's configuration list.
- **Dynamic Dialog Stack Management**: Automatically hides parent dialogs when opening child screens, restoring them cleanly upon exit without mouse-capture or raycast blocking bugs.
