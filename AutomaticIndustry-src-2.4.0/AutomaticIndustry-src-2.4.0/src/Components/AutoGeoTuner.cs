// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.
// Original mod concept: "AutoMachine" (Steam Workshop id 2992024030).

using System;
using System.Reflection;
using AutoMachineRebuilt.Config;
using AutoMachineRebuilt.Util;
using UnityEngine;

namespace AutoMachineRebuilt.Components
{
    /// <summary>
    /// Automates the Geotuner building:
    /// <list type="bullet">
    /// <item>Applies pending geyser switches without waiting for a Duplicant.</item>
    /// <item>Manages material delivery gating: only requests tuning materials when
    /// remaining tuning broadcast time is 5% or less, preventing premature delivery.</item>
    /// <item>Completes the research interaction automatically and consumes 50kg
    /// material once the previous broadcast expires and material is present.</item>
    /// <item>Renders the Geotuner data depletion progress bar (100% -> 0%) or
    /// material delivery fill progress bar.</item>
    /// </list>
    /// </summary>
    public sealed class AutoGeoTuner : AutoWorkControllerBase
    {
        private GeoTuner.Instance smi;
        private ProgressBar progressBar;

        public AutoGeoTuner()
        {
            optionKey = "GEOTUNER";
        }

        private static readonly MethodInfo AbortSwitchGeyserChoreMethod =
            typeof(GeoTuner.Instance).GetMethod(
                "AbortSwitchGeyserChore",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        private static readonly FieldInfo ResearchStateField =
            typeof(GeoTuner).GetField(
                "researcherInteractionNeeded",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        private static readonly FieldInfo OperationalStateField =
            typeof(GeoTuner).GetField(
                "operational",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        protected override void Prepare()
        {
            if (string.IsNullOrEmpty(optionKey))
            {
                optionKey = "GEOTUNER";
            }
            base.Prepare();
            smi = gameObject.GetSMI<GeoTuner.Instance>();
        }

        protected override void Step(float dt)
        {
            if (smi == null || !smi.IsRunning())
            {
                UpdateProgressBar(false);
                return;
            }

            AutoSwitchGeyser();
            UpdateDeliveryGating();
            AutoCompleteResearch();
            UpdateProgressBar(AutoMachineOptions.Instance.ProgressBarGeoTuner);
        }

        /// <summary>
        /// Only allows tuning material delivery when the active broadcast has
        /// 5% or less duration remaining (or when research is currently needed).
        /// This prevents premature delivery or excessive stockpiling while tuning.
        /// </summary>
        private void UpdateDeliveryGating()
        {
            if (smi == null || smi.manualDelivery == null)
            {
                return;
            }

            Geyser assigned = smi.GetAssignedGeyser();
            if (assigned == null)
            {
                return;
            }

            GeoTunerConfig.GeotunedGeyserSettings settings = smi.def.GetSettingsForGeyser(assigned);
            if (settings.quantity <= 0f)
            {
                return;
            }

            bool isBroadcasting = smi.sm.hasBeenWorkedByResearcher.Get(smi);
            if (isBroadcasting)
            {
                float duration = settings.duration > 0f ? settings.duration : 600f;
                float remaining = GeoTuner.GetRemainingExpiraionTime(smi);
                float percentRemaining = duration > 0f ? (remaining / duration) : 0f;

                // When broadcast has more than 5% remaining, suppress delivery
                if (percentRemaining > 0.05f)
                {
                    smi.manualDelivery.refillMass = 0f;
                    smi.manualDelivery.capacity = 0f;
                    return;
                }
            }

            // Broadcast is <= 5% remaining or research is needed: open delivery
            smi.storage.capacityKg = settings.quantity;
            smi.manualDelivery.capacity = settings.quantity;
            smi.manualDelivery.refillMass = settings.quantity;
            smi.manualDelivery.MinimumMass = settings.quantity;
            smi.manualDelivery.RequestedItemTag = settings.material;
        }

        private void UpdateProgressBar(bool show)
        {
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
            if (smi == null || !smi.IsRunning())
            {
                return 0f;
            }

            Geyser assigned = smi.GetAssignedGeyser();
            if (assigned == null)
            {
                return 0f;
            }

            // If broadcasting (consuming data): 100% -> 0%
            GeoTuner.OperationalState operational =
                OperationalStateField?.GetValue(smi.sm) as GeoTuner.OperationalState;
            if (operational?.geyserSelected?.broadcasting != null &&
                AutoOilRefinery.IsInState(smi.GetCurrentState(), operational.geyserSelected.broadcasting))
            {
                float duration = smi.def.GetSettingsForGeyser(assigned).duration;
                if (duration > 0f)
                {
                    float remaining = GeoTuner.GetRemainingExpiraionTime(smi);
                    return Mathf.Clamp01(remaining / duration);
                }
            }

            // If resource needed: 0% -> 100% material filled
            GeoTunerConfig.GeotunedGeyserSettings settings = smi.def.GetSettingsForGeyser(assigned);
            if (smi.storage != null && settings.quantity > 0f)
            {
                return Mathf.Clamp01(smi.storage.MassStored() / settings.quantity);
            }

            return 0f;
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

        /// <summary>
        /// Applies a pending geyser selection made by the player in the side
        /// screen, replacing the Duplicant confirmation chore.
        /// </summary>
        private void AutoSwitchGeyser()
        {
            Geyser future = smi.GetFutureGeyser();
            if (future == null || future == smi.GetAssignedGeyser())
            {
                return;
            }

            if (AbortSwitchGeyserChoreMethod != null)
            {
                AbortSwitchGeyserChoreMethod.Invoke(smi, new object[] { "Switched automatically by AutoMachine Rebuilt" });
            }

            smi.AssignGeyser(future);
        }

        /// <summary>
        /// Consumes the delivered material and starts the broadcast as soon as
        /// the previous broadcast is expired and tuning material is fully delivered.
        /// </summary>
        private void AutoCompleteResearch()
        {
            if (smi == null || !smi.IsRunning()) return;

            Geyser assigned = smi.GetAssignedGeyser();
            if (assigned == null)
            {
                return;
            }

            // If broadcasting is still active (> 0s), do not consume
            if (smi.sm.hasBeenWorkedByResearcher.Get(smi))
            {
                float remaining = GeoTuner.GetRemainingExpiraionTime(smi);
                if (remaining > 0f)
                {
                    return;
                }
                // Expiration reached 0: reset research worked flag
                smi.sm.hasBeenWorkedByResearcher.Set(false, smi);
            }

            GeoTuner.OperationalState operational =
                OperationalStateField?.GetValue(smi.sm) as GeoTuner.OperationalState;
            if (operational?.geyserSelected?.broadcasting != null &&
                AutoOilRefinery.IsInState(smi.GetCurrentState(), operational.geyserSelected.broadcasting))
            {
                float remaining = GeoTuner.GetRemainingExpiraionTime(smi);
                if (remaining > 0f)
                {
                    return;
                }
            }

            if (!HasFullTuningMaterial())
            {
                return;
            }

            // Consume material and activate broadcasting
            GeoTuner.OnResearchCompleted(smi);

            if (operational?.geyserSelected?.broadcasting != null)
            {
                smi.GoTo(operational.geyserSelected.broadcasting);
            }

            smi.RefreshGeyserSymbol();
            smi.RefreshLogicOutput();
        }

        /// <summary>
        /// Whether the tuning material was fully delivered, which is the
        /// non-room half of the vanilla <c>WorkRequirementsMet</c> check.
        /// </summary>
        private bool HasFullTuningMaterial()
        {
            Storage storage = smi?.storage;
            if (storage == null)
            {
                return false;
            }

            Geyser assigned = smi.GetAssignedGeyser();
            if (assigned == null)
            {
                return false;
            }

            GeoTunerConfig.GeotunedGeyserSettings settings = smi.def.GetSettingsForGeyser(assigned);
            if (settings.quantity <= 0f)
            {
                return false;
            }

            float mass = storage.GetMassAvailable(settings.material);
            if (mass >= settings.quantity)
            {
                return true;
            }

            // Secondary fallback scan of storage items
            float total = 0f;
            if (storage.items != null)
            {
                for (int i = 0; i < storage.items.Count; i++)
                {
                    GameObject item = storage.items[i];
                    if (item != null && item.HasTag(settings.material))
                    {
                        PrimaryElement pe = item.GetComponent<PrimaryElement>();
                        if (pe != null) total += pe.Mass;
                    }
                }
            }

            return total >= settings.quantity;
        }

        private static GeoTuner.ResearchState GetResearchState(GeoTuner sm)
        {
            if (sm == null)
            {
                return null;
            }

            if (ResearchStateField != null)
            {
                return ResearchStateField.GetValue(sm) as GeoTuner.ResearchState;
            }

            GeoTuner.OperationalState op = OperationalStateField?.GetValue(sm) as GeoTuner.OperationalState;
            return op?.geyserSelected?.researcherInteractionNeeded;
        }

        public override void StopAutomation()
        {
            if (smi != null && smi.manualDelivery != null)
            {
                Geyser assigned = smi.GetAssignedGeyser();
                if (assigned != null)
                {
                    GeoTunerConfig.GeotunedGeyserSettings settings = smi.def.GetSettingsForGeyser(assigned);
                    if (settings.quantity > 0f)
                    {
                        smi.manualDelivery.capacity = settings.quantity;
                        smi.manualDelivery.refillMass = settings.quantity;
                        smi.manualDelivery.MinimumMass = settings.quantity;
                        smi.manualDelivery.RequestedItemTag = settings.material;
                    }
                }
            }

            UpdateProgressBar(false);
            base.StopAutomation();
        }
    }
}