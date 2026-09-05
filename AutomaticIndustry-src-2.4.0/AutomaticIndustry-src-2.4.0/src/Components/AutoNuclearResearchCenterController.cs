// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.
// Original mod concept: "AutoMachine" (Steam Workshop id 2992024030).

using AutoMachineRebuilt.Automation;
using AutoMachineRebuilt.Config;
using AutoMachineRebuilt.Util;
using UnityEngine;

namespace AutoMachineRebuilt.Components
{
    /// <summary>
    /// Automates the Materials Study Terminal (<see cref="NuclearResearchCenter"/>)
    /// without requiring a Duplicant or the Applied Sciences Research skill perk.
    ///
    /// Unlike standard research stations which rely on <see cref="ElementConverter"/>,
    /// the Materials Study Terminal uses <see cref="HighEnergyParticleStorage"/> (Radbolts)
    /// and <see cref="NuclearResearchCenterWorkable"/>. This controller consumes Radbolts,
    /// awards "nuclear" research points, displays research floating text FX, updates progress bars,
    /// drives the working loop animation, and suppresses Duplicant operate chores.
    /// </summary>
    public sealed class AutoNuclearResearchCenterController : AutoWorkControllerBase
    {
        [MyCmpGet]
        private NuclearResearchCenter nrc;

        [MyCmpGet]
        private NuclearResearchCenterWorkable workable;

        [MyCmpGet]
        private HighEnergyParticleStorage particleStorage;

        private float pointsProduced;
        private bool driving;

        protected override void Prepare()
        {
            base.Prepare();
            if (nrc == null) nrc = GetComponent<NuclearResearchCenter>();
            if (workable == null) workable = GetComponent<NuclearResearchCenterWorkable>();
            if (particleStorage == null) particleStorage = GetComponent<HighEnergyParticleStorage>();
        }

        private void CancelPendingChore()
        {
            if (nrc == null) return;
            var smi = nrc.GetSMI<NuclearResearchCenter.StatesInstance>();
            if (smi == null) return;

            WorkChore<NuclearResearchCenterWorkable> chore = StateMachineUtil.Field(smi, "chore") as WorkChore<NuclearResearchCenterWorkable>;
            if (chore != null && chore.driver != null)
            {
                chore.Cancel("Automated by AutoMachine Rebuilt");
            }
        }

        protected override void Step(float dt)
        {
            if (nrc == null) nrc = GetComponent<NuclearResearchCenter>();
            if (workable == null) workable = GetComponent<NuclearResearchCenterWorkable>();
            if (particleStorage == null) particleStorage = GetComponent<HighEnergyParticleStorage>();

            if (nrc == null || particleStorage == null) return;

            string researchTypeId = !string.IsNullOrEmpty(nrc.researchTypeID) ? nrc.researchTypeID : "nuclear";

            // Check if active research requires nuclear points
            TechInstance activeResearch = Research.Instance != null ? Research.Instance.GetActiveResearch() : null;
            bool needsNuclearResearch = activeResearch != null &&
                activeResearch.tech != null &&
                activeResearch.tech.costsByResearchTypeID != null &&
                activeResearch.tech.costsByResearchTypeID.TryGetValue(researchTypeId, out float cost) &&
                activeResearch.progressInventory.PointsByTypeID.TryGetValue(researchTypeId, out float current) &&
                current < cost;

            bool hasMaterial = particleStorage.Particles > 0f;
            bool hasWorker = workable != null && workable.worker != null;

            bool canWork = IsOperational() && !hasWorker && hasMaterial && needsNuclearResearch;

            if (canWork)
            {
                // Suppress Duplicants from claiming this station
                ChoreSuppression.CancelOperateChores(gameObject);
                CancelPendingChore();

                float speedMultiplier = 1f;
                if (Game.Instance != null && Game.Instance.FastWorkersModeActive)
                {
                    speedMultiplier = 2f;
                }

                float timePerPoint = nrc.timePerPoint > 0f ? nrc.timePerPoint : 100f;
                float progressDelta = (dt / timePerPoint) * speedMultiplier;

                float materialPerPoint = nrc.materialPerPoint > 0f ? nrc.materialPerPoint : 10f;
                float particlesNeeded = progressDelta * materialPerPoint;
                float particlesAvailable = particleStorage.Particles;
                if (particlesNeeded > particlesAvailable)
                {
                    progressDelta = particlesAvailable / materialPerPoint;
                    particlesNeeded = particlesAvailable;
                }

                if (particlesNeeded > 0f)
                {
                    particleStorage.ConsumeAndGet(particlesNeeded);
                }

                pointsProduced += progressDelta;

                if (pointsProduced >= 1f)
                {
                    int pointsToAdd = Mathf.FloorToInt(pointsProduced);
                    pointsProduced -= pointsToAdd;

                    if (PopFXManager.Instance != null)
                    {
                        var resType = Research.Instance != null ? Research.Instance.GetResearchType(researchTypeId) : null;
                        string resName = resType != null ? resType.name : "Applied Sciences Research";
                        PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Research, resName, transform);
                    }

                    if (Research.Instance != null)
                    {
                        Research.Instance.AddResearchPoints(researchTypeId, pointsToAdd);
                    }
                }

                if (!driving)
                {
                    driving = true;
                    SetActive(true);
                    Log.BuildingEvent(optionKey, "Started automated nuclear research on " + optionKey);
                }

                // Animation handling: play working_loop
                if (animController != null && animController.currentAnim != "working_loop")
                {
                    animController.Play("working_loop", KAnim.PlayMode.Loop);
                }

                // Progress Bar handling
                if (workable != null)
                {
                    if (AutoMachineOptions.Instance != null && AutoMachineOptions.Instance.ProgressBarResearch)
                    {
                        workable.ShowProgressBar(true);
                    }
                    else
                    {
                        workable.ShowProgressBar(false);
                    }
                }

                return;
            }

            if (driving)
            {
                StopAutomation();
            }
        }

        public override void StopAutomation()
        {
            if (driving)
            {
                driving = false;
                SetActive(false);
                if (animController != null)
                {
                    animController.Play("on", KAnim.PlayMode.Loop);
                }
                if (workable != null)
                {
                    workable.ShowProgressBar(false);
                }
            }

            base.StopAutomation();
        }
    }
}
