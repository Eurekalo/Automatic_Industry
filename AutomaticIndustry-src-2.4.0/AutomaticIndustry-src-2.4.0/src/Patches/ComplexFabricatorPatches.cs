// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.
// Original mod concept: "AutoMachine" (Steam Workshop id 2992024030).

using AutoMachineRebuilt.Config;
using AutoMachineRebuilt.Util;
using HarmonyLib;

namespace AutoMachineRebuilt.Patches
{
    /// <summary>
    /// Removes the Duplicant requirement from all supported
    /// <see cref="ComplexFabricator"/> buildings.
    ///
    /// This is the mechanism the original mod used, applied through a single
    /// generic patch instead of one patch per building configuration. Doing
    /// it on the spawned instance (not on the prefab configuration) keeps
    /// the change reversible: turning an option off and restarting restores
    /// the vanilla behaviour without touching the save file.
    /// </summary>
    [HarmonyPatch(typeof(ComplexFabricator), "OnSpawn")]
    internal static class ComplexFabricatorOnSpawnPatch
    {
        /// <summary>
        /// Clears <c>duplicantOperated</c> before the fabricator initialises,
        /// so it never creates a fabricate chore.
        /// </summary>
        /// <param name="__instance">The fabricator being spawned.</param>
        internal static void Prefix(ComplexFabricator __instance)
        {
            SafeInvoke.Try("ComplexFabricator automation", delegate
            {
                if (__instance == null) return;

                // Safety guard: Fabricators without a workable component (Kiln, Chlorinator, DataMiner,
                // FoodDehydrator, RubberMaker, Smoker, UraniumCentrifuge) are natively unattended in vanilla.
                // Ensure their duplicantOperated and isManuallyOperated flags remain false so chores are never created.
                if (__instance.GetComponent<ComplexFabricatorWorkable>() == null)
                {
                    __instance.duplicantOperated = false;
                    BuildingComplete bld = __instance.GetComponent<BuildingComplete>();
                    if (bld != null)
                    {
                        bld.isManuallyOperated = false;
                    }
                    return;
                }

                KPrefabID prefabID = __instance.GetComponent<KPrefabID>();
                if (prefabID == null || !AutoMachineOptions.IsEnabledFor(__instance.gameObject, prefabID.PrefabTag.Name))
                {
                    return;
                }

                __instance.duplicantOperated = false;

                BuildingComplete building = __instance.GetComponent<BuildingComplete>();
                if (building != null)
                {
                    building.isManuallyOperated = false;
                }

                Log.Verbose("Automated fabricator " + prefabID.PrefabTag.Name);
            });
        }
    }

    /// <summary>
    /// Self-healing chore creation for manual mode: if a ComplexFabricator is in manual mode
    /// (duplicantOperated == true) and has a working order waiting, ensure the WorkChore is created
    /// so Duplicants are immediately summoned to operate the machine.
    /// </summary>
    [HarmonyPatch(typeof(ComplexFabricator), "Sim1000ms")]
    internal static class ComplexFabricatorSim1000msPatch
    {
        private static readonly System.Reflection.MethodInfo UpdateChoreMethod =
            AccessTools.Method(typeof(ComplexFabricator), "UpdateChore");
        private static readonly System.Reflection.FieldInfo ChoreField =
            AccessTools.Field(typeof(ComplexFabricator), "chore");

        internal static void Postfix(ComplexFabricator __instance)
        {
            if (__instance == null) return;

            // Only run for machines with a valid workable component
            if (__instance.duplicantOperated && __instance.GetComponent<ComplexFabricatorWorkable>() != null && __instance.CurrentWorkingOrder != null)
            {
                SafeInvoke.Try("ComplexFabricatorSim1000msPatch", delegate
                {
                    Operational op = __instance.GetComponent<Operational>();
                    if (op != null && op.IsOperational)
                    {
                        Chore currentChore = ChoreField != null ? ChoreField.GetValue(__instance) as Chore : null;
                        if (currentChore == null && UpdateChoreMethod != null)
                        {
                            UpdateChoreMethod.Invoke(__instance, null);
                        }
                    }
                });
            }
        }
    }
}