# Mod Menu — Architecture & Standalone Separation Guide

## 1. Overview
The **Mod Menu** module is an in-game pause screen extension designed to allow players to browse installed/active mods and modify configuration settings during the game pause state without returning to the main menu.

Currently embedded in `AutomaticIndustry` under the `AutoMachineRebuilt.ModMenu` namespace, it is fully decoupled and ready to be extracted into its own standalone mod repository (`ModMenu`).

---

## 2. Core Architecture & Components

```
┌─────────────────────────────────────────────────────────────┐
│                        ONI Engine                           │
│  ┌──────────────┐         ┌──────────────┐                  │
│  │ PauseScreen  │         │ KMod.Manager │                  │
│  └──────┬───────┘         └──────┬───────┘                  │
└─────────┼────────────────────────┼──────────────────────────┘
          │ (Harmony Hook)         │ (Query Active Mods)
          ▼                        ▼
┌─────────────────────────────────────────────────────────────┐
│                    ModMenu Subsystem                        │
│                                                             │
│  ┌──────────────────────────────┐                           │
│  │ PauseScreenPatch             │                           │
│  │ - Clones native button style │                           │
│  │ - Injects "Mod Menu" button  │                           │
│  └──────────────┬───────────────┘                           │
│                 │ (OnClick)                                 │
│                 ▼                                           │
│  ┌──────────────────────────────┐                           │
│  │ ModMenuManager               │                           │
│  │ - Queries active mod list    │                           │
│  │ - Manages dialog lifecycle   │                           │
│  │ - Applies live config reload │                           │
│  └──────────────┬───────────────┘                           │
│                 │                                           │
│                 ▼                                           │
│  ┌──────────────────────────────┐                           │
│  │ ModMenuDialog (Unity UI)     │                           │
│  │ - Mod list overview          │                           │
│  │ - In-game live config editor │                           │
│  │ - Real-time save & reload    │                           │
│  └──────────────────────────────┘                           │
└─────────────────────────────────────────────────────────────┘
```

### 2.1 File Structure
- `src/ModMenu/ModMenuManager.cs`: Main coordinator and facade.
- `src/ModMenu/PauseScreenPatch.cs`: Non-invasive Harmony hook on `PauseScreen.OnPrefabInit`.
- `src/ModMenu/ModMenuDialog.cs`: Pure Unity UI modal dialog supporting mod browsing and live option editing.

---

## 3. Extensibility & Multi-Mod Compatibility
- **Universal Mod Discovery**: Reads `Global.Instance.modManager.mods` directly, supporting all Workshop and local mods.
- **PLib & Native Options Interop**: Supports reading and editing options via JSON reflection or PLib `POptions`.
- **Zero Asset Dependency**: Built entirely using runtime Unity UI primitives (`Canvas`, `VerticalLayoutGroup`, `ScrollRect`, `Image`, `Button`, `Text`), ensuring compatibility across all game versions and DLCs (Base, Spaced Out, Frosty, Bionic).

---

## 4. Standalone Extraction Plan (Future Mod)
To separate Mod Menu into an independent mod:
1. Create a new repository / folder `ModMenu/`.
2. Move `src/ModMenu/*` to the new mod's `src/` directory.
3. Update `mod.yaml` and `mod_info.yaml` with title `"Mod Menu"` and ID `"ModMenu"`.
4. Provide a public API (`IModMenuProvider` or event hooks) so third-party mods can register custom config tabs dynamically.
