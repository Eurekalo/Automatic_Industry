// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System;
using System.Reflection;
using HarmonyLib;
using PeterHan.PLib.UI;
using UnityEngine;

namespace AutoMachineRebuilt.Patches
{
    /// <summary>
    /// Harmony patch for PLib's OptionsDialog to prevent layout collapse and control clipping.
    /// Ensures adequate dialog dimensions (1020x720) so that bilingual / long option labels
    /// never push checkboxes or sliders outside the visible window boundary.
    /// </summary>
    [HarmonyPatch]
    public static class OptionsDialogLayoutFixPatch
    {
        public static bool Prepare()
        {
            return TargetMethod() != null;
        }

        public static MethodBase TargetMethod()
        {
            Type optionsDialogType = AccessTools.TypeByName("PeterHan.PLib.Options.OptionsDialog");
            if (optionsDialogType == null) return null;

            return AccessTools.Method(optionsDialogType, "AddModInfoScreen");
        }

        public static void Postfix(object __instance, object optionsDialog)
        {
            try
            {
                if (optionsDialog is PDialog pDialog)
                {
                    // Ensure wide and comfortable dialog dimensions so bilingual strings don't push checkboxes out
                    pDialog.Size = new Vector2(Mathf.Max(pDialog.Size.x, 1020f), Mathf.Max(pDialog.Size.y, 720f));
                    pDialog.MaxSize = new Vector2(Mathf.Max(pDialog.MaxSize.x, 1180f), Mathf.Max(pDialog.MaxSize.y, 850f));
                }
            }
            catch
            {
                // Defensive
            }
        }
    }
}
