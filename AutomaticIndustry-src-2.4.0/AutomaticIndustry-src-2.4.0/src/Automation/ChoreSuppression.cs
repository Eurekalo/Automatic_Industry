// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System;
using System.Collections.Generic;
using AutoMachineRebuilt.Patches;
using AutoMachineRebuilt.Util;

namespace AutoMachineRebuilt.Automation
{
    /// <summary>
    /// Suppresses Duplicant "operate the machine" chores on buildings that are
    /// currently being driven by an automation controller.
    ///
    /// Rather than calling chore.Cancel(...) in a high-frequency polling loop
    /// (which triggers state machine failure exits, re-entrant chore recreation loops,
    /// and massive ReportManager Note allocation leaks over hundreds of cycles),
    /// suppression attaches a dynamic precondition to candidate chores.
    ///
    /// The precondition evaluates whether automation is enabled:
    /// - While automated: Duplicants completely ignore the chore.
    /// - When reverted to manual: Duplicants immediately take the chore.
    /// - The chore is never repeatedly created or destroyed, eliminating memory leaks.
    /// </summary>
    internal static class ChoreSuppression
    {
        /// <summary>
        /// Chore type identifiers that represent "operating the machine" and
        /// must be suppressed when automation is driving the building.
        /// </summary>
        internal static readonly HashSet<string> OperateChoreIds =
            new HashSet<string>(StringComparer.Ordinal)
            {
                "Fabricate",
                "Cook",
                "Work",
                "Research",
                "MachineTinker",
                "PowerTinker",
                "PowerFabricate",
                "FarmingFabricate",
                "FarmTinker",
                "Art",
                "EmptyDesalinator",
                "Spice",
                "IceCooledFan",
                "LiquidCooledFan",
                "GeneratePower",
                "AnalyzeSeed",
                "Depressurize",
                "ScrubOre",
                "FlipCompost",
                "Ranch"
            };

        /// <summary>
        /// Checks whether the given chore already carries the mod suppression precondition.
        /// </summary>
        internal static bool HasSuppressionPrecondition(Chore chore)
        {
            if (chore == null) return false;
            var list = chore.GetPreconditions();
            if (list == null) return false;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].condition.id == StationChoreSuppressionPatches.DisabledByModPrecondition.id)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Applies the suppression precondition to the chore, dislodging active workers if necessary.
        /// </summary>
        internal static void SuppressChore(Chore chore)
        {
            if (chore == null || chore.isComplete) return;

            // If a Duplicant is currently working this machine, stop them safely
            if (chore.InProgress() && chore.driver != null)
            {
                chore.driver.StopChore();
            }

            if (!HasSuppressionPrecondition(chore))
            {
                chore.AddPrecondition(StationChoreSuppressionPatches.DisabledByModPrecondition, null);
            }
        }

        /// <summary>
        /// Suppresses all "operate" chores that target the given building.
        ///
        /// Called from <see cref="Components.AutoWorkControllerBase"/> while the controller is active.
        /// </summary>
        /// <param name="building">Building whose chores to inspect.</param>
        internal static void SuppressOperateChores(UnityEngine.GameObject building)
        {
            if (building == null)
            {
                return;
            }

            // 1. ChoreProvider is the per-building chore registry.
            ChoreProvider provider = building.GetComponent<ChoreProvider>();
            if (provider != null && provider.choreWorldMap != null && provider.choreWorldMap.Count > 0)
            {
                foreach (var pair in provider.choreWorldMap)
                {
                    List<Chore> list = pair.Value;
                    if (list == null || list.Count == 0) continue;

                    for (int i = 0; i < list.Count; i++)
                    {
                        Chore chore = list[i];
                        if (chore == null || chore.isComplete) continue;

                        string typeId = chore.choreType != null ? chore.choreType.Id : null;
                        if (!string.IsNullOrEmpty(typeId) && OperateChoreIds.Contains(typeId))
                        {
                            SuppressChore(chore);
                        }
                    }
                }
            }

            // 2. Component chore fields and state machine data tables
            LocalChoreProbe.SuppressLocalOperateChores(building, OperateChoreIds);
        }

        /// <summary>
        /// Backward-compatible alias for <see cref="SuppressOperateChores"/>.
        /// </summary>
        /// <param name="building">Building whose chores to inspect.</param>
        internal static void CancelOperateChores(UnityEngine.GameObject building)
        {
            SuppressOperateChores(building);
        }
    }
}
