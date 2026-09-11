// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System;
using System.Reflection;
using AutoMachineRebuilt.Util;
using HarmonyLib;
using UnityEngine;

namespace AutoMachineRebuilt.Patches
{
    /// <summary>
    /// Compatibility shim for SymbolOverrideController and SymbolOverrideControllerUtil.
    /// Fixes a fatal assertion failure when loading save games containing modded buildings or entities
    /// (e.g. Ronivan's Chemical Processing, metallurgy, nuclear reactors, etc.):
    /// "Assert failed: SymbolOverrideController requires usingNewSymbolOverrideSystem to be set to true."
    ///
    /// Root Cause:
    /// 1. usingNewSymbolOverrideSystem is not serialized by Klei's KSerialization.
    ///    When SaveLoadRoot deserializes saved entities, instantiated clones default to usingNewSymbolOverrideSystem = false.
    /// 2. When an entity is active during AddToPrefab, prefab.AddComponent triggers Awake/OnPrefabInit immediately,
    ///    before usingNewSymbolOverrideSystem is set to true.
    ///
    /// Solution:
    /// A Harmony prefix on SymbolOverrideController.OnPrefabInit and SymbolOverrideControllerUtil.AddToPrefab
    /// auto-heals usingNewSymbolOverrideSystem to true before the assertion is evaluated.
    /// </summary>
    internal static class SymbolOverrideControllerCompatibility
    {
        private static bool applied = false;
        private static readonly object patchLock = new object();

        public static void Apply(Harmony harmony = null)
        {
            if (applied) return;

            lock (patchLock)
            {
                if (applied) return;

                SafeInvoke.Try("SymbolOverrideController compatibility check", delegate
                {
                    Harmony h = harmony ?? new Harmony("AutoMachineRebuilt.SymbolOverrideControllerCompatibility");

                    // 1. Patch SymbolOverrideController.OnPrefabInit
                    MethodInfo onPrefabInit = AccessTools.Method(typeof(SymbolOverrideController), "OnPrefabInit");
                    MethodInfo prefixOnPrefabInit = AccessTools.Method(typeof(SymbolOverrideControllerCompatibility), nameof(OnPrefabInit_Prefix));
                    if (onPrefabInit != null && prefixOnPrefabInit != null)
                    {
                        h.Patch(onPrefabInit, prefix: new HarmonyMethod(prefixOnPrefabInit));
                    }

                    // 2. Patch SymbolOverrideControllerUtil.AddToPrefab
                    MethodInfo addToPrefab = AccessTools.Method(typeof(SymbolOverrideControllerUtil), "AddToPrefab", new Type[] { typeof(GameObject) });
                    MethodInfo prefixAddToPrefab = AccessTools.Method(typeof(SymbolOverrideControllerCompatibility), nameof(AddToPrefab_Prefix));
                    if (addToPrefab != null && prefixAddToPrefab != null)
                    {
                        h.Patch(addToPrefab, prefix: new HarmonyMethod(prefixAddToPrefab));
                    }

                    applied = true;
                    Log.Info("Applied SymbolOverrideController auto-healing save/load safety patch.");
                });
            }
        }

        public static void OnPrefabInit_Prefix(SymbolOverrideController __instance)
        {
            if (__instance == null) return;

            try
            {
                KBatchedAnimController kbac = __instance.GetComponent<KBatchedAnimController>();
                if (kbac == null)
                {
                    kbac = __instance.gameObject.AddComponent<KBatchedAnimController>();
                }
                if (kbac != null && !kbac.usingNewSymbolOverrideSystem)
                {
                    kbac.usingNewSymbolOverrideSystem = true;
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Error in SymbolOverrideController OnPrefabInit prefix: " + ex.Message);
            }
        }

        public static void AddToPrefab_Prefix(GameObject prefab)
        {
            if (prefab == null) return;

            try
            {
                KBatchedAnimController kbac = prefab.GetComponent<KBatchedAnimController>();
                if (kbac != null && !kbac.usingNewSymbolOverrideSystem)
                {
                    kbac.usingNewSymbolOverrideSystem = true;
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Error in SymbolOverrideControllerUtil AddToPrefab prefix: " + ex.Message);
            }
        }
    }
}
