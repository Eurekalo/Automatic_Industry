// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System;
using System.Collections.Generic;
using AutoMachineRebuilt.Components;
using AutoMachineRebuilt.Config;
using HarmonyLib;

namespace AutoMachineRebuilt.Patches
{
    /// <summary>
    /// Harmony patches for the Spice Grinder (<see cref="SpiceGrinder.StatesInstance"/>)
    /// to support universal multi-ingredient delivery for Auto-Sweepers and Duplicants.
    ///
    /// In vanilla, ingredient fetch chores use <c>Db.Get().ChoreTypes.CookFetch</c>,
    /// which prevents Auto-Sweepers from delivering ingredients like Salt and Seeds.
    /// These patches redirect fetch chores to <c>FabricateFetch</c> and provide
    /// continuous advance stocking and self-healing for interrupted chores.
    /// </summary>
    public static class SpiceGrinderPatches
    {
        /// <summary>
        /// Intercepts <c>SpiceGrinder.StatesInstance.CreateFetchChore</c> to use
        /// <c>FabricateFetch</c> instead of <c>CookFetch</c> when automated.
        /// Also attaches completion and cancellation handlers to prevent
        /// the vanilla permanent deadlock where <c>HasOpenFetches</c> stays true.
        /// </summary>
        [HarmonyPatch(typeof(SpiceGrinder.StatesInstance), "CreateFetchChore", new Type[] { typeof(HashSet<Tag>), typeof(float) })]
        public static class SpiceGrinder_CreateFetchChore_Patch
        {
            public static bool Prefix(SpiceGrinder.StatesInstance __instance, HashSet<Tag> ingredients, float amount, ref FetchChore __result)
            {
                if (__instance != null && AutoMachineOptions.IsEnabledFor(__instance.gameObject, "SPICEGRINDER"))
                {
                    FetchChore chore = AutoSpiceGrinderController.CreateIngredientFetchChore(__instance, ingredients, amount);
                    if (chore != null)
                    {
                        __result = chore;
                        return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Hooks <c>SpiceGrinder.StatesInstance.OnOptionSelected</c> to immediately
        /// configure storage headroom, expand filter tags, and start pre-stocking
        /// ingredients before food even arrives.
        /// </summary>
        [HarmonyPatch(typeof(SpiceGrinder.StatesInstance), "OnOptionSelected")]
        public static class SpiceGrinder_OnOptionSelected_Patch
        {
            public static void Postfix(SpiceGrinder.StatesInstance __instance, SpiceGrinder.Option spiceOption)
            {
                if (__instance != null && AutoMachineOptions.IsEnabledFor(__instance.gameObject, "SPICEGRINDER"))
                {
                    AutoSpiceGrinderController controller = __instance.GetComponent<AutoSpiceGrinderController>();
                    if (controller != null)
                    {
                        controller.OnOptionChanged(spiceOption);
                    }
                }
            }
        }
    }
}
