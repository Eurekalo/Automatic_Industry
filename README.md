# Automatic Industry (Auto Machine Rebuilt)

<p align="center">
  <img src="https://raw.githubusercontent.com/Sgt-Imalas/Sgt_Imalas-Oni-Mods/refs/heads/master/Compat_All.png" alt="DLC Compatibility" />
</p>

<p align="center">
  <img src="https://i.imgur.com/wb8ho1w.png" alt="Automatic Industry Banner" />
</p>

<p align="center">
  <img src="https://i.imgur.com/5qdKRkE.gif" alt="Showcase Animation" />
</p>

> [!WARNING]
> **Rapid Iteration & Emergency Recovery**  
> This mod is currently undergoing rapid iteration. If you encounter a crash that prevents entering the game at all, cancel your subscription and delete the corresponding mod directory:  
> `Documents\Klei\OxygenNotIncluded\mods\Steam\3782701870`  
> *(If you like this mod, starring the repository or giving a thumbs up on Steam Workshop is greatly appreciated!)*

<p align="center">
  <img src="https://i.imgur.com/r1W2g4h.gif" alt="Section Divider" />
</p>

---

## 📖 What does this mod do?

**Automatic Industry** is a comprehensive, reverse-engineered automation overhaul for **Oxygen Not Included**.

- **Reduces Dupe Operation Tedium**: Designed to minimize repetitive manual operation errands performed by Duplicants and assist with game debugging.
- **Veteran-Focused Experience**: Not recommended for greenhorns on their first playthrough, as manual Duplicant logistics and operation are core to the early learning experience.
- **Safe Save Architecture**: 
  - Technically will **not damage** your existing saves or builds when enabled or removed.
  - Avoids volatile IL transpilers wherever possible (~70% pure component-driven architecture).
  - Automation generally strictly conforms to vanilla mechanics (requires power, input delivery, ambient pressure thresholds, cooldowns, and room requirements).
  - Individual stations and features can be toggled on/off on demand in Mod Settings or per-building User Menus.
- **Zero Recipe Inflation**: Does not add new items or buildings, and does not alter vanilla material input/output equations (except an optional toggle for Oil Refinery efficiency).

---

## 🔗 Credits & Heritage

- **Original Mod Concept**: Based on a ported and refactored rebuild of *Auto Machine* ([Steam Workshop 2992024030](https://steamcommunity.com/sharedfiles/filedetails/?id=2992024030)) by **一见倾心**. Special thanks to the original author!
- **DLC Compatibility Graphics**: Courtesy of **Sgt_Imalas**.
- **Rework & Modernization**: Maintained and overhauled by **Cherry / Eurekalo**.

---

## 🧩 Compatibility & Ecosystem

### Compatible Mods
> [!NOTE]
> This is a curated compatibility list. We actively audit and engineer compatibility shims for peer community mods.

- **[ModMenu](https://steamcommunity.com/sharedfiles/filedetails/?id=3789353358)** — In-game mod configuration manager.
- **[Curated Compatibility Mod List](https://rentry.co/a4g5oobm)** — Tested community mods.
- **Integrated Compatibility Shims**:
  - *No Manual Delivery* (Steam ID `2047308624`): Safe prober fallback and universal `FabricateFetch` dispatch.
  - *Customize Buildings* (Steam ID `1818138009`): Dynamic prefix shims protecting Oil Refinery, Oil Well Cap, Compost, Desalinator, and Fabricators from component destruction and recursion deadlocks.
  - *EmptyStorage* & *Adjustable Transfer Arm (U&F)*: Dynamic range adaptation and button attribution.
  - *ONI Together*: Full multiplayer synchronization packets and session safeguards.

<p align="center">
  <img src="https://i.imgur.com/de7JsfV.gif" alt="Compatibility Divider" />
</p>

### Incorporated Features from Obsolete Mods
Logic and behaviors from several inactive community mods have been ported, repaired, and integrated:
- **[Incorporated Mod List](https://rentry.co/49qzmxot)**  
*Thanks to all original mod creators for their contributions to the ONI modding community.*

---

## ⚠️ Important Notes & Reporting

- **Test Status**: While rigorously covered by an offline 239-test sandbox suite, mods with overlapping functionality may occasionally interact unexpectedly.
- **Troubleshooting**:
  1. Enable **Detailed Logging** in the mod options menu.
  2. Locate your `player.log` file:
     - Windows: `%USERPROFILE%\AppData\LocalLow\Klei\Oxygen Not Included\player.log`
  3. Upload the log using a third-party paste service (e.g. Pastebin, GitHub Gist) and post it with an issue description.
- **DLC Compatibility**: Fully supports the base game and all DLC expansions up to August 2026, including:
  - *Spaced Out!*
  - *The Frosty Planet Pack*
  - *The Bionic Booster Pack*
  - *The Prehistoric Planet Pack*
  - *The Aquatic Planet Pack*

<p align="center">
  <img src="https://i.imgur.com/cupfWX9.gif" alt="Content Divider" />
</p>

---

## 🌐 Localization & Language Support

- 🇬🇧 **English** (Default)
- 🇨🇳 **Simplified Chinese** (简体中文)
- 🇭🇰 **Traditional Chinese** (繁體中文)
- 🇰🇷 **Korean** (한국어)
- 🇯🇵 **Japanese** (日本語)

*Supports detailed diagnostic logging directly to `player.log`.*

---

## 🏭 Automated Buildings & Status

| Icon | Meaning |
| :---: | :--- |
| ✅ | Confirmed working flawlessly |
| ☑️ | 50-50 / manual interaction optional |
| ❌ | Known issue / under repair |
| 🤷 | Experimental / community verification welcome |
| ⚠️ | Reported issue undergoing investigation |

<p align="center">
  <img src="https://i.imgur.com/KllQl9z.gif" alt="Station Animation" />
</p>

### Building Automation Matrix

| Category | Building | Status | Operational Details |
| :--- | :--- | :---: | :--- |
| **Power** | Manual Generator | ✅ | Runs automatically while requested by grid |
| **Food** | Microbe Musher | ✅ | Automated recipe cooking |
| | Electric Grill | ✅ | Automated recipe cooking |
| | Deep Fryer | ✅ | Automated recipe cooking |
| | Gas Range | ✅ | Automated recipe cooking |
| | Spice Grinder | ✅ | 100 kg headroom, universal auto-sweeper/dupe delivery |
| | Sushi Bar | ✅ | Automated sushi preparation |
| | Food Dehydrator | ✅ | Automated batch dehydration & packet release |
| | Food Rehydrator | 🤷 | Experimental automated rehydration |
| | Food Smoker | ✅ | Automated smoking & auto-drop finished foods |
| **Plumbing** | Liquid Valve | ✅ | Flow rate adjustment without Duplicant wrench errands |
| **Ventilation** | Gas Valve | ✅ | Flow rate adjustment without Duplicant wrench errands |
| **Refinement** | Plywood Press | ✅ | Automated wood pressing |
| | Desalinator | ✅ | Auto-ejects salt upon reaching threshold (~945 kg) |
| | Rock Crusher | ✅ | Automated recipe crushing |
| | Metal Refinery | ✅ | Automated metal smelting |
| | Glass Forge | ✅ | Automated molten glass processing |
| | Vulcanizer | ✅ | Automated vulcanization |
| | Compost | ✅ | Automated pitchfork flipping, dual progress bars |
| | Sludge Press | ✅ | Automated sludge separation |
| | Gleaner | ✅ | Automated harvesting & resource release |
| | Plant Pulverizer | ✅ | Automated pulverization |
| | Oil Refinery | ✅ | Runs when ready; supports 50% vanilla and 100% legacy options |
| | Emulsifier | ✅ | Automated emulsification |
| | Diamond Press | ✅ | Automated diamond manufacturing |
| **Medicine** | Apothecary | ✅ | Automated medicine compounding |
| | Nuclear Apothecary | 🤷 | Klei unused internal asset |
| **Stations** | Grooming Station | ✅ | Tends 1 eligible critter per interval |
| | Shearing Station | ✅ | Automated critter shearing |
| | Milking Station | ✅ | Automated critter milking |
| | Aquatic Grooming Station | ✅ | Aquatic critter grooming |
| | Aquatic Shearing Station | ✅ | Aquatic critter shearing |
| | Aquatic Milking Station | ✅ | Aquatic critter milking |
| | Farm Station | ✅ | Automated plant fertilizer delivery & application |
| | Botanical Analyzer | ✅ | Automated seed genetic analysis |
| | Research Station | ✅ | Automated novice research |
| | Supercomputer | ✅ | Automated advanced research |
| | Material Study Terminal | ✅ | Automated radiation research |
| | Orbital Data Collection Lab | ☑️ | Semi-automated orbital data analysis |
| | Virtual Planetarium | ✅ | Automated astronomy research |
| | Geotuner | ✅ | Scientist interaction, displays geyser tuning progress |
| | Data Miner | ✅ | Automated data collection |
| | Power Control Station | ✅ | Automated microchip production |
| | Skill Scrubbing Station | 🤷 | Experimental |
| | Blastshot Maker | ✅ | Automated meteor defense ammo fabrication |
| | Crafting Station | ✅ | Automated station fabrication |
| | Soldering Station | ✅ | Automated soldering |
| | Textile Loom | ✅ | Automated garment weaving |
| | Clothing Refashionator | ✅ | Automated tailoring |
| | Exosuit Forge | ✅ | Automated suit & jetpack construction |
| **Utilities** | Oil Well Cap | ✅ | Automated depressurization at configurable pressure threshold |
| | Ice Liquefier | ✅ | Auto-ejects 500 kg water per cycle |
| | Wood Heater | ✅ | Automated space heating |
| | Ice-E Fan | ✅ | Automated cooling while ice is stored |
| **Rocketry** | Telescope | ✅ | Automated space scanning |
| | Enclosed Telescope | ✅ | Automated deep space astronomy |
| **Radiation** | Manual Radbolt Generator | ✅ | Automated radbolt generation |
| **Story Traits** | Biobot Builder | ✅ | Automated biobot assembly |

#### 🛢️ Oil Refinery Conversion Ratio Option
The Oil Refinery provides a configuration setting in **Mods → Options**:
- **50% Efficiency (Vanilla Default)**: `10 kg/s Crude Oil` → `5 kg/s Petroleum` + `90 g/s Natural Gas`
- **100% Efficiency (Legacy Mod)**: `10 kg/s Crude Oil` → `10 kg/s Petroleum` + `180 g/s Natural Gas`

<p align="center">
  <img src="https://i.imgur.com/n7dDM7N.gif" alt="Update Divider" />
</p>

---

## 📜 Update Highlights

See [CHANGELOG.md](AutomaticIndustry-src-2.4.0/AutomaticIndustry-src-2.4.0/CHANGELOG.md) for full patch notes.

- **v2.4.39**:
  - Eliminated game-crashing Black Hole NRE crash in `GeoTuner.TriggerSoundsForGeyserChange` caused by premature static constructor evaluation during `OnLoad`.
  - Implemented `GeoTunerSoundSafetyPatch` with dynamic audio path auto-healing via `GlobalAssets.GetSound(...)` and defensive null-checking prefix.
  - Proactive sound path hydration in `BuildingPrefabInjection` and `AutoGeoTuner.Prepare()`.
  - Added `FMODUnity` reference and automated regression Test #39 (246/246 sandbox tests passing).
- **v2.4.38**:
  - Save load & entity deserialization crash prevention for Ronivan's mods (Metallurgy, Chemical Processing, Nuclear).
  - Implemented `SymbolOverrideControllerCompatibility` prefix auto-healing `usingNewSymbolOverrideSystem = true` and guarding against missing `KBatchedAnimController`.
- **v2.4.37**:
  - In-game mod config UI parenting fix neutralizing `Chemical Processing` BuildingEditor `ShowWindow` NRE.
  - Safely routed fallback parenting to `ssOverlayCanvas` when `FrontEndManager` is null.
- **v2.4.36**:
  - ModMenu direct lifecycle decoupling, eliminating profile snapshot synchronization loops and restart wipeout prompts.
- **v2.4.35**:
  - PLib `OptionsDialog` parameter binding alignment (`PDialog dialog`) and load crash prevention via safe manual patch hook.
  - Multi-mod compatibility fix for **No Manual Delivery** (`SolidTransferArm` animation override assertion suppression via `StandardWorkerAttachOverrideAnimsPatch`).
  - Proactive `SymbolOverrideController` injection on `SolidTransferArm` completed prefabs.
  - Case-insensitive prefab lookup in `AutoMachineOptions` with deduplicated dictionary keys.
  - Deconstructed building footprint cell deregistration in `AutoBuildingCustomizer.OnCleanUp()`.
  - Circuit-connected battery query in `AutoManualGeneratorController` resolving chore oscillation.
  - Transfer arm vs Duplicant fetch disambiguation in `LiquidReservoirFetchPatches`.
  - Zero-allocation candidate probe caching in `LocalChoreProbe`.
- **v2.4.34**:
  - Full compatibility shim for **Customize Buildings** (Steam ID `1818138009`).
  - Switched `[MyCmpReq]` to `[MyCmpGet]` in `AutoOilRefinery` and `AutoOilWellCap` to prevent engine-level missing component errors.
  - Decoupled `ComplexFabricator` injection from `duplicantOperated`.
  - Neutralized Compost infinite state recursion and Desalinator transition clearing.
- **v2.4.33**:
  - Spice Grinder universal multi-ingredient automated delivery overhaul (Preserving Spice seed + salt delivery fix).
  - Expanded storage headroom from 31 kg to 100 kg to prevent discrete seed blockages.
  - Converted spice fetches to `FabricateFetch` for universal Duplicant and Auto-Sweeper support.
- **v2.4.32**:
  - Compatibility fix for **No Manual Delivery** mod (prober null fallback).
  - Multitool worker safety fix preventing transfer arm crashes on liquids.
- **v2.4.31**:
  - Kiln ceramic crash fix and safe guards for naturally unattended fabricators.
- **v2.4.28**:
  - Per-building independent user menu toggles and dual-track coexistence.
- **v2.4.9**:
  - Full support for in-game runtime option toggling via **ModMenu**.
- **v2.4.8**:
  - Reworked Ranching stations (land and aquatic).
  - Added Geotuner tuning count progress bar and research progress meters.

---

## 🛠️ Building From Source

```powershell
# 1. Compile Release DLL with ILRepack
dotnet build AutomaticIndustry-src-2.4.0/AutomaticIndustry-src-2.4.0/AutoMachineRebuilt.csproj -c Release

# 2. Run Offline Sandbox Regression Test Suite
dotnet run --project toolchain/sandbox/AutomaticIndustry.Sandbox.csproj

# 3. Package Release Archives
powershell -ExecutionPolicy Bypass -File toolchain/scripts/package-2.4.39.ps1
```

---

## 📄 License & Attribution

- **License**: [MIT License](LICENSE)
- **Original Concept**: [auto machine机器自动工作](https://steamcommunity.com/sharedfiles/filedetails/?id=2992024030) by **一见倾心**
- **In-Game Options**: [PLib](https://github.com/peterhaneve/ONI-Mods/tree/main/PLib) by Peter Han (MIT License)
- *Powered by Lovable / Claude Opus / Gemini Flash*
