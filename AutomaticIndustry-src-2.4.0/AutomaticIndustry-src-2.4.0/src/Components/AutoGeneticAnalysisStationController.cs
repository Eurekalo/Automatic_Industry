// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using AutoMachineRebuilt.Automation;
using AutoMachineRebuilt.Config;
using AutoMachineRebuilt.Util;
using UnityEngine;

namespace AutoMachineRebuilt.Components
{
    /// <summary>
    /// Operates the Botanical Analyzer (GeneticAnalysisStation) autonomously without a Duplicant.
    ///
    /// Validates that an unidentified seed sample is present in internal storage,
    /// advances analysis work cleanly with complete null safety, and safely triggers
    /// mutation identification without relying on Duplicant skills or triggering FastTrack exceptions.
    /// </summary>
    public class AutoGeneticAnalysisStationController : AutoWorkControllerBase
    {
        private const float DefaultWorkTime = 150f;

        private GeneticAnalysisStation station;
        private GeneticAnalysisStationWorkable workable;
        private Storage storage;
        private float elapsed;
        private bool completing;
        private bool isDriving;

        /// <summary>Caches component references upon initialization.</summary>
        protected override void Prepare()
        {
            base.Prepare();
            station = GetComponent<GeneticAnalysisStation>();
            workable = GetComponent<GeneticAnalysisStationWorkable>();
            storage = GetComponent<Storage>();
        }

        /// <summary>Runs one automation step.</summary>
        /// <param name="dt">Seconds since previous step.</param>
        protected override void Step(float dt)
        {
            if (!IsOperational() || workable == null || storage == null)
            {
                Release();
                return;
            }

            // If a Duplicant is already actively performing manual analysis, step aside
            if (workable.GetWorker() != null)
            {
                Release();
                return;
            }

            // Verify a valid unidentified mutant seed is available in storage
            GameObject seed = storage.FindFirst(GameTags.UnidentifiedSeed);
            if (seed == null || storage.GetMassAvailable(GameTags.UnidentifiedSeed) < 1f)
            {
                Release();
                return;
            }

            MutantPlant mutant = seed.GetComponent<MutantPlant>();
            if (mutant == null)
            {
                Release();
                return;
            }

            // Suppress Duplicant operate chores while automation is actively driving
            CancelPendingChore();

            float targetWorkTime = workable.GetWorkTime();
            if (targetWorkTime <= 0f || float.IsInfinity(targetWorkTime))
            {
                targetWorkTime = DefaultWorkTime;
            }

            if (!isDriving)
            {
                isDriving = true;
                elapsed = 0f;
                workable.WorkTimeRemaining = targetWorkTime;
                SetActive(true);
                StartAnimation();
                Log.BuildingEvent(optionKey, "Automating Botanical Analyzer (" + seed.name + ")");
            }

            elapsed += dt;
            workable.WorkTimeRemaining = Mathf.Max(0f, targetWorkTime - elapsed);

            if (AutoMachineOptions.Instance.ProgressBarTelescopes)
            {
                workable.ShowProgressBar(true);
            }
            else
            {
                workable.ShowProgressBar(false);
            }

            if (elapsed >= targetWorkTime)
            {
                Complete();
            }
        }

        /// <summary>Completes the genetic analysis work session safely.</summary>
        private void Complete()
        {
            if (completing)
            {
                return;
            }

            completing = true;
            try
            {
                SafeInvoke.Try("Botanical Analyzer Complete", delegate
                {
                    if (workable != null)
                    {
                        workable.CompleteWork(null);
                    }
                });
                Log.BuildingEvent(optionKey, "Completed automated genetic analysis on " + optionKey);
            }
            finally
            {
                completing = false;
                Release();
            }
        }

        /// <summary>Releases active automation driving session.</summary>
        private void Release()
        {
            if (isDriving)
            {
                isDriving = false;
                elapsed = 0f;
                if (workable != null)
                {
                    workable.WorkTimeRemaining = workable.GetWorkTime();
                    workable.ShowProgressBar(false);
                }
                SetActive(false);
                StopAnimation();
            }
        }

        /// <summary>Cancels conflicting manual Duplicant chores on this station.</summary>
        private void CancelPendingChore()
        {
            ChoreSuppression.CancelOperateChores(gameObject);
        }
    }
}
