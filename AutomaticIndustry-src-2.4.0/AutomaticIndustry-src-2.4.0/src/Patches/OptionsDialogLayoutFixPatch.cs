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
    /// Applied manually in OnLoad to safely guard against upstream PLib version discrepancies.
    /// </summary>
    public static class OptionsDialogLayoutFixPatch
    {
        public static MethodBase TargetMethod()
        {
            Type optionsDialogType = AccessTools.TypeByName("PeterHan.PLib.Options.OptionsDialog");
            if (optionsDialogType == null) return null;

            return AccessTools.Method(optionsDialogType, "AddModInfoScreen");
        }

        public static void Apply(Harmony harmony)
        {
            try
            {
                MethodBase target = TargetMethod();
                if (target == null || harmony == null) return;

                MethodInfo postfix = typeof(OptionsDialogLayoutFixPatch).GetMethod(nameof(Postfix), BindingFlags.Public | BindingFlags.Static);
                if (postfix != null)
                {
                    harmony.Patch(target, postfix: new HarmonyMethod(postfix));
                }
            }
            catch (Exception ex)
            {
                Util.Log.Warn("OptionsDialogLayoutFixPatch optional UI patch skipped: " + ex.Message);
            }
        }

        public static void Postfix(object __instance, object[] __args)
        {
            try
            {
                if (__args != null)
                {
                    foreach (object arg in __args)
                    {
                        if (arg is PDialog pDialog)
                        {
                            // Ensure wide and comfortable dialog dimensions (780x620) so bilingual / CJK strings don't push checkboxes out
                            pDialog.Size = new Vector2(Mathf.Max(pDialog.Size.x, 780f), Mathf.Max(pDialog.Size.y, 620f));
                            pDialog.MaxSize = new Vector2(Mathf.Max(pDialog.MaxSize.x, 920f), Mathf.Max(pDialog.MaxSize.y, 850f));
                            return;
                        }
                    }
                }
            }
            catch
            {
                // Defensive
            }
        }
    }
}
