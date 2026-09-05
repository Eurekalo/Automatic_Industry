// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System;
using System.Reflection;
using AutoMachineRebuilt.Util;
using HarmonyLib;

namespace AutoMachineRebuilt.Patches
{
    /// <summary>
    /// Compatibility shim for the "No Manual Delivery" mod (Steam ID 2047308624).
    ///
    /// "No Manual Delivery" attaches Automatable2 to fabricators, stations and storages,
    /// and patches FetchChore constructors and SolidTransferArm lifecycle methods to query
    /// TransferArmGroupProber.Get().
    ///
    /// However, TransferArmGroupProber.Instance is a singleton that is only instantiated
    /// when Game.Instance spawns and only if HoldMode is enabled. Calling TransferArmGroupProber.Get()
    /// during save loading, pre-game initialization, or when HoldMode is disabled returns null,
    /// causing immediate NullReferenceException crashes when any building creates a FetchChore or
    /// when SolidTransferArm cleans up or updates.
    ///
    /// This shim intercepts TransferArmGroupProber.Get() and provides a safe fallback to
    /// MinionGroupProber.Get() whenever the singleton instance is null, completely eliminating
    /// the crash.
    /// </summary>
    internal static class NoManualDeliveryCompatibility
    {
        private static bool patched;

        /// <summary>
        /// Dynamically patches TransferArmGroupProber.Get() if NoManualDelivery is loaded.
        /// Can be safely invoked multiple times (e.g. OnLoad and Db.Initialize).
        /// </summary>
        public static void Patch(Harmony harmony)
        {
            if (patched || harmony == null)
            {
                return;
            }

            SafeInvoke.Try("NoManualDelivery TransferArmGroupProber compatibility patch", delegate
            {
                Type proberType = AccessTools.TypeByName("NoManualDelivery.TransferArmGroupProber");
                if (proberType == null)
                {
                    return;
                }

                MethodInfo getMethod = AccessTools.Method(proberType, "Get");
                if (getMethod == null)
                {
                    return;
                }

                MethodInfo postfix = AccessTools.Method(typeof(NoManualDeliveryCompatibility), nameof(Get_Postfix));
                if (postfix != null)
                {
                    harmony.Patch(getMethod, postfix: new HarmonyMethod(postfix));
                    patched = true;
                    Log.Info("NoManualDelivery detected: TransferArmGroupProber null-safety compatibility patch successfully applied.");
                }
            });
        }

        private static MinionGroupProber fallbackProber;

        /// <summary>
        /// Provides a safe non-null fallback prober with an allocated cells array
        /// in the rare event that MinionGroupProber.Get() is also null (e.g. before Game.OnPrefabInit).
        /// </summary>
        private static MinionGroupProber GetFallbackProber()
        {
            if (fallbackProber == null)
            {
                try
                {
                    UnityEngine.GameObject go = new UnityEngine.GameObject("NoManualDelivery_FallbackProber");
                    UnityEngine.Object.DontDestroyOnLoad(go);
                    fallbackProber = go.AddComponent<MinionGroupProber>();
                    FieldInfo cellsField = AccessTools.Field(typeof(MinionGroupProber), "cells");
                    if (cellsField != null)
                    {
                        int count = Grid.CellCount > 0 ? Grid.CellCount : 1;
                        cellsField.SetValue(fallbackProber, new int[count]);
                    }
                }
                catch (Exception ex)
                {
                    Log.Verbose("Could not create fallback prober: " + ex.Message);
                }
            }
            return fallbackProber;
        }

        /// <summary>
        /// Postfix on TransferArmGroupProber.Get() ensuring a non-null return value.
        /// </summary>
        public static void Get_Postfix(ref MinionGroupProber __result)
        {
            if (__result == null)
            {
                __result = MinionGroupProber.Get() ?? GetFallbackProber();
            }
        }
    }
}
