# Automatic Industry (Auto Machine Rebuilt) Wiki

Welcome to the **Automatic Industry** engineering wiki and architectural reference.

Automatic Industry is a comprehensive, high-performance automation overhaul for *Oxygen Not Included*. It enables industrial machinery, agricultural stations, ranching equipment, research facilities, and utility buildings to operate continuously and automatically when supply, power, and environmental conditions are met.

---

## 🧭 System Architecture at a Glance

```mermaid
graph TB
    subgraph SubBoot ["Engine Boot and Injection"]
        A["Game Startup: GeneratedBuildings.LoadGeneratedBuildings"] -->|Harmony Postfix| B["BuildingPrefabInjection"]
        B --> C["Scan Assets.BuildingDefs"]
        C --> D["Run Mod Compatibility Shims"]
        D --> E["Attach Controllers and AutoBuildingCustomizer"]
    end

    subgraph SubSim ["Simulation Loop (5Hz / 1Hz)"]
        E --> F["AutoWorkControllerBase"]
        F --> G{"Duplicant Working?"}
        G -->|Yes| H["Yield to Duplicant: Duplicant Always Wins"]
        G -->|No| I{"Vanilla Conditions Met?"}
        I -->|No| J["Idle / Standby"]
        I -->|Yes| K["Execute Automation Step"]
        K --> L["Suppress Duplicate Dupe Chores"]
    end

    subgraph SubSafety ["Safety & Fallback Layer"]
        K -->|Exception| M{"Circuit Breaker"}
        M -->|3 Consecutive Errors| N["Trip Circuit Breaker"]
        N --> O["Safe 60s Cooldown to Manual Mode"]
        O -->|5 Recovery Attempts Exceeded| P["Permanent Manual Fallback"]
    end

    subgraph SubConfig ["User Configuration"]
        Q["Building Configuration Editor"] --> S["BuildingToggleManager"]
        R["In-Game Building Details Toggle<br/>Normal Click / Shift+Click"] --> S
        T["Mod Settings PLib / ModMenu"] --> S
        S --> F
    end

    style A fill:#2d3748,stroke:#4a5568,color:#fff
    style F fill:#1a365d,stroke:#2b6cb0,color:#fff
    style K fill:#22543d,stroke:#38a169,color:#fff
    style N fill:#742a2a,stroke:#e53e3e,color:#fff
    style Q fill:#44337a,stroke:#805ad5,color:#fff
```

---

## 📚 Wiki Sections & Navigation

<div align="center">

| Section | Description | Key Topics |
| :--- | :--- | :--- |
| **[🎛️ Building Configuration & Controls](Building-Configuration-and-Controls)** | Interactive dual-pane configuration screen and in-game controls. | Dual-pane editor, search & category filters, Batch Toggle, Save button, Shift+Left Click quick toggle. |
| **[⚙️ Automated Buildings Logic](Automated-Buildings-Logic)** | Comprehensive logic reference for all 40+ automated buildings. | State machine hooks, power/fuel conditions, recipe progression, auto-drop, and chore suppression. |
| **[🏛️ Architecture & System Design](Architecture-and-Design)** | Deep dive into the component-driven engineering systems. | Prefab injection pipeline, 5Hz simulation cadence, zero-allocation chore filters, and circuit breakers. |
| **[🛡️ Multi-Mod Compatibility & Crash Guards](Multi-Mod-Compatibility-and-Crash-Guards)** | Defensive shims protecting against third-party mod conflicts. | Shims for *No Manual Delivery*, *ModMenu*, *Ronivan's Mods*, *Customize Buildings*, *FastTrack*, *SimDLL*. |
| **[📦 Compatible Mods Roster](Compatible-Mods)** | Tested, verified, and community-audited compatible mods. | Workshop links, compatibility status, tested versions, and multi-mod configuration notes. |
| **[📜 Incorporated Features](Incorporated-Features-from-Obsolete-Mods)** | Modernized and maintained features from legacy mods. | Auto-Sweeper crop harvesting, Liquid Reservoir direct fetching, and 100% save-safe ports. |

</div>

---

## 🎯 Core Engineering Principles

Automatic Industry is engineered with strict adherence to four non-negotiable principles:

```mermaid
flowchart TD
    Root["<b>Core Engineering Principles</b>"] --> P1["<b>Zero Save Footprint</b><br/>• No custom types in save file<br/>• Safe to install or remove anytime<br/>• Zero orphan data or corruption"]
    Root --> P2["<b>Vanilla Condition Parity</b><br/>• Strict power and fuel requirements<br/>• Input delivery and storage headroom<br/>• Environmental pressure/temperature checks<br/>• Zero recipe inflation"]
    Root --> P3["<b>Duplicant Priority</b><br/>• Duplicants always take precedence<br/>• Automation yields immediately on contact<br/>• Resume cleanly when duplicant finishes"]
    Root --> P4["<b>Circuit Breaker Containment</b><br/>• All steps guarded by SafeInvoke<br/>• 3-failure trip limit<br/>• Automatic 60s cooldown and recovery<br/>• Graceful degradation to manual mode"]

    style Root fill:#1a365d,stroke:#2b6cb0,color:#fff
    style P1 fill:#22543d,stroke:#38a169,color:#fff
    style P2 fill:#2d3748,stroke:#4a5568,color:#fff
    style P3 fill:#7b341e,stroke:#dd6b20,color:#fff
    style P4 fill:#742a2a,stroke:#e53e3e,color:#fff
```

| Principle | Technical Implementation | Benefit to Player |
| :--- | :--- | :--- |
| **Zero Save Footprint** | No custom serialized MonoBehaviours or entity classes. Prefab settings map to vanilla tags and colony registries. | Completely safe to enable, update, or uninstall mid-playthrough without corrupting saves. |
| **Vanilla Condition Parity** | Controllers strictly check `Operational.IsOperational`, `JoulesToGenerate`, storage headroom, and ambient pressure. | Preserves authentic game balance and power grids; no magical infinite resources. |
| **Duplicant Priority** | Real-time `worker != null` check yields control to Duplicants immediately whenever commanded. | Players can manually prioritize emergency tasks without disabling mod settings. |
| **Circuit Breakers (`SafeInvoke`)** | Evaluates steps within try-catch circuit breakers with auto-recovery cooldowns. | Transient mod errors or edge cases degrade to manual mode rather than crashing the game to desktop. |

---

## 🚀 What's New in v2.5.0

- **Dual-Pane Building Configuration Editor**:
  - Full-screen configuration interface with left-pane category filters (`All`, `Power`, `Food`, `Refinement`, `Stations`, `Utilities`) and real-time text search.
  - Active building counters (`Active: X / Total: Y`) per category.
  - **Batch Toggle Button**: Enable or disable all visible buildings with a single click.
  - **Dedicated Save Button & Immediate Auto-Save**: Guaranteed disk persistence across game sessions.
- **In-Game Building Details Automation Button**:
  - Direct toggle button on building inspection panels.
  - **Shift + Left Click**: Instantly flip automation on/off without waiting for a Duplicant wrench errand.
  - Rich bilingual tooltip explaining normal click vs Shift+Click behavior and mod attribution.
- **ModMenu Integration**:
  - Canvas sorting order set to 350, ensuring modal dialogs render cleanly above pause menus without UI clipping or occlusion.
  - Automatic dialog stack hiding and restoration when opening sub-screens.
- **Enhanced DLC & Base Game Filtering**:
  - Virtual Planetarium consolidated into a unified entry across DLCs.
  - Telescope dynamic sprite fallback correctly displays domed observatory icon in Spaced Out!

---

## 🔗 Community & Workshop Links

- **Steam Workshop**: [Automatic Industry (Steam ID: 3782701870)](https://steamcommunity.com/sharedfiles/filedetails/?id=3782701870)
- **Source Code Repository**: [GitHub (Eurekalo/Automatic_Industry)](https://github.com/Eurekalo/Automatic_Industry)
- **Issue Tracker**: [GitHub Issues](https://github.com/Eurekalo/Automatic_Industry/issues)
