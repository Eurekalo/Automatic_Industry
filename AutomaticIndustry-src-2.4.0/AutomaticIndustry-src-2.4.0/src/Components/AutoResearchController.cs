// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.
// Original mod concept: "AutoMachine" (Steam Workshop id 2992024030).

using AutoMachineRebuilt.Config;
using AutoMachineRebuilt.Util;

namespace AutoMachineRebuilt.Components
{
    /// <summary>
    /// Runs research buildings (Research Station, Super Computer, Virtual
    /// Planetarium, Materials Study Terminal, Orbital Data Collection Lab)
    /// without a Duplicant.
    ///
    /// A research building has an infinite work time: a Duplicant does not
    /// "finish" it, the attached <see cref="ElementConverter"/> consumes the
    /// research material while <c>Operational.IsActive</c> is set and awards
    /// the research points through the vanilla callback. Ticking the workable
    /// is therefore both unnecessary and unsafe (the vanilla tick reads the
    /// worker's Science attribute), so the controller only sets the active
    /// flag and the work speed multiplier a Duplicant of average skill would
    /// produce.
    ///
    /// Every vanilla gate stays in place: the building must be operational -
    /// which already includes the "a research project that needs this point
    /// type is selected" requirement flag - must hold research material and
    /// must not have completed the selected research. The vanilla Duplicant
    /// chore is deliberately not part of the gate: the base game only creates
    /// it while the building is inactive, so requiring it would deadlock the
    /// automation as soon as it activates the building itself. The pending
    /// chore is cancelled instead, so no Duplicant walks to a station that is
    /// already running.
    /// </summary>
    public class AutoResearchController : AutoWorkControllerBase
    {
        /// <summary>Conversion rate of a Duplicant without any Science bonus.</summary>
        private const float BaseWorkSpeed = 2f;

        private ResearchCenter center;
        private ElementConverter converter;
        private ManualDeliveryKG sweeperDelivery;
        private bool driving;

        /// <summary>Caches the research components and delivery paths.</summary>
        protected override void Prepare()
        {
            base.Prepare();
            center = GetComponent<ResearchCenter>();
            converter = GetComponent<ElementConverter>();
            UpdateSweeperDeliveryState();
        }

        /// <summary>
        /// Synchronizes the parallel MachineFetch delivery state with the
        /// SweeperResearchDelivery option.
        /// </summary>
        private void UpdateSweeperDeliveryState()
        {
            if (sweeperDelivery == null)
            {
                ManualDeliveryKG[] deliveries = GetComponents<ManualDeliveryKG>();
                for (int i = 0; i < deliveries.Length; i++)
                {
                    if (deliveries[i] != null &&
                        deliveries[i].choreTypeIDHash == Db.Get().ChoreTypes.MachineFetch.IdHash)
                    {
                        sweeperDelivery = deliveries[i];
                        break;
                    }
                }
            }

            if (sweeperDelivery != null)
            {
                bool enabled = AutoMachineOptions.Instance.SweeperResearchDelivery;
                sweeperDelivery.allowPause = true;
                if (sweeperDelivery.IsPaused == enabled)
                {
                    sweeperDelivery.Pause(!enabled, "SweeperResearchDelivery toggle");
                }
            }
        }

        /// <summary>
        /// Cancels the pending vanilla research chore of this building, if any.
        /// </summary>
        private void CancelPendingChore()
        {
            if (center == null) return;
            Chore chore = StateMachineUtil.Field(center, "chore") as Chore;
            if (chore != null && chore.driver != null)
            {
                chore.Cancel("Automated by AutoMachine Rebuilt");
            }
        }

        /// <summary>Keeps the building researching while work is available.</summary>
        /// <param name="dt">Seconds since the previous step.</param>
        protected override void Step(float dt)
        {
            UpdateSweeperDeliveryState();

            if (center == null)
            {
                return;
            }

            bool wanted = IsOperational() &&
                          center.GetWorker() == null &&
                          StateMachineUtil.CallBool(center, "HasMaterial", true) &&
                          !StateMachineUtil.CallBool(center, "ResearchComponentCompleted", false);

            if (wanted)
            {
                if (converter != null)
                {
                    float speed = BaseWorkSpeed;
                    if (Game.Instance != null && Game.Instance.FastWorkersModeActive)
                    {
                        speed *= 2f;
                    }

                    converter.SetWorkSpeedMultiplier(speed);
                }

                if (!driving)
                {
                    driving = true;
                    CancelPendingChore();
                    SetActive(true);
                    StartAnimation();
                    Log.BuildingEvent(optionKey, "Started automated research on " + optionKey);
                }

                if (AutoMachineOptions.Instance.ProgressBarResearch)
                {
                    center.ShowProgressBar(true);
                }
                else
                {
                    center.ShowProgressBar(false);
                }

                return;
            }

            if (driving)
            {
                StopAutomation();
            }
        }

        /// <summary>Stops the research and returns the building to idle.</summary>
        public override void StopAutomation()
        {
            if (driving)
            {
                driving = false;
                SetActive(false);
                if (center != null)
                {
                    center.ShowProgressBar(false);
                }
            }

            base.StopAutomation();
        }
    }
}
