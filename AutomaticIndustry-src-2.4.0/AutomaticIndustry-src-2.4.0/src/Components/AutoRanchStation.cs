// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.
// Original mod concept: "AutoMachine" (Steam Workshop id 2992024030).

using System;
using System.Collections.Generic;
using System.Reflection;
using AutoMachineRebuilt.Config;
using AutoMachineRebuilt.Util;
using UnityEngine;

namespace AutoMachineRebuilt.Components
{
    /// <summary>
    /// Automates the six critter stations (Grooming, Shearing, Milking and
    /// their aquatic counterparts) by replaying the vanilla rancher flow
    /// without a Duplicant.
    ///
    /// Fixes in v2.4.6:
    /// <list type="bullet">
    /// <item>Active Target Validation: Bypasses the vanilla "if (!HasRancher) return;"
    /// limitation which caused queued critters to get permanently stuck if scales were
    /// sheared, pathing changed, or different critter species coexisted in the room.</item>
    /// <item>Arrival Watchdog: Automatically evicts critters that fail to navigate to
    /// the station within 15 seconds, preventing pathfinding anomalies from stalling the queue.</item>
    /// <item>Aquatic Pathfinding Isolation: Validates liquid navigation costs for
    /// Underwater Shearing Stations.</item>
    /// </list>
    /// </summary>
    public sealed class AutoRanchStation : AutoWorkControllerBase
    {
        private RanchStation.Instance stationSmi;
        private RanchStation.Def stationDef;
        private Workable rancherWorkable;

        /// <summary>Seconds of work already spent on the current critter.</summary>
        private float workElapsed;

        /// <summary>Seconds left before the next critter is invited.</summary>
        private float cooldownRemaining;

        /// <summary>Watchdog timer tracking how long we've waited for an invited critter to reach the table.</summary>
        private float waitingForArrivalElapsed;

        /// <summary>Maximum seconds to wait for a queued critter to walk/swim to the station table before evicting it.</summary>
        private const float ArrivalTimeoutSeconds = 15f;

        /// <summary>Whether a critter is currently being tended.</summary>
        private bool working;

        /// <summary>Whether critters were invited and must be released on stop.</summary>
        private bool invited;

        /// <summary>Whether the station work animation is currently playing.</summary>
        private bool stationAnimating;

        private ProgressBar progressBar;

        private static readonly FieldInfo TargetRanchablesField =
            typeof(RanchStation.Instance).GetField(
                "targetRanchables",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        private static readonly MethodInfo AbandonMethod =
            typeof(RanchStation.Instance).GetMethod(
                "Abandon",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        /// <summary>
        /// Work animations of the six stations, best match first.
        /// </summary>
        private static readonly string[] StationWorkAnims =
        {
            "shearing_loop", "working_loop", "work_loop", "milking_loop",
            "grooming_loop", "working", "on"
        };

        /// <summary>Caches the state machine instance and its definition.</summary>
        protected override void Prepare()
        {
            base.Prepare();
            stationSmi = gameObject.GetSMI<RanchStation.Instance>();
            stationDef = stationSmi == null ? null : stationSmi.def;
            rancherWorkable = GetComponent<RancherChore.RancherWorkable>();
        }

        /// <summary>Runs one step of the automated ranching cycle.</summary>
        /// <param name="dt">Seconds since the previous step.</param>
        protected override void Step(float dt)
        {
            if (stationSmi == null || stationDef == null || !stationSmi.IsRunning())
            {
                return;
            }

            // A Duplicant rancher always wins: the vanilla chore owns the
            // station and drives the critter queue on its own.
            if (stationSmi.HasRancher)
            {
                ResetWork();
                invited = false;
                waitingForArrivalElapsed = 0f;
                return;
            }

            if (!IsOperational())
            {
                ReleaseQueue();
                return;
            }

            // Proactively purge dead, ineligible, or unreachable critters from the queue
            PurgeAndValidateTargetRanchables();

            RanchedStates.Instance critter = stationSmi.ActiveRanchable;
            if (critter.IsNullOrStopped())
            {
                ResetWork();
                InviteCritter(dt);
                CheckArrivalWatchdog(dt);
                return;
            }

            // Critter is on the table: reset arrival watchdog and tend it
            waitingForArrivalElapsed = 0f;
            TendCritter(critter, dt);
        }

        /// <summary>
        /// Proactively validates the queued critters in targetRanchables.
        /// Vanilla's ValidateTargetRanchables has "if (!HasRancher) return;"
        /// which prevents un-manned stations from cleaning up stale/stuck targets.
        /// </summary>
        private void PurgeAndValidateTargetRanchables()
        {
            if (stationSmi == null || TargetRanchablesField == null)
            {
                return;
            }

            List<RanchableMonitor.Instance> list = TargetRanchablesField.GetValue(stationSmi) as List<RanchableMonitor.Instance>;
            if (list == null || list.Count == 0)
            {
                return;
            }

            int targetCell = stationSmi.GetRanchNavTarget();
            CavityInfo stationCavity = Game.Instance.roomProber.GetCavityForCell(targetCell);

            for (int i = list.Count - 1; i >= 0; i--)
            {
                RanchableMonitor.Instance item = list[i];
                if (item.IsNullOrStopped() || item.States.IsNullOrStopped() || item.gameObject == null)
                {
                    EvictQueueItem(item, list, i);
                    continue;
                }

                // 1. Eligibility Check (e.g. scales grown for Shearing, milk ready for Milking)
                if (stationDef.IsCritterEligibleToBeRanchedCb != null &&
                    !stationDef.IsCritterEligibleToBeRanchedCb(item.gameObject, stationSmi))
                {
                    EvictQueueItem(item, list, i);
                    continue;
                }

                // 2. Cavity / Room Check
                int critterCell = Grid.PosToCell(item.transform.GetPosition());
                CavityInfo critterCavity = Game.Instance.roomProber.GetCavityForCell(critterCell);
                if (stationCavity != null && critterCavity != stationCavity)
                {
                    EvictQueueItem(item, list, i);
                    continue;
                }

                // 3. Navigation Reachability Check
                int navCell = targetCell;
                if (item.HasTag(GameTags.Creatures.Flyer))
                {
                    navCell = Grid.CellAbove(navCell);
                }

                if (item.NavComponent != null && item.NavComponent.GetNavigationCost(navCell) == -1)
                {
                    EvictQueueItem(item, list, i);
                    continue;
                }
            }
        }

        private void EvictQueueItem(RanchableMonitor.Instance item, List<RanchableMonitor.Instance> list, int index)
        {
            if (AbandonMethod != null && item != null)
            {
                try
                {
                    AbandonMethod.Invoke(stationSmi, new object[] { item });
                    return;
                }
                catch { }
            }

            if (item != null)
            {
                item.TargetRanchStation = null;
                item.Trigger(1689625967); // RanchStationNoLongerAvailable
            }

            if (index >= 0 && index < list.Count)
            {
                list.RemoveAt(index);
            }
        }

        private void CheckArrivalWatchdog(float dt)
        {
            if (!stationSmi.IsCritterAvailableForRanching)
            {
                waitingForArrivalElapsed = 0f;
                return;
            }

            waitingForArrivalElapsed += dt;
            if (waitingForArrivalElapsed > ArrivalTimeoutSeconds)
            {
                waitingForArrivalElapsed = 0f;
                Log.Verbose("Critter arrival timed out on " + name + "; evicting stalled critter.");
                List<RanchableMonitor.Instance> list = TargetRanchablesField?.GetValue(stationSmi) as List<RanchableMonitor.Instance>;
                if (list != null && list.Count > 0)
                {
                    EvictQueueItem(list[0], list, 0);
                }
                else
                {
                    stationSmi.TriggerRanchStationNoLongerAvailable();
                }
            }
        }

        /// <summary>
        /// Queues an eligible critter and signals that the station is ready.
        /// The invitation is skipped while the configured cooldown runs.
        /// </summary>
        /// <param name="dt">Seconds since the previous step.</param>
        private void InviteCritter(float dt)
        {
            if (cooldownRemaining > 0f)
            {
                cooldownRemaining -= dt;
                return;
            }

            if (!stationSmi.IsCritterAvailableForRanching)
            {
                stationSmi.FindRanchable();
            }

            if (!stationSmi.IsCritterAvailableForRanching)
            {
                return;
            }

            invited = true;

            if (!stationSmi.IsRancherReady)
            {
                stationSmi.MessageRancherReady();
            }
        }

        /// <summary>
        /// Advances the vanilla work on the critter currently on the table and
        /// completes it once the vanilla work time elapsed.
        /// </summary>
        /// <param name="critter">Critter state machine on the station.</param>
        /// <param name="dt">Seconds since the previous step.</param>
        private void TendCritter(RanchedStates.Instance critter, float dt)
        {
            if (!working)
            {
                working = true;
                workElapsed = 0f;
                if (stationDef.OnRanchWorkBegins != null && rancherWorkable != null)
                {
                    stationDef.OnRanchWorkBegins(critter, rancherWorkable);
                }

                PlayCritterAnim(critter, stationDef.RanchedPreAnim, stationDef.RanchedLoopAnim);
            }

            SetActive(true);
            PlayStationWorkAnim();

            if (progressBar == null)
            {
                progressBar = ProgressBar.CreateProgressBar(gameObject, () =>
                {
                    if (!working || stationDef == null) return 0f;
                    return Mathf.Clamp01(workElapsed / Mathf.Max(1f, stationDef.WorkTime));
                });
            }

            if (progressBar != null)
            {
                progressBar.SetVisibility(true);
            }

            workElapsed += dt;
            if (stationDef.OnRanchWorkTick != null && rancherWorkable != null)
            {
                stationDef.OnRanchWorkTick(critter.gameObject, dt, rancherWorkable);
            }

            if (workElapsed < Mathf.Max(1f, stationDef.WorkTime))
            {
                return;
            }

            KBatchedAnimController critterAnim = critter.AnimController;
            stationSmi.RanchCreature();
            if (critterAnim != null)
            {
                critterAnim.Play(stationDef.RanchedPstAnim);
            }

            cooldownRemaining = Mathf.Max(0f, AutoMachineOptions.Instance.RanchIntervalSeconds);
            ResetWork();

            Log.Verbose("Automated ranching finished on " + name);
        }

        /// <summary>Plays the station specific critter animation pair.</summary>
        /// <param name="critter">Critter that is being tended.</param>
        /// <param name="pre">Introduction animation.</param>
        /// <param name="loop">Looping work animation.</param>
        private static void PlayCritterAnim(RanchedStates.Instance critter, HashedString pre, HashedString loop)
        {
            KBatchedAnimController controller = critter.AnimController;
            if (controller == null)
            {
                return;
            }

            controller.Play(pre);
            controller.Queue(loop, KAnim.PlayMode.Loop);
        }

        /// <summary>Starts the station specific work animation once.</summary>
        private void PlayStationWorkAnim()
        {
            if (stationAnimating)
            {
                return;
            }

            stationAnimating = WorkAnim.PlayFirst(animController, StationWorkAnims);
            if (!stationAnimating)
            {
                StartAnimation();
            }
        }

        /// <summary>Stops the automated work without touching the queue.</summary>
        private void ResetWork()
        {
            working = false;
            workElapsed = 0f;
            SetActive(false);

            if (progressBar != null)
            {
                progressBar.SetVisibility(false);
            }

            if (stationAnimating)
            {
                stationAnimating = false;
                WorkAnim.PlayIdle(animController);
            }

            StopAnimation();
        }

        /// <summary>
        /// Releases every critter that was invited by the automation so they
        /// return to their normal behaviour instead of waiting forever.
        /// </summary>
        private void ReleaseQueue()
        {
            ResetWork();
            if (!invited || stationSmi == null || !stationSmi.IsRunning() || stationSmi.HasRancher)
            {
                return;
            }

            invited = false;
            waitingForArrivalElapsed = 0f;
            stationSmi.TriggerRanchStationNoLongerAvailable();
        }

        /// <summary>Releases the queue when the automation is switched off.</summary>
        public override void StopAutomation()
        {
            base.StopAutomation();
            SafeInvoke.Try("releasing ranch queue", ReleaseQueue);
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
