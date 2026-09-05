// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.
// Original mod concept: "AutoMachine" (Steam Workshop id 2992024030).

using AutoMachineRebuilt.Util;
using UnityEngine;

namespace AutoMachineRebuilt.Components
{
    /// <summary>
    /// Automates the Mission Control Station (base game and cluster variant).
    ///
    /// The vanilla work is performed through <c>MissionControlWorkable</c>,
    /// whose target rocket is only assigned while the chore exists. Ticking
    /// that workable without a Duplicant was the cause of the "no target
    /// assigned" crash reported for 2.0.7, because the workable asserts on its
    /// <c>TargetSpacecraft</c>.
    ///
    /// This controller never touches the workable. It asks the building's own
    /// state machine whether a boostable rocket is in range, waits the vanilla
    /// work time and then applies the same effect the vanilla completion
    /// applies (10 minutes of control station buff). If no rocket qualifies,
    /// nothing happens at all.
    /// </summary>
    public sealed class AutoMissionControlController : AutoWorkControllerBase
    {
        /// <summary>Vanilla work time of the Mission Control workable.</summary>
        private const float FallbackWorkTime = 90f;

        private MissionControl.Instance planetary;
        private MissionControlCluster.Instance cluster;
        private Workable workable;

        /// <summary>Seconds of work already spent on the current rocket.</summary>
        private float elapsed;

        /// <summary>Caches the state machine of the installed variant.</summary>
        protected override void Prepare()
        {
            base.Prepare();
            planetary = gameObject.GetSMI<MissionControl.Instance>();
            cluster = gameObject.GetSMI<MissionControlCluster.Instance>();
            workable = GetComponent<MissionControlWorkable>();
            if (workable == null)
            {
                workable = GetComponent<MissionControlClusterWorkable>();
            }
        }

        /// <summary>Runs one boosting step.</summary>
        /// <param name="dt">Seconds since the previous step.</param>
        protected override void Step(float dt)
        {
            // A Duplicant is boosting right now: leave the station alone.
            if (workable != null && workable.worker != null)
            {
                Reset();
                return;
            }

            if (!IsOperational() || !HasBoostableRocket())
            {
                Reset();
                return;
            }

            SetActive(true);
            StartAnimation();

            elapsed += dt;
            if (elapsed < WorkTime())
            {
                return;
            }

            elapsed = 0f;
            ApplyBoost();
            Reset();
        }

        /// <summary>Vanilla work time of the station, with a safe fallback.</summary>
        private float WorkTime()
        {
            return workable != null && workable.workTime > 0f ? workable.workTime : FallbackWorkTime;
        }

        /// <summary>
        /// Whether the building itself reports a rocket that may be boosted.
        /// The flag is maintained by the vanilla state machine, so room, range
        /// and "already boosted" checks stay exactly as in vanilla.
        /// </summary>
        private bool HasBoostableRocket()
        {
            if (planetary != null && planetary.IsRunning())
            {
                return planetary.sm.WorkableRocketsAreInRange.Get(planetary);
            }

            if (cluster != null && cluster.IsRunning())
            {
                return cluster.sm.WorkableRocketsAreInRange.Get(cluster);
            }

            return false;
        }

        /// <summary>Applies the vanilla control station buff to one rocket.</summary>
        private void ApplyBoost()
        {
            if (planetary != null && planetary.IsRunning())
            {
                Spacecraft craft = planetary.GetRandomBoostableSpacecraft();
                if (craft != null)
                {
                    planetary.ApplyEffect(craft);
                    planetary.UpdateWorkableRockets(null);
                    Log.Verbose("Mission Control boosted a spacecraft automatically.");
                }

                return;
            }

            if (cluster != null && cluster.IsRunning())
            {
                Clustercraft craft = cluster.GetRandomBoostableClustercraft();
                if (craft != null)
                {
                    cluster.ApplyEffect(craft);
                    cluster.UpdateWorkableRocketsInRange(null);
                    Log.Verbose("Mission Control boosted a clustercraft automatically.");
                }
            }
        }

        /// <summary>Stops the automated work.</summary>
        private void Reset()
        {
            elapsed = 0f;
            SetActive(false);
            StopAnimation();
        }
    }
}
