// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.
// Original mod concept: "AutoMachine" (Steam Workshop id 2992024030).

using AutoMachineRebuilt.Util;

namespace AutoMachineRebuilt.Components
{
    /// <summary>
    /// Automates the Telescope and the (Enclosed) Telescope of the cluster
    /// map.
    ///
    /// The cluster telescope asserts inside <c>GetAnalyzeTarget</c> that a
    /// target was assigned before any work tick happens; ticking it without a
    /// target aborts the game in debug builds and produces a broken analysis
    /// otherwise. The controller therefore refuses to drive the workable until
    /// the telescope itself reports a target, and rechecks that condition on
    /// every step so a target that disappears (revealed by another telescope,
    /// destroyed rocket) stops the automation instead of crashing it.
    /// </summary>
    public class AutoTelescopeController : AutoWorkableController
    {
        /// <summary>
        /// Accepts the telescope work only when the building has a valid
        /// analysis target.
        /// </summary>
        /// <param name="workable">Workable published by the telescope.</param>
        protected override bool IsWorkAllowed(Workable workable)
        {
            ClusterTelescope.Instance cluster = gameObject.GetSMI<ClusterTelescope.Instance>();
            if (cluster != null)
            {
                return StateMachineUtil.BoolField(cluster, "m_hasAnalyzeTarget") ||
                       StateMachineUtil.CallBool(cluster, "HasValidAnalyzeTarget", false);
            }

            return true;
        }
    }
}
