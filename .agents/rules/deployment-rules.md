---
trigger: always_on
description: Packaging and deployment rules for Automatic Industry and Mod Menu.
---

# Mod Packaging and Deployment Rules

1. **No Game Directory Deployment (Documents/Klei)**:
   - **DO NOT** deploy, copy, or extract mod packages into the game's local mod folder (`Documents/Klei/OxygenNotIncluded/mods/local/` or `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\local\`).

2. **Workspace Root Deployment Allowed**:
   - Building, packaging, and extracting mod release folders (e.g. `AutomaticIndustry-X.Y.Z/`, `ModMenu-X.Y.Z/`) directly inside the project root workspace directory is permitted and encouraged.
   - Output release ZIP archives (`.zip`) and source ZIP archives (`-src-*.zip`) inside the project workspace root.

3. **Packaging Integrity**:
   - Maintain all standard build verification, multi-language localization audits, and automated sandbox test suites prior to packaging.
