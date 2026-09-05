// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Reflection;
using AutoMachineRebuilt.Config;
using AutoMachineRebuilt.Localization;
using HarmonyLib;
using UnityEngine;

namespace AutoMachineRebuilt.Patches
{
    /// <summary>
    /// Harmony patch on UserMenu.AddButton that dynamically intercepts all UserMenu buttons from third-party / community mods
    /// (such as EmptyStorage, ShowRanges, etc.) and appends a color-coded mod source attribution tag to their hover tooltips,
    /// or automatically generates an informative tooltip if the mod did not provide one.
    /// Vanilla game buttons (Assembly-CSharp) are strictly preserved and left unmodified.
    /// </summary>
    [HarmonyPatch(typeof(UserMenu), "AddButton")]
    public static class UserMenu_AddButton_ModAttribution_Patch
    {
        private static readonly HashSet<string> VanillaAssemblyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Assembly-CSharp",
            "Assembly-CSharp-firstpass",
            "UnityEngine",
            "UnityEngine.CoreModule",
            "UnityEngine.UI",
            "UnityEngine.UIModule",
            "mscorlib",
            "System",
            "System.Core",
            "0Harmony",
            "Harmony",
            "Newtonsoft.Json",
            "KSerialization"
        };

        public static void Prefix(GameObject go, KIconButtonMenu.ButtonInfo button, ref float sort_order)
        {
            if (button == null) return;

            // When ModMenu is loaded and active, ModMenu takes precedence and handles universal mod attribution!
            if (IsModMenuActive()) return;

            try
            {
                // 1. Identify which Assembly registered this button
                Assembly assembly = GetButtonAssembly(button);
                if (assembly == null) return;

                string asmName = assembly.GetName().Name;
                if (VanillaAssemblyNames.Contains(asmName))
                {
                    // Vanilla game button: strictly untouched!
                    return;
                }

                // If button is from AutomaticIndustry, it is already handled with building-specific details
                if (asmName.Equals("AutomaticIndustry", StringComparison.OrdinalIgnoreCase) ||
                    asmName.Equals("AutoMachineRebuilt", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                // 2. Prevent duplicate tags
                if (!string.IsNullOrEmpty(button.tooltipText) &&
                    (button.tooltipText.Contains("color=#4BC5FF") ||
                     button.tooltipText.Contains("[Mod:") ||
                     button.tooltipText.Contains("【来源模组：") ||
                     button.tooltipText.Contains("【來源模組：") ||
                     button.tooltipText.Contains("[제공 모德:") ||
                     button.tooltipText.Contains("[제공 모드:") ||
                     button.tooltipText.Contains("【提供MOD：")))
                {
                    return;
                }

                // 3. Resolve friendly Mod Name
                string modName = ResolveModName(assembly);
                if (string.IsNullOrEmpty(modName))
                {
                    modName = asmName;
                }

                // 4. Format mod source attribution tag based on game UI language
                string modTag = GetModSourceTag(modName);

                // 5. Update button tooltip
                if (!string.IsNullOrEmpty(button.tooltipText))
                {
                    button.tooltipText = button.tooltipText + "\n\n" + modTag;
                }
                else
                {
                    // Community mod did not provide a tooltip: create one from button text + mod badge
                    string baseText = !string.IsNullOrEmpty(button.text) ? button.text : modName;
                    button.tooltipText = baseText + "\n\n" + modTag;
                }
            }
            catch
            {
                // Defensive: never break game UserMenu if an unknown mod throws
            }
        }

        private static bool IsModMenuActive()
        {
            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < assemblies.Length; i++)
                {
                    if (assemblies[i].GetName().Name.Equals("ModMenu", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            catch
            {
            }
            return false;
        }

        private static Assembly GetButtonAssembly(KIconButtonMenu.ButtonInfo button)
        {
            if (button.onClick != null && button.onClick.Method != null && button.onClick.Method.DeclaringType != null)
            {
                return button.onClick.Method.DeclaringType.Assembly;
            }
            if (button.onToolTip != null && button.onToolTip.Method != null && button.onToolTip.Method.DeclaringType != null)
            {
                return button.onToolTip.Method.DeclaringType.Assembly;
            }
            if (button.onCreate != null && button.onCreate.Method != null && button.onCreate.Method.DeclaringType != null)
            {
                return button.onCreate.Method.DeclaringType.Assembly;
            }
            return null;
        }

        private static string ResolveModName(Assembly assembly)
        {
            try
            {
                if (Global.Instance != null && Global.Instance.modManager != null && Global.Instance.modManager.mods != null)
                {
                    foreach (var mod in Global.Instance.modManager.mods)
                    {
                        if (mod != null && mod.loaded_mod_data != null && mod.loaded_mod_data.dlls != null)
                        {
                            if (mod.loaded_mod_data.dlls.Contains(assembly))
                            {
                                return !string.IsNullOrEmpty(mod.title) ? mod.title : (!string.IsNullOrEmpty(mod.label.title) ? mod.label.title : mod.label.id);
                            }
                        }
                    }
                }
            }
            catch
            {
            }

            return assembly.GetName().Name;
        }

        private static string GetModSourceTag(string modName)
        {
            UiLanguage lang = OptionTextBinder.Resolve(UiLanguage.Auto);
            switch (lang)
            {
                case UiLanguage.ChineseSimplified:
                    return $"<color=#4BC5FF><b>【来源模组：{modName}】</b></color>";
                case UiLanguage.ChineseTraditional:
                    return $"<color=#4BC5FF><b>【來源模組：{modName}】</b></color>";
                case UiLanguage.Korean:
                    return $"<color=#4BC5FF><b>[제공 모드: {modName}]</b></color>";
                case UiLanguage.Japanese:
                    return $"<color=#4BC5FF><b>【提供MOD：{modName}】</b></color>";
                default:
                    return $"<color=#4BC5FF><b>[Mod: {modName}]</b></color>";
            }
        }
    }
}
