# Compatible Mods with Automatic Industry

> [!NOTE]
> **Ecosystem Compatibility Guarantee**:  
> Automatic Industry is rigorously tested against popular Steam Workshop mods. When overlapping functionality or potential engine collisions occur, we actively engineer and maintain defensive compatibility shims.

- **Automatic Industry Workshop**: [Steam Workshop (ID: 3782701870)](https://steamcommunity.com/sharedfiles/filedetails/?id=3782701870)
- **Technical Crash Guards Reference**: [Multi-Mod Compatibility & Crash Guards](Multi-Mod-Compatibility-and-Crash-Guards)
- **Incorporated Features Reference**: [Incorporated Features Reference](Incorporated-Features-from-Obsolete-Mods)

---

## 📋 Compatibility Summary Table

| Mod Name | Workshop ID | Author / Origin | Category | Compatibility Status |
| :--- | :---: | :--- | :--- | :---: |
| **Multithreaded Simulation (SimDLL_Rust)** | `3779276157` | Community | Engine / Sim | 🟢 Fully Compatible (RC-2.6+) |
| **FastTrack** | GitHub | Peter Han | Engine / Performance | 🟢 Fully Compatible (Beta) |
| **Mod Menu** | `3789353358` | Cherry / Eurekalo | UI / Config | 🎨 Fully Integrated (v1.4.13+) |
| **Ronivan's Legacy - Industrial Suite** | `3557584850` | Ronivan / Port | Industry / Machines | 🛡️ Fully Compatible (v2.4.38+) |
| **No Manual Delivery** | `2047308624` | roland | Logistics / Delivery | 🛡️ Fully Compatible (v2.4.35+) |
| **Zoned Auto Sweeper** | `3745253371` | Community | Logistics / Sweep | 🟢 Fully Compatible |
| **Adjustable Transfer Arm** | Community | Community | Logistics / Sweep | 🟢 Fully Compatible |
| **Empty Storage** | `3626760918` | Community | Storage / Utility | 🛡️ Fully Compatible |
| **自动堆肥 / Auto Compost** | `2995311574` | Community | Agriculture / Compost | 🟢 Fully Compatible |

---

## 🗂️ Compatibility Cards

### 1. 多线程模拟 Multithreaded Simulation (SimDLL_Rust)
- **Workshop Link**: [Steam Workshop (ID: 3779276157)](https://steamcommunity.com/sharedfiles/filedetails/?id=3779276157)
- **Mod Purpose**: Rewrites the core physics and element simulation in native Rust with multi-threading capabilities to eliminate mid-game frame stuttering.
- **Compatibility Guarantee**: **Thread-Safe**. Automatic Industry simulation controllers execute on the Unity engine main thread cadence (`ISim200ms`, `ISim1000ms`), avoiding asynchronous memory race conditions against native Rust worker threads.

---

### 2. FastTrack
- **Release Link**: [GitHub Releases (FastTrack Beta)](https://github.com/peterhaneve/ONIMods/releases#release-FastTrackBeta)
- **Mod Purpose**: Heavily optimizes game loops, pathfinding, and memory caching to boost colony performance.
- **Compatibility Guarantee**: **High Performance**. Automatic Industry caches component references during `Prepare()` and avoids reflective lookups (`GetComponent`, `Find`) during high-frequency simulation ticks.

---

### 3. Mod Menu (v1.4.13+)
- **Workshop Link**: [Steam Workshop (ID: 3789353358)](https://steamcommunity.com/sharedfiles/filedetails/?id=3789353358)
- **Mod Purpose**: In-game mod manager providing access to mod configuration screens directly from the Pause Screen without restarting.
- **Compatibility Guarantee**: **Full UI Integration**. Features language-agnostic Options button locator, canvas sorting order 350 to prevent dialog occlusion, and mutual attribution tag de-duplication.

---

### 4. Ronivan's Legacy - Industrial Revolution
- **Workshop Link**: [Steam Workshop (ID: 3557584850)](https://steamcommunity.com/sharedfiles/filedetails/?id=3557584850)
- **Mod Purpose**: Introduces new industrial manufacturing buildings, chemical processing lines, and metallurgy tech.
- **Compatibility Guarantee**: **Defensive Shims Active**. Automated fabricator logic cleanly discovers machines derived from `ComplexFabricator`. Dedicated compatibility shims auto-heal `SymbolOverrideController` deserialization on save load and ensure safe UI parenting for in-game `BuildingEditor` windows.

---

### 5. No Manual Delivery
- **Workshop Link**: [Steam Workshop (ID: 2047308624)](https://steamcommunity.com/sharedfiles/filedetails/?id=2047308624)
- **Mod Purpose**: Toggles buildings to refuse manual Duplicant deliveries, routing supply chores exclusively to Auto-Sweepers (`SolidTransferArm`).
- **Compatibility Guarantee**: **Defensive Shims Active**. Provides dual-tier fallback probers to prevent `NullReferenceException` on save load, and suppresses `StandardWorker.AttachOverrideAnims` to prevent assertion crashes when robotic arms execute pickup errands.

---

### 6. Zoned Auto Sweeper & Adjustable Transfer Arm
- **Workshop Link**: [Steam Workshop (ID: 3745253371)](https://steamcommunity.com/sharedfiles/filedetails/?id=3745253371)
- **Mod Purpose**: Allows drawing custom, cell-specific pickup/drop-off zones or extending the reach radius of Auto-Sweepers.
- **Compatibility Guarantee**: **Adaptive Grid**. `AutoSweeperHarvestController` dynamically queries custom cell zones and pick filters rather than hardcoding vanilla's 4-cell radius.

---

### 7. Empty Storage
- **Workshop Link**: [Steam Workshop (ID: 3626760918)](https://steamcommunity.com/sharedfiles/filedetails/?id=3626760918)
- **Mod Purpose**: Adds a command button allowing players to instruct Duplicants to dump all contents of a storage container onto the floor.
- **Compatibility Guarantee**: **Entity Shielding**. `VanillaEmptyPaths.cs` intercepts manual ejection requests, protecting dropped item entities from accidental re-consumption loops.

---

### 8. 自动堆肥 / Auto Compost
- **Workshop Link**: [Steam Workshop (ID: 2995311574)](https://steamcommunity.com/sharedfiles/filedetails/?id=2995311574)
- **Mod Purpose**: Automates composting by removing the need for Duplicants to turn polluted dirt.
- **Compatibility Guarantee**: **Loop-Free**. Automatic Industry's independent compost flip logic operates cleanly without recursive state machine loops.
