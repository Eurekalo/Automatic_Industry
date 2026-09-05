// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using HarmonyLib;

namespace AutoMachineRebuilt.Patches
{
    /// <summary>
    /// Prevents animation crashes when non-duplicant workers (such as Solid Transfer Arms
    /// or robotic fetch arms) interact with buildings or items that define duplicant work anims
    /// or multitool controllers.
    ///
    /// Non-duplicant workers have usesMultiTool = false and lack Navigator and AnimEventHandler
    /// components. Attempting to initialize MultitoolController or apply duplicant override anims
    /// for them causes NullReferenceExceptions and animation glitches.
    /// </summary>
    [HarmonyPatch(typeof(Workable), "GetAnim")]
    internal static class WorkableWorkerSafetyPatch
    {
        public static bool Prefix(WorkerBase worker, ref Workable.AnimInfo __result)
        {
            if (worker != null && !worker.UsesMultiTool())
            {
                __result = default(Workable.AnimInfo);
                return false;
            }
            return true;
        }
    }
}
