// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.
// Original mod concept: "AutoMachine" (Steam Workshop id 2992024030).

using System.Reflection;
using AutoMachineRebuilt.Automation;
using AutoMachineRebuilt.Config;
using AutoMachineRebuilt.Util;
using HarmonyLib;
using UnityEngine;

namespace AutoMachineRebuilt.Components
{
    /// <summary>
    /// Releases the pressure of an <see cref="OilWellCap"/> without a Duplicant,
    /// provides a backpressure threshold progress bar at the building's base,
    /// and displays the time remaining until pressure release in the Status panel (EN, ZH, KO, JA).
    /// </summary>
    public sealed class AutoOilWellCap : KMonoBehaviour, ISim1000ms
    {
        /// <summary>Backing field of the private state machine instance.</summary>
        private static readonly FieldInfo SmiField = AccessTools.Field(typeof(OilWellCap), "smi");

        [MyCmpGet]
        private OilWellCap wellCap;

        [MyCmpGet]
        private KSelectable selectable;

        private ProgressBar progressBar;
        private float pressureProgress;

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            if (wellCap == null) wellCap = GetComponent<OilWellCap>();
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            CustomStatusItems.Register();

            if (wellCap == null) wellCap = GetComponent<OilWellCap>();
            if (selectable == null) selectable = GetComponent<KSelectable>();

            if (wellCap != null)
            {
                wellCap.ShowProgressBar(false);
            }
        }

        /// <summary>Reads the private state machine instance of the well.</summary>
        private OilWellCap.StatesInstance GetStateMachine()
        {
            if (wellCap == null) wellCap = GetComponent<OilWellCap>();
            if (SmiField == null || wellCap == null)
            {
                return null;
            }

            return SmiField.GetValue(wellCap) as OilWellCap.StatesInstance;
        }

        /// <summary>
        /// Polls the current pressure once per simulated second, drives automated release,
        /// updates the threshold progress bar, and updates the Status countdown.
        /// </summary>
        /// <param name="dt">Elapsed simulated seconds.</param>
        public void Sim1000ms(float dt)
        {
            if (wellCap == null) wellCap = GetComponent<OilWellCap>();
            if (selectable == null) selectable = GetComponent<KSelectable>();
            OilWellCap.StatesInstance smi = GetStateMachine();

            if (smi == null || !smi.IsRunning() || wellCap == null)
            {
                return;
            }

            bool automated = AutoMachineOptions.IsEnabledFor(gameObject, OilWellCapConfig.ID) ||
                             AutoMachineOptions.IsEnabledFor(gameObject, "OILWELLCAP") ||
                             AutoMachineOptions.IsEnabledFor(gameObject, "OilWellCap");

            if (automated)
            {
                SafeInvoke.Try("OilWellCap automatic depressurize", delegate
                {
                    bool isReleasing = smi.sm.working.Get(smi);
                    if (!isReleasing)
                    {
                        // Start depressurizing when pressure reaches the threshold configured by player
                        if (wellCap.NeedsDepressurizing())
                        {
                            smi.sm.working.Set(true, smi);
                            Log.Problematic("OILWELLCAP", "OilWellCap reached threshold, started automated depressurizing.");
                        }
                    }
                    else
                    {
                        // Depressurization is in progress: keep venting until pressure reaches 0% (fully depleted)
                        float currentPressure = smi.GetPressurePercent();
                        if (currentPressure <= 0.001f)
                        {
                            smi.sm.working.Set(false, smi);
                            Log.Problematic("OILWELLCAP", "OilWellCap depressurizing completed (pressure reached 0%).");
                        }

                        // Suppress any duplicant release chores
                        ChoreSuppression.CancelOperateChores(gameObject);
                    }
                });
            }

            UpdateProgressBar(smi);
            UpdateStatusItem(smi);
        }

        /// <summary>
        /// Displays the Backpressure progress bar at the building's bottom,
        /// scaled so that 100% corresponds precisely to the configured release threshold.
        /// </summary>
        private void UpdateProgressBar(OilWellCap.StatesInstance smi)
        {
            if (smi == null || wellCap == null) return;
            float curPressure = smi.GetPressurePercent();
            float threshold = Mathf.Max(0.01f, wellCap.GetSliderValue(0) / 100f);
            pressureProgress = Mathf.Clamp01(curPressure / threshold);

            if (progressBar == null)
            {
                progressBar = ProgressBar.CreateProgressBar(gameObject, () => pressureProgress);
            }

            if (progressBar != null)
            {
                progressBar.SetVisibility(true);
            }
        }

        public bool IsVenting
        {
            get
            {
                OilWellCap.StatesInstance smi = GetStateMachine();
                return smi != null && smi.sm.working.Get(smi);
            }
        }

        public float GetRemainingPressureTime()
        {
            OilWellCap.StatesInstance smi = GetStateMachine();
            if (smi == null || wellCap == null) return 0f;

            float threshold = Mathf.Max(0.01f, wellCap.GetSliderValue(0) / 100f);
            float curPressure = smi.GetPressurePercent();
            float remPressure = Mathf.Max(0f, threshold - curPressure);
            float remKg = remPressure * wellCap.maxGasPressure;
            float rate = wellCap.addGasRate > 0f ? wellCap.addGasRate : 1f;
            return remKg / rate;
        }

        /// <summary>
        /// Updates the Status panel with the estimated time remaining until pressure release is needed.
        /// </summary>
        private void UpdateStatusItem(OilWellCap.StatesInstance smi)
        {
            if (selectable == null)
            {
                return;
            }

            selectable.SetStatusItem(
                Db.Get().StatusItemCategories.Main,
                CustomStatusItems.OilWellPressureCountdown,
                this);
        }

        protected override void OnCleanUp()
        {
            if (progressBar != null)
            {
                progressBar.gameObject.DeleteObject();
                progressBar = null;
            }
            if (selectable != null)
            {
                selectable.SetStatusItem(Db.Get().StatusItemCategories.Main, null);
            }
            base.OnCleanUp();
        }
    }
}