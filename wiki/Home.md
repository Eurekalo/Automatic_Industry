# Automatic Industry Wiki

Welcome to the **Automatic Industry (Auto Machine Rebuilt)** technical wiki and architectural reference.

This wiki documents the engineering internals, code functions, state machine interceptions, and simulation logic behind every automated building in the mod.

---

## 📚 Table of Contents

1. **[Automated Buildings Code Logic](Automated-Buildings-Logic)**
   - Detailed breakdown of all 50+ automated buildings.
   - Exact C# controller classes, interfaces (`ISim200ms`, `ISim1000ms`), lifecycle methods (`OnPrefabInit`, `OnSpawn`, `Step`, `StopAutomation`), and operational flowcharts.
2. **[Architecture & Core Systems](Architecture-and-Design)**
   - Component-driven architecture vs transpilers.
   - `BuildingPrefabInjection` scan pipeline.
   - `AutoBuildingCustomizer` per-building independent toggle and wrench errand dispatch.
   - `ChoreSuppression` zero-allocation Duplicant chore cancellation.
3. **[Multi-Mod Compatibility & Crash Guards](Multi-Mod-Compatibility-and-Crash-Guards)**
   - Compatibility layers for *No Manual Delivery*, *PLib*, *Customize Buildings*, *I_实用系统*, *Multithreaded Simulation (SimDLL_Rust)*, *EmptyStorage*, *Adjustable Transfer Arm*, *Mod Menu*, and *ONI Together*.
   - Proactive `SymbolOverrideController` injection, animation override suppression on robotic workers, and prober fallbacks.
4. **[Compatible Mods Roster](Compatible-Mods)**
   - Planned and verified compatible community mods (*Multithreaded Simulation*, *FastTrack*, *ModMenu*, *Ronivan's Legacy*, *Auto Compost*, *Empty Storage*, *Zoned Auto Sweeper*, *No Manual Delivery*).
5. **[Incorporated Features from Obsolete Mods](Incorporated-Features-from-Obsolete-Mods)**
   - Community mod concepts modernized, ported, and maintained (*Auto-Sweeper Harvest*, *Liquid Reservoir Boost*).

---

## 🎯 Core Engineering Principles

Automatic Industry is designed around four strict engineering rules:

1. **Vanilla Condition Parity**:
   - Automation only operates when all vanilla preconditions hold: electrical power, ingredient supply, storage headroom, ambient pressure, temperature thresholds, and room requirements.
   - No cheat outputs, no recipe modifications, and no relaxed power costs (except the explicitly optional legacy Oil Refinery 100% efficiency toggle).
2. **Zero Save Data Footprint**:
   - No custom serializable fields, no new building prefabs, and no custom entities stored in the save file.
   - Completely safe to install or uninstall at any point in an existing colony without corrupting save files or leaving orphaned data.
3. **Safe Exception Containment (`SafeInvoke`)**:
   - Every reflective method call, state machine poll, and controller step is protected by `SafeInvoke.Try`.
   - Any runtime failure logs cleanly to `player.log` and degrades to manual operation instead of crashing the Unity game thread.
4. **Clean Component Architecture**:
   - ~70% pure component-driven logic attached to completed prefabs via `GeneratedBuildings.LoadGeneratedBuildings` postfix.
   - Avoids fragile IL transpilers that break upon game updates.
