// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System;
using System.Reflection;
using AutoMachineRebuilt.Util;
using HarmonyLib;

namespace AutoMachineRebuilt.Patches
{
    /// <summary>
    /// Compatibility shim for the "Customize Buildings" mod (Steam Workshop ID 1818138009).
    ///
    /// Customize Buildings contains several aggressive patches that directly conflict with
    /// Automatic Industry when enabled together:
    ///
    /// 1. Oil Refinery:
    ///    Customize Buildings' <c>OilRefineryConfig_ConfigureBuildingTemplate.Postfix</c> forcibly
    ///    destroys the <see cref="OilRefinery"/> component via <c>DestroyImmediate</c> and attaches
    ///    <c>WaterPurifier</c>. This triggers engine-level component requirement crashes on spawn,
    ///    disables Automatic Industry's <see cref="Components.AutoOilRefinery"/>, bypasses pressure
    ///    safety limits, and corrupts animations.
    ///
    /// 2. Oil Well Cap:
    ///    Customize Buildings' <c>OilWellCapConfig_ConfigureBuildingTemplate.Postfix</c> forcibly
    ///    destroys the <see cref="OilWellCap"/> component via <c>DestroyImmediate</c>, causing missing
    ///    component errors and breaking threshold venting automation.
    ///
    /// 3. Compost:
    ///    Customize Buildings' <c>Compost_States_Patch.Postfix</c> strips inert transition actions
    ///    and forces <c>inert.GoTo(composting)</c>. In combination with Automatic Industry's
    ///    automated flip cycle, this triggers an infinite state machine loop and freezes the game.
    ///
    /// 4. Desalinator:
    ///    Customize Buildings' <c>Desalinator_Patch.Postfix</c> wipes all full enterActions and transitions,
    ///    breaking threshold release and customizer controls.
    ///
    /// 5. Complex Fabricators:
    ///    Customize Buildings' <c>NoDupeHelper.SetAutomatic</c> forcibly mutates <c>duplicantOperated = false</c>,
    ///    bypassing Automatic Industry's fabricator controller and customizer menu.
    ///
    /// 6. Ice-E Fan:
    ///    Customize Buildings' <c>NoDupe_IceCooledFan.EditGO</c> removes <see cref="IceCooledFan"/> and
    ///    <see cref="IceCooledFanWorkable"/>, conflicting with Automatic Industry's dedicated controller.
    ///
    /// This shim gracefully neutralizes these destructive patches by applying Harmony prefixes that return false,
    /// and ensures that Automatic Industry's comprehensive automation takes precedence without errors.
    /// </summary>
    internal static class CustomizeBuildingsCompatibility
    {
        private static bool applied;

        public static void Apply(Harmony harmony)
        {
            if (applied || harmony == null)
            {
                return;
            }

            SafeInvoke.Try("CustomizeBuildings compatibility check", delegate
            {
                Type modType = AccessTools.TypeByName("CustomizeBuildings.FumiKMod") ??
                               AccessTools.TypeByName("CustomizeBuildings.CustomizeBuildingsState");
                if (modType == null)
                {
                    return;
                }

                Log.Info("CustomizeBuildings detected. Applying compatibility patches to preserve building automation...");

                // 1. Disable conflicting options in CustomizeBuildingsState.Instance via reflection
                DisableConflictingSettings();

                // 2. Dynamically patch destructive methods with Harmony prefixes returning false
                PatchPrefix(harmony, "CustomizeBuildings.OilRefineryConfig_ConfigureBuildingTemplate", "Postfix", nameof(SkipExecution_Prefix));
                PatchPrefix(harmony, "CustomizeBuildings.OilWellCapConfig_ConfigureBuildingTemplate", "Postfix", nameof(SkipExecution_Prefix));
                PatchPrefix(harmony, "CustomizeBuildings.Compost_States_Patch", "Postfix", nameof(SkipExecution_Prefix));
                PatchPrefix(harmony, "CustomizeBuildings.Desalinator_Patch", "Postfix", nameof(SkipExecution_Prefix));
                PatchPrefix(harmony, "CustomizeBuildings.NoDupeHelper", "SetAutomatic", nameof(SkipExecution_Prefix));
                PatchPrefix(harmony, "CustomizeBuildings.NoDupeMods", "EditGO", nameof(SkipExecution_Prefix));
                PatchPrefix(harmony, "CustomizeBuildings.NoDupeMods", "Enabled", nameof(ReturnFalse_Prefix));
                PatchPrefix(harmony, "CustomizeBuildings.NoDupe_IceCooledFan", "EditGO", nameof(SkipExecution_Prefix));
                PatchPrefix(harmony, "CustomizeBuildings.NoDupe_IceCooledFan", "EditDef", nameof(SkipExecution_Prefix));
                PatchPrefix(harmony, "CustomizeBuildings.NoDupe_IceCooledFan", "Enabled", nameof(ReturnFalse_Prefix));

                applied = true;
                Log.Info("CustomizeBuildings compatibility patches successfully applied.");
            });
        }

        private static void DisableConflictingSettings()
        {
            SafeInvoke.Try("Disabling CustomizeBuildings conflicting options in state", delegate
            {
                Type stateType = AccessTools.TypeByName("CustomizeBuildings.CustomizeBuildingsState");
                if (stateType == null) return;

                Type baseSettingsType = AccessTools.TypeByName("Shared.BaseSettings`1")?.MakeGenericType(stateType);
                object instance = null;
                if (baseSettingsType != null)
                {
                    FieldInfo instField = AccessTools.Field(baseSettingsType, "Instance");
                    if (instField != null)
                    {
                        instance = instField.GetValue(null);
                    }
                }

                if (instance == null) return;

                string[] boolPropsToDisable = new string[]
                {
                    "NoDupeOilRefinery",
                    "NoDupeOilWellCap",
                    "NoDupeCompost",
                    "NoDupeDesalinator",
                    "NoDupeIceCooledFan"
                };

                foreach (string propName in boolPropsToDisable)
                {
                    PropertyInfo prop = AccessTools.Property(stateType, propName);
                    if (prop != null && prop.CanWrite)
                    {
                        prop.SetValue(instance, false, null);
                        Log.Verbose("Disabled CustomizeBuildings setting: " + propName);
                    }
                }
            });
        }

        private static void PatchPrefix(Harmony harmony, string typeName, string methodName, string prefixMethodName)
        {
            SafeInvoke.Try($"Patching {typeName}.{methodName}", delegate
            {
                Type targetType = AccessTools.TypeByName(typeName);
                if (targetType == null) return;

                MethodInfo targetMethod = AccessTools.Method(targetType, methodName);
                if (targetMethod == null) return;

                MethodInfo prefixMethod = AccessTools.Method(typeof(CustomizeBuildingsCompatibility), prefixMethodName);
                if (prefixMethod == null) return;

                harmony.Patch(targetMethod, prefix: new HarmonyMethod(prefixMethod));
                Log.Verbose($"Patched {typeName}.{methodName} with {prefixMethodName}");
            });
        }

        /// <summary>
        /// Harmony prefix that returns false to skip the execution of the target method.
        /// </summary>
        public static bool SkipExecution_Prefix()
        {
            return false;
        }

        /// <summary>
        /// Harmony prefix that sets __result to false and skips original method execution.
        /// </summary>
        public static bool ReturnFalse_Prefix(ref bool __result)
        {
            __result = false;
            return false;
        }
    }
}
