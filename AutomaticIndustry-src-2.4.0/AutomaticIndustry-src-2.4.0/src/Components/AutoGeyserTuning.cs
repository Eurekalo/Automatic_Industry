// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using AutoMachineRebuilt.Config;
using AutoMachineRebuilt.Util;
using UnityEngine;

namespace AutoMachineRebuilt.Components
{
    /// <summary>
    /// Displays a native floating progress bar beneath/over a Geyser reflecting
    /// how many Geotuners are currently assigned to tune it (1 = 20%, 2 = 40%,
    /// 3 = 60%, 4 = 80%, 5 = 100%).
    /// </summary>
    public sealed class AutoGeyserTuning : KMonoBehaviour, ISim1000ms
    {
        private Geyser geyser;
        private ProgressBar progressBar;

        protected override void OnSpawn()
        {
            base.OnSpawn();
            geyser = GetComponent<Geyser>();
        }

        public void Sim1000ms(float dt)
        {
            if (geyser == null)
            {
                return;
            }

            bool show = AutoMachineOptions.Instance.ProgressBarGeyserTuning &&
                        geyser.GetAmountOfGeotunersPointingThisGeyser() > 0;

            if (show)
            {
                if (progressBar == null)
                {
                    progressBar = ProgressBar.CreateProgressBar(gameObject, GetPercentComplete);
                }
                progressBar.SetVisibility(true);
            }
            else if (progressBar != null)
            {
                progressBar.gameObject.DeleteObject();
                progressBar = null;
            }
        }

        private float GetPercentComplete()
        {
            if (geyser == null)
            {
                return 0f;
            }

            int count = geyser.GetAmountOfGeotunersPointingThisGeyser();
            return Mathf.Clamp01(count / 5.0f);
        }

        protected override void OnCleanUp()
        {
            if (progressBar != null)
            {
                progressBar.gameObject.DeleteObject();
                progressBar = null;
            }

            base.OnCleanUp();
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(Geyser), "OnSpawn")]
    public static class Geyser_OnSpawn_TuningPatch
    {
        public static void Postfix(Geyser __instance)
        {
            if (__instance != null && __instance.gameObject != null)
            {
                if (__instance.gameObject.GetComponent<AutoGeyserTuning>() == null)
                {
                    __instance.gameObject.AddComponent<AutoGeyserTuning>();
                }
            }
        }
    }
}
