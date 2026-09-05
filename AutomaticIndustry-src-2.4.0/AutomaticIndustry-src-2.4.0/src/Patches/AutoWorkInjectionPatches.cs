// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.
// Original mod concept: "AutoMachine" (Steam Workshop id 2992024030).

using AutoMachineRebuilt.Automation;
using AutoMachineRebuilt.Components;
using AutoMachineRebuilt.Util;
using HarmonyLib;
using UnityEngine;

namespace AutoMachineRebuilt.Patches
{
    /// <summary>
    /// Enables the automated first study of geysers and other studyable
    /// features.
    ///
    /// <see cref="Studyable"/> is attached to the feature at runtime, so the
    /// driver is added on spawn rather than on a prefab. Vanilla behaviour is
    /// untouched: the driver only performs the study chore the feature
    /// already offers.
    /// </summary>
    [HarmonyPatch(typeof(Studyable), "OnSpawn")]
    internal static class StudyableAutomationPatch
    {
        /// <summary>Adds the driver to a spawned studyable feature.</summary>
        /// <param name="__instance">Studyable component.</param>
        internal static void Postfix(Studyable __instance)
        {
            SafeInvoke.Try("Attaching geyser study automation", delegate
            {
                if (__instance == null || __instance.gameObject == null)
                {
                    return;
                }

                GameObject go = __instance.gameObject;
                AutoWorkableController driver = go.GetComponent<AutoWorkableController>();
                if (driver == null)
                {
                    driver = go.AddComponent<AutoWorkableController>();
                }

                driver.Configure(AutomationRegistry.GeyserStudyKey);
            });
        }
    }
}
