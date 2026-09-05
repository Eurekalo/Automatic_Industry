#!/usr/bin/env bash
# Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.
#
# Compiles the mod with the Mono C# compiler against a local game install.
#
# Usage:
#   ONI_MANAGED=/path/to/OxygenNotIncluded_Data/Managed \
#   PLIB=/path/to/PLib.dll ./build.sh

set -euo pipefail

ROOT="$(cd "$(dirname "$0")" && pwd)"
OUT="${OUT:-$ROOT/dist}"
M="${ONI_MANAGED:?Set ONI_MANAGED to the game Managed folder}"
PLIB="${PLIB:?Set PLIB to the path of PLib.dll}"

mkdir -p "$OUT"

mcs -target:library -langversion:latest -nostdlib -noconfig \
  -out:"$OUT/AutomaticIndustry.raw.dll" \
  -r:"$M/mscorlib.dll" \
  -r:"$M/System.dll" \
  -r:"$M/System.Core.dll" \
  -r:"$M/netstandard.dll" \
  -r:"$M/UnityEngine.dll" \
  -r:"$M/UnityEngine.CoreModule.dll" \
  -r:"$M/Assembly-CSharp.dll" \
  -r:"$M/Assembly-CSharp-firstpass.dll" \
  -r:"$M/0Harmony.dll" \
  -r:"$M/Newtonsoft.Json.dll" \
  -r:"$PLIB" \
  $(find "$ROOT/src" -name '*.cs')

# PLib MUST be merged into the mod assembly. Shipping PLib.dll next to the mod
# makes the game load it as a second mod DLL, which breaks PLib's mod ownership
# detection and removes the "Options" button from the mods screen.
ILREPACK="${ILREPACK:?Set ILREPACK to the path of ILRepack.exe}"
mono "$ILREPACK" /target:library /internalize /ndebug \
  /lib:"$M" /lib:"$(dirname "$PLIB")" \
  /out:"$OUT/AutomaticIndustry.dll" \
  "$OUT/AutomaticIndustry.raw.dll" "$PLIB"
rm -f "$OUT/AutomaticIndustry.raw.dll" "$OUT/PLib.dll"

cp "$ROOT/mod.yaml" "$ROOT/mod_info.yaml" "$ROOT/LICENSE" "$OUT/"

echo "Built $OUT/AutomaticIndustry.dll"