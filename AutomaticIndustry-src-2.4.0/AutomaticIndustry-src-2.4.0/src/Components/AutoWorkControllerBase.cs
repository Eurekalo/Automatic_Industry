// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.
// Original mod concept: "AutoMachine" (Steam Workshop id 2992024030).

using System;
using AutoMachineRebuilt.Automation;
using AutoMachineRebuilt.Config;
using AutoMachineRebuilt.Util;
using UnityEngine;

namespace AutoMachineRebuilt.Components
{
    /// <summary>
    /// Shared skeleton of every automation controller.
    ///
    /// The base class owns everything that must behave identically for all
    /// automated buildings:
    /// <list type="bullet">
    /// <item>a fixed evaluation interval instead of per frame work,</item>
    /// <item>the option toggle and the optional room requirement waiver,</item>
    /// <item>the "a Duplicant always wins" rule,</item>
    /// <item>a per instance circuit breaker: repeated failures disable the
    /// automation of that single building and are reported once with the full
    /// exception, so a broken building can never spam the log or crash the
    /// simulation,</item>
    /// <item>a deterministic teardown on <c>OnCleanUp</c> and whenever the
    /// automation stops.</item>
    /// </list>
    /// Derived classes only implement <see cref="Step"/> for one vanilla
    /// mechanism.
    /// </summary>
    public abstract class AutoWorkControllerBase : KMonoBehaviour
    {
        /// <summary>Seconds between two automation evaluations.</summary>
        protected const float EvaluationInterval = 0.2f;

        /// <summary>Consecutive failures before this instance is switched off.</summary>
        private const int FailureLimit = 3;

        /// <summary>Seconds the automation stays off after a circuit break.</summary>
        private const float RecoveryDelaySeconds = 60f;

        /// <summary>How often a single building may recover before giving up.</summary>
        private const int RecoveryLimit = 5;

        /// <summary>
        /// Registry key of the building. Assigned on the prefab, therefore
        /// copied to every instance by Unity.
        /// </summary>
        public string optionKey;

        private bool prepared;
        private bool brokenOut;
        private int failures;
        private float accumulator;

        /// <summary>Seconds spent in the broken out state.</summary>
        private float recoveryTimer;

        /// <summary>Number of recoveries already granted to this instance.</summary>
        private int recoveries;

        /// <summary>Cached operational component, may be <c>null</c>.</summary>
        protected Operational operational;

        /// <summary>Cached animation controller, may be <c>null</c>.</summary>
        protected KAnimControllerBase animController;

        /// <summary>Whether the controller currently plays the work animation.</summary>
        private bool animating;

        /// <summary>
        /// Set as soon as the building starts to disappear. While it is set
        /// the controller must not touch any component that the game has
        /// already unregistered from its simulation vectors.
        /// </summary>
        private bool tearingDown;

        /// <summary>Assigns the registry key while the prefab is built.</summary>
        /// <param name="key">Registry option key.</param>
        internal void Configure(string key)
        {
            optionKey = key;
        }

        /// <summary>Evaluates the automation on a fixed interval.</summary>
        private void Update()
        {
            if (tearingDown || string.IsNullOrEmpty(optionKey) || Game.Instance == null)
            {
                return;
            }

            if (brokenOut)
            {
                TryRecover();
                return;
            }

            accumulator += Time.deltaTime;
            if (accumulator < EvaluationInterval)
            {
                return;
            }

            float dt = accumulator;
            accumulator = 0f;

            if (!AutoMachineOptions.IsEnabledFor(gameObject, optionKey))
            {
                StopAutomation();
                return;
            }

            if (!prepared)
            {
                prepared = true;
                if (!Guarded("preparing " + optionKey, Prepare))
                {
                    return;
                }
            }

            Guarded("automating " + optionKey, delegate { Step(dt); });

            // While the controller is actively automating this building, cancel
            // any Duplicant "operate the machine" chore so they do not walk
            // over and compete with the automation. Logistics chores (fetch,
            // deliver, empty) are preserved.
            ChoreSuppression.CancelOperateChores(gameObject);
        }

        /// <summary>
        /// Re-arms the automation of a building that was switched off by its
        /// circuit breaker.
        ///
        /// Most failures are transient: a critter that despawned mid work, a
        /// rocket that left while it was boosted, a storage a Duplicant
        /// emptied in the same frame. Leaving the building disabled until the
        /// next reload would silently degrade a colony, so the controller
        /// retries after a cool down. After <see cref="RecoveryLimit"/>
        /// unsuccessful recoveries the building stays on plain vanilla
        /// behaviour, which keeps a genuinely broken building from looping.
        /// </summary>
        private void TryRecover()
        {
            if (recoveries >= RecoveryLimit)
            {
                return;
            }

            recoveryTimer += Time.deltaTime;
            if (recoveryTimer < RecoveryDelaySeconds)
            {
                return;
            }

            recoveryTimer = 0f;
            recoveries++;
            failures = 0;
            prepared = false;
            brokenOut = false;
            Log.Warn("Retrying automation for " + optionKey + " (" + name + ", recovery " +
                     recoveries + " of " + RecoveryLimit + ").");
        }

        /// <summary>Caches components and applies the optional room waiver.</summary>
        protected virtual void Prepare()
        {
            operational = GetComponent<Operational>();
            animController = WorkAnim.ControllerOf(gameObject);
            ApplyRoomOverride();
        }

        /// <summary>One automation step of the concrete mechanism.</summary>
        /// <param name="dt">Seconds since the previous step.</param>
        protected abstract void Step(float dt);

        /// <summary>Releases everything the automation may have started.</summary>
        public virtual void StopAutomation()
        {
            SetActive(false);
            StopAnimation();
        }

        /// <summary>Stops the automation when the building goes away.</summary>
        protected override void OnCleanUp()
        {
            // Deconstruction unregisters the building from the temperature and
            // operational simulation before the components are destroyed.
            // Writing Operational.IsActive at that point makes the game throw
            // "Accessing mismatched handle version", so the teardown only
            // returns the animation to idle and leaves the operational flag to
            // the game.
            tearingDown = true;
            SafeInvoke.Try("cleaning up " + optionKey, StopAnimation);
            base.OnCleanUp();
        }

        /// <summary>
        /// Runs one automation action. Any exception is contained: the failure
        /// counter grows and the controller stops itself once the limit is
        /// reached, so the building falls back to plain vanilla behaviour.
        /// </summary>
        /// <param name="context">Description used in the log.</param>
        /// <param name="action">Action to perform.</param>
        /// <returns><c>true</c> when the action completed.</returns>
        protected bool Guarded(string context, System.Action action)
        {
            try
            {
                action();
                failures = 0;
                return true;
            }
            catch (Exception e)
            {
                failures++;
                Log.Error("Automation error while " + context + " (" + name + ", failure " +
                          failures + " of " + FailureLimit + ").", e);

                if (failures >= FailureLimit)
                {
                    brokenOut = true;
                    recoveryTimer = 0f;
                    Log.Warn("Automation paused for this " + optionKey +
                             " instance after repeated failures; the building keeps working " +
                             "normally with Duplicants and retries in " +
                             RecoveryDelaySeconds + " s.");
                    SafeInvoke.Try("stopping " + optionKey, StopAutomation);
                }

                return false;
            }
        }

        /// <summary>
        /// Waives the vanilla room requirement when the player asked for it.
        /// Only the hard requirement of this single building is relaxed; the
        /// room tracker itself keeps reporting the real room.
        /// </summary>
        private void ApplyRoomOverride()
        {
            AutomationEntry entry = AutomationRegistry.Find(optionKey);
            if (entry == null || !entry.RoomRequired ||
                !AutoMachineOptions.IsRoomRequirementIgnored(optionKey))
            {
                return;
            }

            RoomTracker tracker = GetComponent<RoomTracker>();
            if (tracker != null && tracker.requirement == RoomTracker.Requirement.Required)
            {
                tracker.requirement = RoomTracker.Requirement.Recommended;
                Log.Verbose("Room requirement waived for " + optionKey);
            }
        }

        /// <summary>Sets the operational active flag when it has to change.</summary>
        /// <param name="active">Requested active state.</param>
        protected void SetActive(bool active)
        {
            if (tearingDown || operational == null || this == null || gameObject == null)
            {
                return;
            }

            if (operational.IsActive != active)
            {
                operational.SetActive(active);
            }
        }

        /// <summary>Whether the building may run at all right now.</summary>
        protected bool IsOperational()
        {
            return operational == null || operational.IsOperational;
        }

        /// <summary>Starts the vanilla working animation.</summary>
        protected void StartAnimation()
        {
            if (!animating)
            {
                animating = WorkAnim.PlayWorking(animController);
            }
        }

        /// <summary>Returns to the idle animation.</summary>
        protected void StopAnimation()
        {
            if (animating && animController != null)
            {
                animating = false;
                WorkAnim.PlayIdle(animController);
            }
            else
            {
                animating = false;
            }
        }
    }
}
