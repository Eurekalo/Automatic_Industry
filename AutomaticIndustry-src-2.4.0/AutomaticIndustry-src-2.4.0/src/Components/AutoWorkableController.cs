// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using AutoMachineRebuilt.Automation;
using AutoMachineRebuilt.Config;
using AutoMachineRebuilt.Util;

namespace AutoMachineRebuilt.Components
{
    /// <summary>
    /// Performs the work chore a building offers, without a Duplicant.
    ///
    /// The controller never invents work: it waits for the chore the building
    /// publishes itself (<see cref="LocalChoreProbe"/>), so all vanilla
    /// conditions still apply. The vanilla tick and completion are only used
    /// when <see cref="WorkSafety"/> proved they do not read the worker;
    /// otherwise the work is timed and, when even the completion is unsafe,
    /// the building is left to the Duplicants.
    /// </summary>
    public class AutoWorkableController : AutoWorkControllerBase
    {
        private Workable active;
        private float elapsed;
        private bool completing;

        /// <summary>Runs one automation step.</summary>
        /// <param name="dt">Seconds since the previous step.</param>
        protected override void Step(float dt)
        {
            if (!IsOperational())
            {
                Release();
                return;
            }

            Workable workable = active != null ? active : LocalChoreProbe.FindPendingWorkable(gameObject);
            if (workable == null || workable.GetWorker() != null || !IsWorkAllowed(workable))
            {
                Release();
                return;
            }

            if (!WorkSafety.CanCompleteWithoutWorker(workable))
            {
                Release();
                return;
            }

            if (active != workable)
            {
                active = workable;
                elapsed = 0f;
                SetActive(true);
                StartAnimation();
                Log.BuildingEvent(optionKey, "Automating " + optionKey + " (" + workable.GetType().Name + ")");
            }

            if (AutoMachineOptions.Instance.ProgressBarTelescopes)
            {
                workable.ShowProgressBar(true);
            }

            if (Advance(workable, dt))
            {
                Complete(workable);
            }
        }

        /// <summary>
        /// Extra condition of the concrete building family. The base
        /// implementation accepts every chore the building offers.
        /// </summary>
        /// <param name="workable">Workable published by the building.</param>
        protected virtual bool IsWorkAllowed(Workable workable)
        {
            return true;
        }

        /// <summary>Advances the work and reports whether it is finished.</summary>
        /// <param name="workable">Workable being driven.</param>
        /// <param name="dt">Seconds since the previous step.</param>
        private bool Advance(Workable workable, float dt)
        {
            elapsed += dt;

            if (WorkSafety.CanTickWithoutWorker(workable))
            {
                return workable.WorkTick(null, dt);
            }

            float workTime = workable.GetWorkTime();
            return workTime > 0f && !float.IsInfinity(workTime) && elapsed >= workTime;
        }

        /// <summary>Completes the work exactly once.</summary>
        /// <param name="workable">Workable being driven.</param>
        private void Complete(Workable workable)
        {
            if (completing)
            {
                return;
            }

            completing = true;
            try
            {
                workable.CompleteWork(null);
                Log.BuildingEvent(optionKey, "Completed automated work on " + optionKey);
            }
            finally
            {
                completing = false;
                Release();
            }
        }

        /// <summary>Stops the current session without completing it.</summary>
        private void Release()
        {
            if (active != null)
            {
                active.ShowProgressBar(false);
            }
            active = null;
            elapsed = 0f;
            StopAutomation();
        }
    }
}
