// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System;
using AutoMachineRebuilt.Config;
using HarmonyLib;
using UnityEngine;

namespace AutoMachineRebuilt.Patches
{
    /// <summary>
    /// Implements automated liquid retrieval from Liquid Reservoirs with separate
    /// controls for Duplicants and Auto-Sweepers (Solid Transfer Arms).
    /// Fully compatible with Empty Storage mods without attaching DropAllWorkable.
    /// </summary>
    internal static class LiquidReservoirFetchPatches
    {
        /// <summary>
        /// Checks if a Storage component belongs to a Liquid Reservoir building.
        /// </summary>
        public static bool IsLiquidReservoirStorage(Storage storage)
        {
            if (storage == null) return false;
            KPrefabID kpid = storage.GetComponent<KPrefabID>();
            if (kpid != null)
            {
                string name = kpid.PrefabTag.Name;
                if (name == "LiquidReservoir" || name == LiquidReservoirConfig.ID)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Configures Liquid Reservoir building template to allow item removal
        /// when either Duplicant or Auto-Sweeper fetching is enabled.
        /// </summary>
        [HarmonyPatch(typeof(LiquidReservoirConfig), "ConfigureBuildingTemplate")]
        internal static class LiquidReservoirConfig_ConfigureBuildingTemplate_Patch
        {
            public static void Postfix(GameObject go)
            {
                if (go == null) return;
                var options = AutoMachineOptions.Instance;
                bool enableRemoval = (options == null) || options.EnableAllAutomation ||
                                     options.LiquidReservoirDuplicantFetch || options.LiquidReservoirAutoSweeperFetch;

                Storage storage = go.GetComponent<Storage>();
                if (storage != null)
                {
                    storage.allowItemRemoval = enableRemoval;
                }
            }
        }

        /// <summary>
        /// Synchronizes allowItemRemoval on Liquid Reservoir storage when spawned or loaded.
        /// </summary>
        [HarmonyPatch(typeof(Storage), "OnSpawn")]
        internal static class Storage_OnSpawn_LiquidReservoir_Patch
        {
            public static void Postfix(Storage __instance)
            {
                if (__instance == null || !IsLiquidReservoirStorage(__instance)) return;

                var options = AutoMachineOptions.Instance;
                bool enableRemoval = (options == null) || options.EnableAllAutomation ||
                                     options.LiquidReservoirDuplicantFetch || options.LiquidReservoirAutoSweeperFetch;

                __instance.allowItemRemoval = enableRemoval;
            }
        }

        /// <summary>
        /// Filters fetch errands from Liquid Reservoir: properly distinguishes Duplicant errands
        /// and Auto-Sweeper errands (including with Zoned Solid Transfer Arm).
        /// </summary>
        [HarmonyPatch(typeof(FetchManager), "IsFetchablePickup", new Type[] { typeof(Pickupable), typeof(FetchChore), typeof(Storage) })]
        internal static class FetchManager_IsFetchablePickup_Patch
        {
            public static void Postfix(Pickupable pickup, FetchChore chore, Storage destination, ref bool __result)
            {
                if (!__result || pickup == null) return;

                Storage storage = pickup.storage;
                if (storage != null && IsLiquidReservoirStorage(storage))
                {
                    var options = AutoMachineOptions.Instance;
                    if (options == null || options.EnableAllAutomation) return;

                    // If both Duplicant and Auto-Sweeper fetching are disabled, block all retrieval
                    if (!options.LiquidReservoirDuplicantFetch && !options.LiquidReservoirAutoSweeperFetch)
                    {
                        __result = false;
                        return;
                    }

                    // Differentiate between Duplicant fetch and Auto-Sweeper fetch
                    bool isSweeper = (chore != null && chore.driver != null && chore.driver.GetComponent<SolidTransferArm>() != null);
                    bool isDupe = (chore != null && chore.driver != null && chore.driver.GetComponent<MinionIdentity>() != null);

                    if (isSweeper && !options.LiquidReservoirAutoSweeperFetch)
                    {
                        __result = false;
                    }
                    else if (isDupe && !options.LiquidReservoirDuplicantFetch)
                    {
                        __result = false;
                    }
                    else if (!isSweeper && !isDupe && !options.LiquidReservoirDuplicantFetch)
                    {
                        // Background Duplicant chore generation
                        __result = false;
                    }
                }
            }
        }

        /// <summary>
        /// Filters Auto-Sweeper pickup interest: if an item is stored inside a Liquid Reservoir,
        /// only allow Auto-Sweepers to fetch it if LiquidReservoirAutoSweeperFetch or EnableAllAutomation is on.
        /// </summary>
        [HarmonyPatch(typeof(SolidTransferArm), "IsPickupableRelevantToMyInterests")]
        internal static class SolidTransferArm_IsPickupableRelevantToMyInterests_Patch
        {
            public static void Postfix(KPrefabID prefabID, int storage_cell, ref bool __result)
            {
                if (!__result || prefabID == null) return;

                var options = AutoMachineOptions.Instance;
                if (options != null && !options.EnableAllAutomation && !options.LiquidReservoirAutoSweeperFetch)
                {
                    Pickupable p = prefabID.GetComponent<Pickupable>();
                    if (p != null && p.storage != null && IsLiquidReservoirStorage(p.storage))
                    {
                        __result = false;
                    }
                }
            }
        }
    }
}
