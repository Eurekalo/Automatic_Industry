# Automatic Industry

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

- **[Interactive Engineering Wiki](https://github.com/Eurekalo/Automatic_Industry/wiki)** — Comprehensive visual architectural documentation, building logic flowcharts, and crash guard specifications.
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
- See [CHANGELOG.md](AutomaticIndustry-src-2.4.0/AutomaticIndustry-src-2.4.0/CHANGELOG.md) for full patch notes.

- **v2.5.0**:
  - **Building Configuration Editor**: Full-screen dual-pane editor with category filtering, real-time search, category active counters, **Batch Toggle**, and dedicated **Save Button** with immediate auto-save.
  - **In-Game Details Automation Button**: Directly toggle automation from building inspection panels; supports **Shift + Left Click** to instantly toggle without queueing Duplicant wrench errands.
  - Excluded non-prefab mechanics (Auto-Sweeper Harvest, Geyser Study) from the Building Configuration Editor, retaining them strictly in Mod Options.
  - Consolidated Virtual Planetarium (`DLC1CosmicResearchCenter`) into a unified DLC entry without redundant Base Game / Spaced Out duplicate entries.
  - Corrected Telescope sprite fallback in Spaced Out! to use the enclosed domed observatory icon (`ClusterTelescopeEnclosed`) instead of the low tripod.
  - Enhanced ModMenu compatibility with automatic multi-dialog hiding and restoration, alongside canvas sorting order 350 to eliminate window occlusion.
  - Comprehensive automated regression tests verified (270/270 sandbox tests passing).

---

## 🛠️ Building From Source

```powershell
# 1. Compile Release DLL with ILRepack
dotnet build AutomaticIndustry-src-2.4.0/AutomaticIndustry-src-2.4.0/AutoMachineRebuilt.csproj -c Release

# 2. Run Offline Sandbox Regression Test Suite
dotnet run --project toolchain/sandbox/AutomaticIndustry.Sandbox.csproj

# 3. Package Release Archives
powershell -ExecutionPolicy Bypass -File toolchain/scripts/package-2.5.0.ps1
```

---

## 📄 License & Attribution

- **License**: [MIT License](LICENSE)
- **Original Concept**: [auto machine机器自动工作](https://steamcommunity.com/sharedfiles/filedetails/?id=2992024030) by **一见倾心**
- **In-Game Options**: [PLib](https://github.com/peterhaneve/ONI-Mods/tree/main/PLib) by Peter Han (MIT License)
- *Powered by Lovable / Claude Opus / Gemini Flash*
