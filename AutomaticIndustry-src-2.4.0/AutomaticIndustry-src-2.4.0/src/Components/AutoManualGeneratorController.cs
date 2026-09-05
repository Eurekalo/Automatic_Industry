// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.
// Original mod concept: "AutoMachine" (Steam Workshop id 2992024030).

using AutoMachineRebuilt.Automation;
using AutoMachineRebuilt.Util;

namespace AutoMachineRebuilt.Components
{
    /// <summary>
    /// Runs the Manual Generator without a Duplicant.
    ///
    /// The base game decides in <c>ManualGenerator.EnergySim200ms</c> whether
    /// the wheel should run: it looks at the circuit, at the presence of
    /// consumers and at the lowest battery charge on that circuit compared to
    /// the building's own refill slider, and only then creates the
    /// <c>GeneratePower</c> chore. The controller reuses that decision by
    /// watching the chore the building publishes itself, which is why a
    /// freshly swapped empty battery restarts the generator immediately and a
    /// full circuit stops it - exactly like a Duplicant would.
    ///
    /// Power generation itself is not simulated by the mod: setting
    /// <c>Operational.IsActive</c> makes the vanilla energy simulation
    /// generate the joules and makes the state machine play the working
    /// animation, so wattage, heat and runtime stay vanilla.
    /// </summary>
    public class AutoManualGeneratorController : AutoWorkControllerBase
    {
        private ManualGenerator generator;
        private bool driving;

        /// <summary>Caches the generator component.</summary>
        protected override void Prepare()
        {
            base.Prepare();
            generator = GetComponent<ManualGenerator>();
            ChoreSuppression.CancelOperateChores(gameObject);
        }

        /// <summary>Starts or stops the wheel following the vanilla chore.</summary>
        /// <param name="dt">Seconds since the previous step.</param>
        protected override void Step(float dt)
        {
            if (generator == null)
            {
                return;
            }

            ChoreSuppression.CancelOperateChores(gameObject);

            bool wanted = IsOperational() &&
                          StateMachineUtil.Field(generator, "chore") != null;

            if (wanted == driving)
            {
                return;
            }

            driving = wanted;
            SetActive(wanted);
            Log.Verbose((wanted ? "Started" : "Stopped") + " automated power generation on " + optionKey);
        }

        /// <summary>Hands the wheel back to the base game.</summary>
        public override void StopAutomation()
        {
            if (driving)
            {
                driving = false;
                SetActive(false);
            }

            base.StopAutomation();
        }
    }
}
