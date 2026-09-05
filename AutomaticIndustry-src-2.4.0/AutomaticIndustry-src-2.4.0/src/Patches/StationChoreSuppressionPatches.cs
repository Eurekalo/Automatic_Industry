// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System.Reflection;
using AutoMachineRebuilt.Config;
using HarmonyLib;

namespace AutoMachineRebuilt.Patches
{
    /// <summary>
    /// Comprehensive chore suppression patches across all specialized station and manufacturing buildings:
    /// SpiceGrinder, ResearchCenter, NuclearResearchCenter, GeneticAnalysisStation, GeoTuner, Telescope, ClusterTelescope, FoodSmoker.
    ///
    /// Rather than returning null from CreateChore callbacks (which crashes GameStateMachine.State.SetupChore
    /// with a NullReferenceException when hooking state transitions), these patches attach an Always-False
    /// precondition to the chore.
    ///
    /// This guarantees:
    /// 1. The StateMachine receives a valid non-null Chore and hooks lifecycle transitions without crashing.
    /// 2. Duplicant chore assignment completely ignores the chore (no Duplicant will ever accept or approach it).
    /// 3. Standard Duplicant operation is cleanly preserved when set to manual mode.
    /// </summary>
    internal static class StationChoreSuppressionPatches
    {
        private static readonly Chore.Precondition DisabledByModPrecondition = new Chore.Precondition
        {
            id = "AutomaticIndustry_StationDisabledByMod",
            description = "Automated by Automatic Industry",
            fn = (ref Chore.Precondition.Context context, object data) => false
        };

        private static readonly FieldInfo NuclearChoreField =
            typeof(NuclearResearchCenter.StatesInstance).GetField("chore", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>Spice Grinder: Suppress Cook chore in ready state when automated.</summary>
        [HarmonyPatch(typeof(SpiceGrinder), "CreateChore")]
        public static class SpiceGrinder_CreateChore_Patch
        {
            public static void Postfix(SpiceGrinder.StatesInstance smi, ref Chore __result)
            {
                if (__result != null && smi != null && AutoMachineOptions.IsEnabledFor(smi.gameObject, "SPICEGRINDER"))
                {
                    __result.AddPrecondition(DisabledByModPrecondition, null);
                }
            }
        }

        /// <summary>Research Stations (Basic, Advanced, Cosmic, DLC1): Suppress Research chores when automated.</summary>
        [HarmonyPatch(typeof(ResearchCenter), "CreateChore")]
        public static class ResearchCenter_CreateChore_Patch
        {
            public static void Postfix(ResearchCenter __instance, ref Chore __result)
            {
                if (__result != null && __instance != null)
                {
                    KPrefabID prefabID = __instance.GetComponent<KPrefabID>();
                    string optionKey = prefabID != null ? prefabID.PrefabTag.Name : null;
                    if (!string.IsNullOrEmpty(optionKey) && AutoMachineOptions.IsEnabledFor(__instance.gameObject, optionKey))
                    {
                        __result.AddPrecondition(DisabledByModPrecondition, null);
                    }
                }
            }
        }

        /// <summary>Nuclear Research Center (Materials Study Terminal): Suppress Research chore when automated.</summary>
        [HarmonyPatch(typeof(NuclearResearchCenter.StatesInstance), nameof(NuclearResearchCenter.StatesInstance.CreateChore))]
        public static class NuclearResearchCenter_CreateChore_Patch
        {
            public static void Postfix(NuclearResearchCenter.StatesInstance __instance)
            {
                if (__instance != null && AutoMachineOptions.IsEnabledFor(__instance.gameObject, "NUCLEARRESEARCHCENTER"))
                {
                    Chore chore = NuclearChoreField?.GetValue(__instance) as Chore;
                    chore?.AddPrecondition(DisabledByModPrecondition, null);
                }
            }
        }

        /// <summary>Genetic Analysis Station: Suppress Seed Analysis chore when automated.</summary>
        [HarmonyPatch(typeof(GeneticAnalysisStation), "CreateChore")]
        public static class GeneticAnalysisStation_CreateChore_Patch
        {
            public static void Postfix(GeneticAnalysisStation.StatesInstance smi, ref Chore __result)
            {
                if (__result != null && smi != null && AutoMachineOptions.IsEnabledFor(smi.gameObject, "GENETICANALYSISSTATION"))
                {
                    __result.AddPrecondition(DisabledByModPrecondition, null);
                }
            }
        }

        /// <summary>GeoTuner: Suppress Geyser Research chore when automated.</summary>
        [HarmonyPatch(typeof(GeoTuner), "CreateResearchChore")]
        public static class GeoTuner_CreateResearchChore_Patch
        {
            public static void Postfix(GeoTuner.Instance smi, ref Chore __result)
            {
                if (__result != null && smi != null && AutoMachineOptions.IsEnabledFor(smi.gameObject, "GEOTUNER"))
                {
                    __result.AddPrecondition(DisabledByModPrecondition, null);
                }
            }
        }

        /// <summary>Telescope: Suppress space analysis chore when automated.</summary>
        [HarmonyPatch(typeof(Telescope), "CreateChore")]
        public static class Telescope_CreateChore_Patch
        {
            public static void Postfix(Telescope __instance, ref Chore __result)
            {
                if (__result != null && __instance != null && AutoMachineOptions.IsEnabledFor(__instance.gameObject, "TELESCOPE"))
                {
                    __result.AddPrecondition(DisabledByModPrecondition, null);
                }
            }
        }

        /// <summary>Cluster Telescope & Enclosed Telescope: Suppress reveal and identify chores when automated.</summary>
        [HarmonyPatch(typeof(ClusterTelescope.Instance), nameof(ClusterTelescope.Instance.CreateRevealTileChore))]
        public static class ClusterTelescope_CreateRevealTileChore_Patch
        {
            public static void Postfix(ClusterTelescope.Instance __instance, ref Chore __result)
            {
                if (__result != null && __instance != null)
                {
                    KPrefabID prefabID = __instance.GetComponent<KPrefabID>();
                    string optionKey = prefabID != null ? prefabID.PrefabTag.Name : "CLUSTERTELESCOPE";
                    if (AutoMachineOptions.IsEnabledFor(__instance.gameObject, optionKey))
                    {
                        __result.AddPrecondition(DisabledByModPrecondition, null);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(ClusterTelescope.Instance), nameof(ClusterTelescope.Instance.CreateIdentifyMeteorChore))]
        public static class ClusterTelescope_CreateIdentifyMeteorChore_Patch
        {
            public static void Postfix(ClusterTelescope.Instance __instance, ref Chore __result)
            {
                if (__result != null && __instance != null)
                {
                    KPrefabID prefabID = __instance.GetComponent<KPrefabID>();
                    string optionKey = prefabID != null ? prefabID.PrefabTag.Name : "CLUSTERTELESCOPE";
                    if (AutoMachineOptions.IsEnabledFor(__instance.gameObject, optionKey))
                    {
                        __result.AddPrecondition(DisabledByModPrecondition, null);
                    }
                }
            }
        }

        /// <summary>Food Smoker: Suppress empty chore when automated.</summary>
        [HarmonyPatch(typeof(FoodSmoker), "CreateChore")]
        public static class FoodSmoker_CreateChore_Patch
        {
            public static void Postfix(FoodSmoker.StatesInstance smi, ref Chore __result)
            {
                if (__result != null && smi != null && AutoMachineOptions.IsEnabledFor(smi.gameObject, "FoodSmoker"))
                {
                    __result.AddPrecondition(DisabledByModPrecondition, null);
                }
            }
        }
    }
}
