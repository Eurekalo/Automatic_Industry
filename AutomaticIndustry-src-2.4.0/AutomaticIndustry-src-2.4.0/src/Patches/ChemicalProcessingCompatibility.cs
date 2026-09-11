// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System;
using System.Reflection;
using AutoMachineRebuilt.Util;
using HarmonyLib;
using UnityEngine;

namespace AutoMachineRebuilt.Patches
{
    /// <summary>
    /// Compatibility shim for RonivansLegacy_ChemicalProcessing (Chemical Processing).
    /// Fixes a fatal NullReferenceException when opening the Building Configuration Editor
    /// from in-game options menus or Mod Menu while inside an active game session.
    ///
    /// Root Cause:
    /// In BuildingEditor_MainScreen.ShowWindow, the mod calls:
    ///   Util.KInstantiateUI(..., ((Component)FrontEndManager.Instance).gameObject, true)
    /// FrontEndManager only exists in the main menu. In-game, FrontEndManager.Instance is null,
    /// causing an instant NullReferenceException crash.
    ///
    /// Fix:
    /// Intercept ShowWindow. If in-game (FrontEndManager.Instance == null), resolve a safe UI parent
    /// (GameScreenManager.Instance.ssOverlayCanvas or PauseScreen.Instance or Global.Instance.globalCanvas)
    /// and safely instantiate the window, bypassing the unshielded access.
    /// </summary>
    internal static class ChemicalProcessingCompatibility
    {
        private static bool applied = false;
        private static readonly object patchLock = new object();

        private const string TargetTypeName = "RonivansLegacy_ChemicalProcessing.Content.Scripts.UI.BuildingEditor_MainScreen";
        private const string ModAssetsTypeName = "RonivansLegacy_ChemicalProcessing.ModAssets";

        public static void Apply(Harmony harmony)
        {
            if (applied) return;

            lock (patchLock)
            {
                if (applied) return;

                SafeInvoke.Try("ChemicalProcessing compatibility check", delegate
                {
                    Type targetType = AccessTools.TypeByName(TargetTypeName);
                    if (targetType == null) return;

                    MethodInfo targetMethod = AccessTools.Method(targetType, "ShowWindow");
                    if (targetMethod == null) return;

                    Harmony h = harmony ?? new Harmony("AutoMachineRebuilt.ChemicalProcessingCompatibility");
                    MethodInfo prefixMethod = AccessTools.Method(typeof(ChemicalProcessingCompatibility), nameof(ShowWindow_Prefix));

                    if (prefixMethod != null)
                    {
                        h.Patch(targetMethod, prefix: new HarmonyMethod(prefixMethod));
                        applied = true;
                        Log.Info("Applied Chemical Processing in-game BuildingEditor ShowWindow compatibility patch.");
                    }
                });
            }
        }

        public static bool ShowWindow_Prefix(object source)
        {
            try
            {
                Type screenType = AccessTools.TypeByName(TargetTypeName);
                if (screenType == null) return true;

                FieldInfo instanceField = AccessTools.Field(screenType, "Instance");
                if (instanceField == null) return true;

                object instance = instanceField.GetValue(null);
                bool needsCreation = instance == null || (instance is UnityEngine.Object unityObj && unityObj == null);

                if (needsCreation)
                {
                    // 1. Resolve safe UI parent GameObject
                    GameObject parent = null;
                    if (FrontEndManager.Instance != null && FrontEndManager.Instance.gameObject != null)
                    {
                        parent = FrontEndManager.Instance.gameObject;
                    }
                    else if (GameScreenManager.Instance != null && GameScreenManager.Instance.ssOverlayCanvas != null)
                    {
                        parent = GameScreenManager.Instance.ssOverlayCanvas;
                    }
                    else if (PauseScreen.Instance != null && PauseScreen.Instance.gameObject != null)
                    {
                        parent = PauseScreen.Instance.gameObject;
                    }
                    else if (Global.Instance != null && Global.Instance.globalCanvas != null)
                    {
                        parent = Global.Instance.globalCanvas;
                    }

                    if (parent == null)
                    {
                        return true; // Fallback to original method
                    }

                    // 2. Fetch BuildingEditorWindowPrefab from ModAssets
                    Type modAssetsType = AccessTools.TypeByName(ModAssetsTypeName);
                    if (modAssetsType == null) return true;

                    FieldInfo prefabField = AccessTools.Field(modAssetsType, "BuildingEditorWindowPrefab");
                    GameObject prefab = prefabField != null ? prefabField.GetValue(null) as GameObject : null;

                    if (prefab == null) return true;

                    // 3. Instantiate window under safe parent
                    GameObject windowGO = global::Util.KInstantiateUI(prefab, parent, true);
                    if (windowGO == null) return true;

                    Component screenComp = windowGO.GetComponent(screenType) ?? windowGO.AddComponent(screenType);
                    instanceField.SetValue(null, screenComp);

                    MethodInfo initMethod = AccessTools.Method(screenType, "Init");
                    initMethod?.Invoke(screenComp, null);

                    windowGO.name = "AIO_BuildingEditor_MainScreen";
                    instance = screenComp;
                }

                // 4. Show and configure
                if (instance != null && (instance is UnityEngine.Object uo ? uo != null : true))
                {
                    MethodInfo showMethod = AccessTools.Method(screenType, "Show", new Type[] { typeof(bool) });
                    showMethod?.Invoke(instance, new object[] { true });

                    MethodInfo openedFromMethod = AccessTools.Method(screenType, "OpenedFrom");
                    openedFromMethod?.Invoke(instance, new object[] { source });

                    // Successfully presented UI using safe parent! Skip original unshielded ShowWindow.
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Error in ChemicalProcessing ShowWindow safe prefix: " + ex.Message);
            }

            return true;
        }
    }
}
