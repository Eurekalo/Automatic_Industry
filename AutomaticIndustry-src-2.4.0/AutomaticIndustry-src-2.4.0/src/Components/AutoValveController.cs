// Copyright (c) 2026 Automatic Industry contributors. Licensed under the MIT License.
// Original mod concept: "AutoMachine" (Steam Workshop id 2992024030).

using AutoMachineRebuilt.Util;

namespace AutoMachineRebuilt.Components
{
    /// <summary>
    /// Applies the flow setting of the Liquid Valve and the Gas Valve without
    /// a Duplicant.
    ///
    /// Vanilla flow: <c>Valve.ChangeFlow</c> stores the requested value in its
    /// private <c>desiredFlow</c> field and creates a Toggle work chore. Only
    /// when a Duplicant completes that chore does <c>Valve.UpdateFlow</c> run,
    /// which copies the desired value into <c>ValveBase.CurrentFlow</c>,
    /// refreshes the valve animation, cancels the chore and clears the
    /// "valve request" status items.
    ///
    /// <c>UpdateFlow</c> is public and takes no worker: it is the very method
    /// the base game also calls directly in instant build mode. The controller
    /// therefore performs exactly the vanilla completion path as soon as the
    /// requested flow differs from the flow the valve currently applies, and
    /// touches nothing else - conduit throughput, animation ranges and the
    /// user menu keep working as shipped.
    /// </summary>
    public class AutoValveController : AutoWorkControllerBase
    {
        /// <summary>Tolerance used when comparing two flow rates in kg/s.</summary>
        private const float FlowEpsilon = 0.0001f;

        private Valve valve;
        private ValveBase valveBase;

        /// <summary>Caches the two vanilla valve components.</summary>
        protected override void Prepare()
        {
            base.Prepare();
            valve = GetComponent<Valve>();
            valveBase = GetComponent<ValveBase>();
        }

        /// <summary>Applies a pending flow change.</summary>
        /// <param name="dt">Seconds since the previous step.</param>
        protected override void Step(float dt)
        {
            if (valve == null || valveBase == null)
            {
                return;
            }

            // A Duplicant that already started the vanilla chore always wins;
            // its completion runs the same UpdateFlow a moment later.
            if (valve.GetWorker() != null)
            {
                return;
            }

            float desired = valve.DesiredFlow;
            if (UnityEngine.Mathf.Abs(desired - valveBase.CurrentFlow) <= FlowEpsilon)
            {
                return;
            }

            valve.UpdateFlow();
            Log.Verbose("Valve flow applied automatically: " + desired + " kg/s");
        }

        /// <summary>
        /// Valves only apply flow settings and do not have an operational active flag or work animation.
        /// </summary>
        public override void StopAutomation()
        {
            // Do not call SetActive(false) or StopAnimation() on valves
        }
    }
}
