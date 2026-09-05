// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using AutoMachineRebuilt.Util;
using HarmonyLib;

namespace AutoMachineRebuilt.Patches
{
    /// <summary>
    /// Defensive null-safety patches for NuclearResearchCenter (Materials Study Terminal).
    /// Prevents null dereference crashes when research completes (e.g. 19/20 -> 20/20)
    /// or when Research Queue advances queued tech, triggering ready.Exit -> smi.DestroyChore().
    /// </summary>
    [HarmonyPatch(typeof(NuclearResearchCenter.StatesInstance), nameof(NuclearResearchCenter.StatesInstance.DestroyChore))]
    public static class NuclearResearchCenterPatches
    {
        public static bool Prefix(NuclearResearchCenter.StatesInstance __instance)
        {
            if (__instance == null) return false;

            SafeInvoke.Try("Safe NuclearResearchCenter.DestroyChore", delegate
            {
                var chore = StateMachineUtil.Field(__instance, "chore") as Chore;
                if (chore != null)
                {
                    chore.Cancel("destroy me!");
                    StateMachineUtil.SetField(__instance, "chore", null);
                }
            });

            return false; // Safely handled, prevents NullReferenceException if chore is null
        }
    }
}
