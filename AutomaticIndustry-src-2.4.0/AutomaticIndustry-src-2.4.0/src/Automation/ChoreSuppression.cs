// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System;
using System.Collections.Generic;
using AutoMachineRebuilt.Util;

namespace AutoMachineRebuilt.Automation
{
    /// <summary>
    /// Cancels Duplicant "operate the machine" chores on buildings that are
    /// currently being driven by an automation controller.
    ///
    /// The suppression is deliberately conservative:
    /// <list type="bullet">
    /// <item>Only chores whose type is in the <see cref="OperateChoreIds"/>
    /// set are cancelled. Every delivery, fetch, sweep, mop, repair and
    /// deconstruct chore is preserved.</item>
    /// <item>Chores are only cancelled while the automation is actively
    /// driving the building (the controller has set the building to active).
    /// When the automation is turned off the vanilla state machine recreates
    /// the operate chore on its next cycle.</item>
    /// <item>Global chore providers and chore types are never modified.</item>
    /// </list>
    /// </summary>
    internal static class ChoreSuppression
    {
        /// <summary>
        /// Chore type identifiers that represent "operating the machine" and
        /// must be cancelled when automation is driving the building.
        /// </summary>
        private static readonly HashSet<string> OperateChoreIds =
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
                "FlipCompost"
            };

        /// <summary>
        /// Cancels all "operate" chores that target the given building.
        ///
        /// Called from <see cref="Components.AutoWorkControllerBase"/> after
        /// each successful automation step while the controller is active.
        /// </summary>
        /// <param name="building">Building whose chores to inspect.</param>
        internal static void CancelOperateChores(UnityEngine.GameObject building)
        {
            if (building == null)
            {
                return;
            }

            SafeInvoke.Try("Chore suppression", delegate
            {
                // 1. ChoreProvider is the per-building chore registry. Every
                // chore the building publishes is visible here.
                ChoreProvider provider = building.GetComponent<ChoreProvider>();
                if (provider != null && provider.choreWorldMap != null && provider.choreWorldMap.Count > 0)
                {
                    foreach (var pair in provider.choreWorldMap)
                    {
                        List<Chore> list = pair.Value;
                        if (list == null || list.Count == 0) continue;

                        for (int i = list.Count - 1; i >= 0; i--)
                        {
                            if (i >= list.Count) continue;
                            Chore chore = list[i];
                            if (chore == null || chore.isComplete)
                            {
                                continue;
                            }

                            string typeId = chore.choreType != null ? chore.choreType.Id : null;
                            if (string.IsNullOrEmpty(typeId))
                            {
                                continue;
                            }

                            if (OperateChoreIds.Contains(typeId))
                            {
                                chore.Cancel("Automated by AutoMachine Rebuilt");
                            }
                        }
                    }
                }

                // 2. Component chore fields and state machine data tables
                LocalChoreProbe.CancelLocalOperateChores(building, OperateChoreIds);
            });
        }
    }
}
