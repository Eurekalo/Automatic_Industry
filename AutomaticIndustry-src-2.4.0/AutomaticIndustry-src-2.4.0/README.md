# Auto Machine Rebuilt

A maintained, refactored rebuild of the *Auto Machine* mod
([Steam Workshop 2992024030](https://steamcommunity.com/sharedfiles/filedetails/?id=2992024030))
for **Oxygen Not Included**.

Industrial buildings keep running **without Duplicants**, but only while their
vanilla requirements are met: power, delivered ingredients, room requirements,
gas pressure and cooldowns are all respected. No new buildings, no new
recipes, no changed build costs.

## Compatibility

* Base game and **Spaced Out!**
* The Frosty Planet Pack, The Bionic Booster Pack, The Prehistoric Planet Pack,
  The Aquatic Planet Pack

DLC-only buildings are patched by type. If a DLC is not installed the game
never creates those prefabs, so the corresponding patches simply never run.

## Automated buildings

| Building | Mechanism | Default |
| --- | --- | --- |
| Electric Grill, Gas Range, Microbe Musher, Deep Fryer, Plant Pulverizer, Smoker, Rock Crusher, Metal Refinery, Glass Forge, Molecular Forge, Exosuit Forge, Textile Loom, Clothing Refashionator, Sculpting Block, Soldering Station, Sludge Press, Diamond Press, Emulsifier, Blastshot Maker, Data Miner | `duplicantOperated = false` | on |
| Smoker (emptying) | drops finished products automatically | on |
| Oil Refinery | runs while the state machine reports "ready" | on |
| Oil Well | releases pressure at the building's own threshold | off |
| Geotuner | performs the scientist interaction and geyser switch | off |
| Grooming / Shearing / Milking Station (land and aquatic) | tends one eligible critter per interval | off |
| Manual Generator, Telescope / Enclosed / Cluster Telescope, Manual Radbolt Generator, Skill Scrubber, Ice Kettle, Campfire, Ice-E Fan, Compost, Dehydrator | generic work driver, gated on the building's own chore | off |
| Apothecary, Advanced Apothecary, Sushi Bar | `duplicantOperated = false` | off |
| Super Computer, Virtual Planetarium, Materials Study Terminal, Orbital Research Center, Botanical Analyzer, Biobot Builder | generic work driver | off |
| Mission Control (base and cluster), Farm Station, Spice Grinder | generic work driver, plus an optional *ignore room requirement* switch | off |
| Geysers and study-able features | performs the first study automatically | off |

Everything is individually toggleable in **Mods → Options**. Buildings the
original mod already automated default to **on**; newly automated buildings
default to **off** so existing colonies keep behaving as their owner expects.

## Options language

The **General** category contains two UI settings:

* **Options language** — `Auto` (follows the game language), `English`,
  `简体中文`, `繁體中文`, `한국어`, `日本語`. Changes apply the next time the
  mod options screen is opened; no game restart required.
* **Bilingual labels** — shows `English / translation` on every label so the
  options screen can be read in both languages at once.

Building names are always taken from the running game, so they match the
vanilla wording of the active language, DLC buildings included.

## Oil Refinery conversion ratio

The legacy mod shipped a 1:1 conversion. This rebuild defaults to the **vanilla
ratio** (10 kg/s crude oil → 5 kg/s petroleum + 90 g/s natural gas) and offers
the legacy **full (100%)** ratio as an explicit option.

## Building from source

```bash
ONI_MANAGED="/path/to/OxygenNotIncluded_Data/Managed" \
PLIB="/path/to/PLib.dll" ./build.sh
```

or open `AutoMachineRebuilt.csproj` after setting the `ONIManaged` property.

Copy `AutoMachineRebuilt.dll`, `PLib.dll`, `mod.yaml` and `mod_info.yaml` into
`Documents/Klei/OxygenNotIncluded/mods/local/AutoMachineRebuilt/`.

## Documentation

* [CHANGELOG.md](CHANGELOG.md) — what changed in each version.
* [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md) — architecture, mechanisms and
  the file that owns each behaviour.

## Design notes

* All automation runs from `Sim1000ms` / `Sim200ms` callbacks on components
  attached to the vanilla prefabs — no state machine rewrites, no new save
  data, so removing the mod leaves saves loadable.
* Every automation step is wrapped in `SafeInvoke.Try`, so a failure logs once
  and never propagates into the simulation loop.
* Private game members are reached through Harmony `AccessTools` with null
  checks, so a future game update degrades to "feature disabled" instead of a
  crash.

## License

MIT — see [LICENSE](LICENSE). Original mod concept and behaviour by the
author of *Auto Machine*; this rebuild keeps that attribution.