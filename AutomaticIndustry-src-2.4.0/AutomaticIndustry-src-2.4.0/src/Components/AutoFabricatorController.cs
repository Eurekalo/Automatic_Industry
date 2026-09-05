// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.
// Original mod concept: "AutoMachine" (Steam Workshop id 2992024030).

using AutoMachineRebuilt.Config;
using AutoMachineRebuilt.Util;
using UnityEngine;

namespace AutoMachineRebuilt.Components
{
    /// <summary>
    /// Automates every <see cref="ComplexFabricator"/> based building
    /// (Apothecary, Sushi Bar, Dehydrator, Plant Pulverizer, Rock Crusher,
    /// Manual Radbolt Generator, ...).
    ///
    /// The base game already contains a complete unattended production path:
    /// when <c>ComplexFabricator.duplicantOperated</c> is <c>false</c> the
    /// fabricator consumes its ingredients, runs its own timer, plays its own
    /// working animation and produces the recipe result from
    /// <c>Sim200ms</c>. The controller therefore only clears that flag while
    /// the option is on and restores the vanilla value as soon as the player
    /// turns the option off, which keeps recipe order, ingredient delivery and
    /// output storage exactly as shipped.
    ///
    /// Buildings that shipped with <c>duplicantOperated = true</c> also need
    /// the building kanim work loop driven from this controller, because
    /// vanilla only plays it from the Duplicant interaction. This applies to
    /// the Manual Radbolt Generator, the Sushi Bar and all other originally
    /// manually operated fabricators. The progress bar is also enabled for
    /// these buildings so the player can see production progress.
    /// </summary>
    public class AutoFabricatorController : AutoWorkControllerBase
    {
        private ComplexFabricator fabricator;

        /// <summary>
        /// Whether the building originally required a Duplicant and therefore
        /// needs its building animation and progress bar driven from this
        /// controller.
        /// </summary>
        private bool needsBuildingAnim;

        /// <summary>
        /// Tracks whether we are currently playing the working animation so we
        /// can stop it cleanly when the order completes.
        /// </summary>
        private bool playingWorkAnim;

        /// <summary>Caches the fabricator and its shipped configuration.</summary>
        protected override void Prepare()
        {
            base.Prepare();
            if (fabricator == null)
            {
                fabricator = GetComponent<ComplexFabricator>();
            }

            if (fabricator == null || fabricator.GetComponent<ComplexFabricatorWorkable>() == null)
            {
                // This fabricator is not duplicant-operated or has no workable (e.g. Kiln).
                // Do not automate or interfere with it.
                enabled = false;
                return;
            }

            needsBuildingAnim = true;

            if (fabricator != null)
            {
                fabricator.showProgressBar = true;
            }
        }

        /// <summary>Keeps the unattended production flag in sync with the option.</summary>
        /// <param name="dt">Seconds since the previous step.</param>
        protected override void Step(float dt)
        {
            if (fabricator == null)
            {
                fabricator = GetComponent<ComplexFabricator>();
            }

            if (fabricator == null || fabricator.GetComponent<ComplexFabricatorWorkable>() == null)
            {
                enabled = false;
                return;
            }

            if (fabricator.duplicantOperated)
            {
                fabricator.duplicantOperated = false;
                BuildingComplete building = GetComponent<BuildingComplete>();
                if (building != null)
                {
                    building.isManuallyOperated = false;
                }
                Log.Verbose("Unattended production enabled for " + optionKey);
            }

            if (fabricator != null && AutoMachineOptions.Instance != null)
            {
                fabricator.showProgressBar = AutoMachineOptions.Instance.ProgressBarFabricators;
            }

            UpdateProgressBar();

            if (needsBuildingAnim)
            {
                UpdateWorkAnimation();
            }
        }

        private ProgressBar progressBar;

        private void UpdateProgressBar()
        {
            bool show = AutoMachineOptions.Instance != null &&
                        AutoMachineOptions.Instance.ProgressBarFabricators &&
                        fabricator != null &&
                        fabricator.CurrentWorkingOrder != null &&
                        (operational == null || operational.IsOperational);

            if (show)
            {
                if (progressBar == null)
                {
                    progressBar = ProgressBar.CreateProgressBar(gameObject, () =>
                    {
                        if (fabricator == null)
                        {
                            return 0f;
                        }

                        return Mathf.Clamp01(fabricator.GetPercentComplete());
                    });
                }
                progressBar.SetVisibility(true);
            }
            else if (progressBar != null)
            {
                progressBar.gameObject.DeleteObject();
                progressBar = null;
            }
        }

        /// <summary>
        /// Drives the building's own working loop while an order is running.
        /// When the order completes or the building is no longer operational,
        /// the animation is stopped and the building returns to idle.
        ///
        /// For the Manual Radbolt Generator, the vanilla state machine only
        /// toggles the <see cref="RadiationEmitter"/> in response to
        /// <c>WorkableStartWork</c> and <c>WorkableStopWork</c> events which
        /// never fire without a Duplicant. The controller mirrors that toggle
        /// so the emitter is active while the building is working.
        /// </summary>
        private void UpdateWorkAnimation()
        {
            bool working = fabricator.CurrentWorkingOrder != null &&
                           (operational == null || operational.IsOperational);

            if (working)
            {
                if (!playingWorkAnim)
                {
                    playingWorkAnim = true;
                    SetActive(true);
                    StartAnimation();
                }

                // Manual Radbolt Generator: toggle the radiation emitter.
                ToggleRadboltEmitter(true);
            }
            else if (playingWorkAnim)
            {
                playingWorkAnim = false;
                SetActive(false);
                StopAnimation();

                ToggleRadboltEmitter(false);
            }
        }

        /// <summary>
        /// Toggles the <see cref="RadiationEmitter"/> on the Manual Radbolt
        /// Generator. No-op on any other building.
        /// </summary>
        /// <param name="emitting">Whether the emitter should be active.</param>
        private void ToggleRadboltEmitter(bool emitting)
        {
            RadiationEmitter emitter = GetComponent<RadiationEmitter>();
            if (emitter != null)
            {
                emitter.SetEmitting(emitting);
            }
        }

        private static readonly System.Reflection.MethodInfo FabricatorUpdateChoreMethod =
            HarmonyLib.AccessTools.Method(typeof(ComplexFabricator), "UpdateChore");

        /// <summary>Restores the shipped behaviour when the option is turned off.</summary>
        public override void StopAutomation()
        {
            if (fabricator != null && fabricator.GetComponent<ComplexFabricatorWorkable>() != null)
            {
                fabricator.duplicantOperated = true;

                BuildingComplete building = GetComponent<BuildingComplete>();
                if (building != null)
                {
                    building.isManuallyOperated = true;
                }

                if (FabricatorUpdateChoreMethod != null)
                {
                    SafeInvoke.Try("FabricatorUpdateChore", delegate
                    {
                        FabricatorUpdateChoreMethod.Invoke(fabricator, null);
                    });
                }
                fabricator.SetQueueDirty();
                fabricator.Trigger((int)GameHashes.FabricatorOrdersUpdated, fabricator);
            }

            if (playingWorkAnim)
            {
                playingWorkAnim = false;
                SetActive(false);
                StopAnimation();
                ToggleRadboltEmitter(false);
            }

            if (progressBar != null)
            {
                progressBar.gameObject.DeleteObject();
                progressBar = null;
            }

            base.StopAutomation();
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
}
