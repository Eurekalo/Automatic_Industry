# Compatible Mods with Automatic Industry

> [!WARNING]
> **Attention**: This is a planned list for compatibility with other mods, which is continuously maintained and tested. Some integrations are actively undergoing refinement and debugging across game updates.

- **Automatic Industry Workshop**: [Steam Workshop (ID: 3782701870)](https://steamcommunity.com/sharedfiles/filedetails/?id=3782701870)
- **Incorporated Features from Obsolete Mods**: [Incorporated Features Reference](Incorporated-Features-from-Obsolete-Mods)
- **Technical Crash Guards & Shims Reference**: [Multi-Mod Compatibility & Crash Guards](Multi-Mod-Compatibility-and-Crash-Guards)

---

## 📋 Compatibility Summary Table

| Mod Name | Workshop ID | Author / Origin | Category | Compatibility Status |
| :--- | :---: | :--- | :--- | :---: |
| **Multithreaded Simulation (SimDLL_Rust)** | `3779276157` | Community | Engine / Sim | 🟢 Fully Compatible (v59-744825-RC-3.1) |
| **FastTrack** | GitHub | Peter Han | Engine / Performance | 🟢 Fully Compatible (Beta 0.18.6.0) |
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
- **Compatibility Status**: **Fully Compatible (v59-744825-RC-3.1)**. Automatic Industry automation controllers execute on the Unity main-thread cadence (`ISim200ms`, `ISim1000ms`), ensuring thread-safe interaction without race conditions against native Rust worker threads.

---

### 2. FastTrack
- **Release Link**: [GitHub Releases (FastTrack Beta)](https://github.com/peterhaneve/ONIMods/releases#release-FastTrackBeta)
- **Mod Purpose**: Heavily optimizes game loops, pathfinding, and memory caching to boost colony performance.
- **Compatibility Status**: **Fully Compatible (Beta 0.18.6.0)**. Component lookups and reflective calls are cached during `Prepare()`, avoiding expensive `GetComponent` calls during high-frequency simulation ticks. Precondition-based chore suppression completely prevents ReportManager note inflation.

---

### 3. Mod Menu (v1.4.13+)
- **Workshop Link**: [Steam Workshop (ID: 3789353358)](https://steamcommunity.com/sharedfiles/filedetails/?id=3789353358)
- **Mod Purpose**: In-game mod manager providing access to mod configuration screens directly from the Pause Screen without restarting.
- **Compatibility Status**: **Fully Integrated (v1.4.13)**. Features language-agnostic Options button locator, canvas sorting order 350 to prevent dialog occlusion, positioning Mod Menu directly below "选项" (Options) across all languages, with mutual attribution tag de-duplication.

---

### 4. Ronivan's Legacy - Industrial Suite
- **Workshop Link**: [Steam Workshop (ID: 3557584850)](https://steamcommunity.com/sharedfiles/filedetails/?id=3557584850)
- **Description**: Revives and expands upon a classic mod to introduce new industrial-era machinery, resources, and complex manufacturing chains into the game's tech progression.
- **Compatibility Status**: **Fully Compatible (v2.4.38+)**. Automated fabricator logic cleanly discovers machines derived from ComplexFabricator. Dedicated compatibility shims auto-heal SymbolOverrideController deserialization on save load and ensure safe UI parenting for in-game BuildingEditor windows.

---

### 5. 自动堆肥 / Auto Compost
- **Workshop Link**: [Steam Workshop (ID: 2995311574)](https://steamcommunity.com/sharedfiles/filedetails/?id=2995311574)
- **Description**: Streamlines waste management by making composting fully automatic, removing the need for duplicants to manually flip dirt while increasing building capacity.
- **Compatibility Status**: **Compatible**. Automatic Industry's independent compost flip logic operates without infinite state recursion loops.

---

### 6. Empty Storage
- **Workshop Link**: [Steam Workshop (ID: 3626760918)](https://steamcommunity.com/sharedfiles/filedetails/?id=3626760918)
- **Description**: Adds a convenient command feature that allows players to easily instruct duplicants to empty all the contents of a storage bin back onto the floor.
- **Compatibility Status**: **Compatible**. `VanillaEmptyPaths.cs` intercepts manual ejection requests, protecting dropped item entities from accidental re-consumption loops.

---

### 7. Zoned Auto Sweeper
- **Workshop Link**: [Steam Workshop (ID: 3745253371)](https://steamcommunity.com/sharedfiles/filedetails/?id=3745253371)
- **Description**: Offers precise automation control by allowing players to draw custom, cell-specific pickup and drop-off zones for individual Auto-Sweepers instead of being restricted to their default circular radius.
- **Compatibility Status**: **Compatible**. `AutoSweeperHarvestController` dynamically queries custom cell zones and pick filters for plant harvesting.

---

### 8. No Manual Delivery
- **Workshop Link**: [Steam Workshop (ID: 2047308624)](https://steamcommunity.com/sharedfiles/filedetails/?id=2047308624)
- **Description**: Adds a toggle to buildings that prevents duplicants from manually delivering materials, ensuring logistics are handled exclusively by automated Auto-Sweepers.
- **Compatibility Status**: **Fully Compatible (v2.4.35)**. Intercepts `TransferArmGroupProber.Get()` with non-null fallback probers, and suppresses `StandardWorker.AttachOverrideAnims` to prevent assertion crashes when robotic arms execute pickup errands.
