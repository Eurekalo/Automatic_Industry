// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using AutoMachineRebuilt.Components;
using HarmonyLib;

namespace AutoMachineRebuilt.Patches
{
    /// <summary>
    /// Prevents multiple automated ranch stations (Grooming, Shearing, Milking, land or aquatic)
    /// in the same room from stealing critters from each other.
    ///
    /// In vanilla ONI, CanRanchableBeRanchedAtRanchStation checks:
    ///   if (ranchable.TargetRanchStation != this)
    ///       flag = !ranchable.TargetRanchStation.IsRunning() || !ranchable.TargetRanchStation.HasRancher;
    ///
    /// Because automated stations have HasRancher == false, vanilla allowed any station in the room
    /// to steal a critter that was already queued by another station. This caused critters to rapidly
    /// oscillate between "Excited" and "Being Groomed/Normal" every frame.
    ///
    /// This patch treats active AutoRanchStation instances as owning their assigned critters,
    /// ensuring each critter only attends one station at a time.
    /// </summary>
    [HarmonyPatch(typeof(RanchStation.Instance), "CanRanchableBeRanchedAtRanchStation")]
    public static class RanchStation_CanRanchableBeRanchedAtRanchStation_Patch
    {
        public static bool Prefix(RanchStation.Instance __instance, RanchableMonitor.Instance ranchable, ref bool __result)
        {
            if (ranchable == null || ranchable.IsNullOrStopped())
            {
                __result = false;
                return false;
            }

            // If this critter is already assigned to a DIFFERENT running ranch station with an active automated controller,
            // do NOT allow this station to steal it!
            if (ranchable.TargetRanchStation != null && ranchable.TargetRanchStation != __instance)
            {
                if (ranchable.TargetRanchStation.IsRunning())
                {
                    AutoRanchStation autoRanch = ranchable.TargetRanchStation.GetComponent<AutoRanchStation>();
                    if (ranchable.TargetRanchStation.HasRancher || (autoRanch != null && autoRanch.enabled))
                    {
                        __result = false;
                        return false;
                    }
                }
            }

            return true; // proceed to vanilla checks
        }
    }
}
