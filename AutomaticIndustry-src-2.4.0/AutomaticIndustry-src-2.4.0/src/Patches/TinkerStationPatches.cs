// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using AutoMachineRebuilt.Config;
using AutoMachineRebuilt.Util;
using HarmonyLib;

namespace AutoMachineRebuilt.Patches
{
    /// <summary>
    /// Prevents TinkerStation (Power Control Station, Farm Station) from spawning
    /// duplicant work chores while automated, and cleans up any pending chores.
    /// </summary>
    internal static class TinkerStationPatches
    {
        [HarmonyPatch(typeof(TinkerStation), "UpdateChore")]
        public static class TinkerStation_UpdateChore_Patch
        {
            public static bool Prefix(TinkerStation __instance, ref Chore ___chore)
            {
                if (__instance == null) return true;

                KPrefabID prefabID = __instance.GetComponent<KPrefabID>();
                string optionKey = prefabID != null ? prefabID.PrefabTag.Name : null;

                if (!string.IsNullOrEmpty(optionKey) && AutoMachineOptions.IsEnabledFor(__instance.gameObject, optionKey))
                {
                    // Automation is active: cancel any active duplicant work chore
                    if (___chore != null)
                    {
                        ___chore.Cancel("Automated by AutoMachine Rebuilt");
                        ___chore = null;
                    }
                    return false; // Skip vanilla UpdateChore, preventing chore recreation
                }

                return true; // Manual mode: allow vanilla UpdateChore to execute
            }
        }
    }
}
