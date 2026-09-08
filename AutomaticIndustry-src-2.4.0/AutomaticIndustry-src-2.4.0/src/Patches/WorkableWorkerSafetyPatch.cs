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

    /// <summary>
    /// Prevents 'Assert failed: Anim overrides containing additional symbols require a symbol override controller.'
    /// When automated fetchers (e.g. SolidTransferArm under No Manual Delivery) start chores on items
    /// with override anims, StandardWorker attempts to attach override animations to the arm's controller.
    /// Because SolidTransferArm lacks a SymbolOverrideController, KAnimControllerBase asserts and crashes.
    /// This prefix completely suppresses attaching override animations on workers that do not use multi-tool
    /// or lack a SymbolOverrideController.
    /// </summary>
    [HarmonyPatch(typeof(StandardWorker), "AttachOverrideAnims")]
    internal static class StandardWorkerAttachOverrideAnimsPatch
    {
        public static bool Prefix(StandardWorker __instance, KAnimControllerBase worker_controller)
        {
            if (__instance != null && (!__instance.UsesMultiTool() || worker_controller == null || worker_controller.GetComponent<SymbolOverrideController>() == null))
            {
                return false;
            }
            return true;
        }
    }
}
