// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.
// Original mod concept: "AutoMachine" (Steam Workshop id 2992024030).

using AutoMachineRebuilt.Automation;
using AutoMachineRebuilt.Config;
using AutoMachineRebuilt.Util;
using UnityEngine;

namespace AutoMachineRebuilt.Components
{
    /// <summary>
    /// Keeps the Oil Refinery converting without an operator.
    ///
    /// The vanilla refinery only runs while a Duplicant works its
    /// <c>WorkableTarget</c>; that work does nothing but call
    /// <c>Operational.SetActive(true)</c>. The state machine already gates the
    /// building on power, crude oil supply and the surrounding gas pressure,
    /// so mirroring the active flag while the machine is in its "ready" state
    /// reproduces vanilla output exactly while completely suppressing Duplicant
    /// / Bionic Duplicant operate chores.
    /// </summary>
    public sealed class AutoOilRefinery : KMonoBehaviour, ISim200ms
    {
        [MyCmpGet]
        private OilRefinery refinery;

        [MyCmpGet]
        private Operational operational;

        private ElementConverter converter;
        private Workable workable;

        /// <summary>Whether this component currently drives the building.</summary>
        private bool drivingActiveFlag;

        /// <summary>Animation controller used for the automated work anim.</summary>
        private KAnimControllerBase animController;

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            if (refinery == null) refinery = GetComponent<OilRefinery>();
            if (operational == null) operational = GetComponent<Operational>();
            if (converter == null) converter = GetComponent<ElementConverter>();
            if (workable == null) workable = GetComponent<Workable>();
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            if (refinery == null) refinery = GetComponent<OilRefinery>();
            if (operational == null) operational = GetComponent<Operational>();
            if (converter == null) converter = GetComponent<ElementConverter>();
            if (workable == null) workable = GetComponent<Workable>();
            if (animController == null) animController = WorkAnim.ControllerOf(gameObject);
            SyncEfficiency(converter);
        }

        /// <summary>
        /// Dynamically synchronizes the Oil Refinery conversion efficiency (50 % vs 100 %)
        /// on the building instance so mod menu setting changes apply immediately in-game.
        /// </summary>
        public static void SyncEfficiency(GameObject go)
        {
            if (go == null) return;
            ElementConverter conv = go.GetComponent<ElementConverter>();
            SyncEfficiency(conv);
        }

        public static void SyncEfficiency(ElementConverter converter)
        {
            if (converter == null || converter.outputElements == null) return;

            var options = AutoMachineOptions.Instance;
            bool full100 = options != null && options.OilRefineryEfficiency == OilRefineryEfficiency.Full100;
            float targetPetroleum = full100 ? 10f : 5f;
            float targetMethane = full100 ? 0.18f : 0.09f;

            for (int i = 0; i < converter.outputElements.Length; i++)
            {
                var output = converter.outputElements[i];
                if (output.elementHash == SimHashes.Petroleum)
                {
                    if (output.massGenerationRate != targetPetroleum)
                    {
                        output.massGenerationRate = targetPetroleum;
                        converter.outputElements[i] = output;
                    }
                }
                else if (output.elementHash == SimHashes.Methane)
                {
                    if (output.massGenerationRate != targetMethane)
                    {
                        output.massGenerationRate = targetMethane;
                        converter.outputElements[i] = output;
                    }
                }
            }
        }

        /// <summary>
        /// Returns <c>true</c> when <paramref name="state"/> is
        /// <paramref name="ancestor"/> or one of its child states.
        /// </summary>
        internal static bool IsInState(StateMachine.BaseState state, StateMachine.BaseState ancestor)
        {
            if (ancestor == null)
            {
                return false;
            }

            for (StateMachine.BaseState current = state; current != null; current = current.parent)
            {
                if (current == ancestor)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Restores vanilla behavior, clears operational active flag, plays idle anim,
        /// and resets state machine so duplicants can claim operate chores.
        /// </summary>
        public void StopAutomation()
        {
            if (drivingActiveFlag)
            {
                drivingActiveFlag = false;
                if (operational != null)
                {
                    operational.SetActive(false);
                }

                if (animController == null)
                {
                    animController = WorkAnim.ControllerOf(gameObject);
                }

                if (animController != null)
                {
                    WorkAnim.PlayIdle(animController);
                }

                if (refinery == null)
                {
                    refinery = GetComponent<OilRefinery>();
                }

                if (refinery != null)
                {
                    OilRefinery.StatesInstance smi = refinery.smi ?? gameObject.GetSMI<OilRefinery.StatesInstance>();
                    if (smi != null && smi.IsRunning())
                    {
                        smi.StopSM("switch manual");
                        smi.StartSM();
                    }
                }
            }
        }

        /// <summary>
        /// Mirrors the "ready" state of the refinery onto its operational
        /// active flag five times per second, which is the same cadence the
        /// vanilla pressure test uses.
        /// </summary>
        /// <param name="dt">Elapsed simulated seconds.</param>
        public void Sim200ms(float dt)
        {
            if (converter == null) converter = GetComponent<ElementConverter>();
            SyncEfficiency(converter);

            if (!AutoMachineOptions.IsEnabledFor(gameObject, OilRefineryConfig.ID) &&
                !AutoMachineOptions.IsEnabledFor(gameObject, "OILREFINERY") &&
                !AutoMachineOptions.IsEnabledFor(gameObject, "OilRefinery"))
            {
                StopAutomation();
                return;
            }

            SafeInvoke.Try("Oil Refinery automatic operation", delegate
            {
                if (refinery == null) refinery = GetComponent<OilRefinery>();
                if (operational == null) operational = GetComponent<Operational>();
                if (refinery == null || operational == null)
                {
                    return;
                }

                OilRefinery.StatesInstance smi = refinery.smi ?? gameObject.GetSMI<OilRefinery.StatesInstance>();
                if (smi == null || !smi.IsRunning())
                {
                    return;
                }

                bool shouldRun = operational.IsOperational && IsInState(smi.GetCurrentState(), smi.sm.ready);

                if (shouldRun)
                {
                    // Dislodge any Duplicant or Bionic Duplicant currently operating the machine
                    if (workable == null) workable = GetComponent<Workable>();
                    if (workable != null && workable.worker != null)
                    {
                        workable.StopWork(workable.worker, true);
                    }

                    // Suppress and cancel all operate chores on this refinery
                    ChoreSuppression.CancelOperateChores(gameObject);
                }

                if (shouldRun != drivingActiveFlag || (operational != null && operational.IsActive != shouldRun))
                {
                    drivingActiveFlag = shouldRun;
                    if (operational != null)
                    {
                        operational.SetActive(shouldRun);
                    }

                    // Vanilla plays the refining animation from the Duplicant's
                    // chore; drive it here so an automated refinery is not idle
                    // looking while it produces.
                    if (animController == null)
                    {
                        animController = WorkAnim.ControllerOf(gameObject);
                    }

                    if (shouldRun)
                    {
                        WorkAnim.PlayWorking(animController);
                    }
                    else
                    {
                        WorkAnim.PlayIdle(animController);
                    }
                }
            });
        }

        /// <summary>Releases the active flag when the building disappears.</summary>
        protected override void OnCleanUp()
        {
            if (drivingActiveFlag && operational != null)
            {
                operational.SetActive(false);
            }

            base.OnCleanUp();
        }
    }
}