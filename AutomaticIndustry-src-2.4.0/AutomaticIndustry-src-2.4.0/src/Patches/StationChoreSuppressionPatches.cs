// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System.Reflection;
using AutoMachineRebuilt.Config;
using HarmonyLib;

namespace AutoMachineRebuilt.Patches
{
    /// <summary>
    /// Comprehensive chore suppression patches across all specialized station and manufacturing buildings:
    /// SpiceGrinder, ResearchCenter, NuclearResearchCenter, GeneticAnalysisStation, GeoTuner, Telescope, ClusterTelescope, FoodSmoker,
    /// RanchStation (Grooming, Shearing, Milking, land and aquatic), and Oil Refinery.
    ///
    /// Rather than returning null from CreateChore callbacks (which crashes GameStateMachine.State.SetupChore
    /// with a NullReferenceException when hooking state transitions) or cancelling chores in a polling loop
    /// (which causes state machine failure transitions, re-entrant chore creation loops, and ReportManager memory leaks),
    /// these patches attach an intelligent precondition to the chore.
    ///
    /// This guarantees:
    /// 1. The StateMachine receives a valid non-null Chore and hooks lifecycle transitions without crashing or looping.
    /// 2. Duplicants completely ignore the chore when automated (zero Duplicants will accept or approach it).
    /// 3. Standard Duplicant operation is cleanly preserved when set to manual mode.
    /// 4. Zero ReportManager note spam, zero re-entrant chore allocations, zero memory leak.
    /// </summary>
    internal static class StationChoreSuppressionPatches
    {
        internal static readonly Chore.Precondition DisabledByModPrecondition = new Chore.Precondition
        {
            id = "AutomaticIndustry_DisabledByMod",
            description = "Automated by Automatic Industry",
            fn = (ref Chore.Precondition.Context context, object data) =>
            {
                if (context.chore != null && context.chore.gameObject != null)
                {
                    KPrefabID prefabID = context.chore.gameObject.GetComponent<KPrefabID>();
                    string optionKey = prefabID != null ? prefabID.PrefabTag.Name : null;
                    if (!string.IsNullOrEmpty(optionKey) && AutoMachineOptions.IsEnabledFor(context.chore.gameObject, optionKey))
                    {
                        return false; // Automated: suppress duplicants
                    }
                }
                return true; // Manual mode: allow duplicants
            }
        };

        private static readonly FieldInfo NuclearChoreField =
            typeof(NuclearResearchCenter.StatesInstance).GetField("chore", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>Spice Grinder: Suppress Cook chore in ready state when automated.</summary>
        [HarmonyPatch(typeof(SpiceGrinder), "CreateChore")]
        public static class SpiceGrinder_CreateChore_Patch
        {
            public static bool Prepare() => AccessTools.Method(typeof(SpiceGrinder), "CreateChore") != null;

            public static void Postfix(SpiceGrinder.StatesInstance smi, ref Chore __result)
            {
                if (__result != null && smi != null)
                {
                    __result.AddPrecondition(DisabledByModPrecondition, null);
                }
            }
        }

        /// <summary>Research Stations (Basic, Advanced, Cosmic, DLC1): Suppress Research chores when automated.</summary>
        [HarmonyPatch(typeof(ResearchCenter), "CreateChore")]
        public static class ResearchCenter_CreateChore_Patch
        {
            public static bool Prepare() => AccessTools.Method(typeof(ResearchCenter), "CreateChore") != null;

            public static void Postfix(ResearchCenter __instance, ref Chore __result)
            {
                if (__result != null && __instance != null)
                {
                    __result.AddPrecondition(DisabledByModPrecondition, null);
                }
            }
        }

        /// <summary>Nuclear Research Center (Materials Study Terminal): Suppress Research chore when automated.</summary>
        [HarmonyPatch(typeof(NuclearResearchCenter.StatesInstance), nameof(NuclearResearchCenter.StatesInstance.CreateChore))]
        public static class NuclearResearchCenter_CreateChore_Patch
        {
            public static bool Prepare() => AccessTools.Method(typeof(NuclearResearchCenter.StatesInstance), nameof(NuclearResearchCenter.StatesInstance.CreateChore)) != null;

            public static void Postfix(NuclearResearchCenter.StatesInstance __instance)
            {
                if (__instance != null)
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
            public static bool Prepare() => AccessTools.Method(typeof(GeneticAnalysisStation), "CreateChore") != null;

            public static void Postfix(GeneticAnalysisStation.StatesInstance smi, ref Chore __result)
            {
                if (__result != null && smi != null)
                {
                    __result.AddPrecondition(DisabledByModPrecondition, null);
                }
            }
        }

        /// <summary>GeoTuner: Suppress Geyser Research chore when automated.</summary>
        [HarmonyPatch(typeof(GeoTuner), "CreateResearchChore")]
        public static class GeoTuner_CreateResearchChore_Patch
        {
            public static bool Prepare() => AccessTools.Method(typeof(GeoTuner), "CreateResearchChore") != null;

            public static void Postfix(GeoTuner.Instance smi, ref Chore __result)
            {
                if (__result != null && smi != null)
                {
                    __result.AddPrecondition(DisabledByModPrecondition, null);
                }
            }
        }

        /// <summary>Telescope: Suppress space analysis chore when automated.</summary>
        [HarmonyPatch(typeof(Telescope), "CreateChore")]
        public static class Telescope_CreateChore_Patch
        {
            public static bool Prepare() => AccessTools.Method(typeof(Telescope), "CreateChore") != null;

            public static void Postfix(Telescope __instance, ref Chore __result)
            {
                if (__result != null && __instance != null)
                {
                    __result.AddPrecondition(DisabledByModPrecondition, null);
                }
            }
        }

        /// <summary>Cluster Telescope & Enclosed Telescope: Suppress reveal and identify chores when automated.</summary>
        [HarmonyPatch(typeof(ClusterTelescope.Instance), nameof(ClusterTelescope.Instance.CreateRevealTileChore))]
        public static class ClusterTelescope_CreateRevealTileChore_Patch
        {
            public static bool Prepare() => AccessTools.Method(typeof(ClusterTelescope.Instance), nameof(ClusterTelescope.Instance.CreateRevealTileChore)) != null;

            public static void Postfix(ClusterTelescope.Instance __instance, ref Chore __result)
            {
                if (__result != null && __instance != null)
                {
                    __result.AddPrecondition(DisabledByModPrecondition, null);
                }
            }
        }

        [HarmonyPatch(typeof(ClusterTelescope.Instance), nameof(ClusterTelescope.Instance.CreateIdentifyMeteorChore))]
        public static class ClusterTelescope_CreateIdentifyMeteorChore_Patch
        {
            public static bool Prepare() => AccessTools.Method(typeof(ClusterTelescope.Instance), nameof(ClusterTelescope.Instance.CreateIdentifyMeteorChore)) != null;

            public static void Postfix(ClusterTelescope.Instance __instance, ref Chore __result)
            {
                if (__result != null && __instance != null)
                {
                    __result.AddPrecondition(DisabledByModPrecondition, null);
                }
            }
        }

        /// <summary>Food Smoker: Suppress empty chore when automated.</summary>
        [HarmonyPatch(typeof(FoodSmoker), "CreateChore")]
        public static class FoodSmoker_CreateChore_Patch
        {
            public static bool Prepare() => AccessTools.Method(typeof(FoodSmoker), "CreateChore") != null;

            public static void Postfix(FoodSmoker.StatesInstance smi, ref Chore __result)
            {
                if (__result != null && smi != null)
                {
                    __result.AddPrecondition(DisabledByModPrecondition, null);
                }
            }
        }

        /// <summary>Ranch Stations (Grooming, Shearing, Milking, land and aquatic): Suppress Ranch chore when automated.</summary>
        [HarmonyPatch(typeof(RanchStation.Instance), nameof(RanchStation.Instance.CreateChore))]
        public static class RanchStation_CreateChore_Patch
        {
            public static bool Prepare() => AccessTools.Method(typeof(RanchStation.Instance), nameof(RanchStation.Instance.CreateChore)) != null;

            public static void Postfix(RanchStation.Instance __instance, ref Chore __result)
            {
                if (__result != null && __instance != null)
                {
                    __result.AddPrecondition(DisabledByModPrecondition, null);
                }
            }
        }

        /// <summary>Oil Refinery: Suppress conversion work chore when automated.</summary>
        [HarmonyPatch(typeof(OilRefinery.States), "SetRemoteChore")]
        public static class OilRefinery_SetRemoteChore_Patch
        {
            public static bool Prepare() => AccessTools.Method(typeof(OilRefinery.States), "SetRemoteChore") != null;

            public static void Postfix(OilRefinery.StatesInstance smi, Chore chore)
            {
                if (chore != null && smi != null)
                {
                    chore.AddPrecondition(DisabledByModPrecondition, null);
                }
            }
        }
    }
}
