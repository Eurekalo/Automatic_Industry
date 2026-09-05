// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.
// Original mod concept: "AutoMachine" (Steam Workshop id 2992024030).

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AutoMachineRebuilt.Automation
{
    /// <summary>
    /// Finds the work chore a single building currently offers - by looking at
    /// that one building only.
    ///
    /// Why the automation needs this: the vanilla "may this building be worked
    /// right now?" logic lives inside the chore the building publishes
    /// (ingredients delivered, operational, correct room, research selected,
    /// batteries not full, ...). Re-implementing those conditions would drift
    /// from the base game on every update, so the automation waits until the
    /// building itself offers the chore.
    ///
    /// Why it is not a Harmony patch on <c>ChoreProvider</c>: patching the
    /// provider observes every chore of every object in the game and therefore
    /// collides with mods that reorder, wrap or replace chores. This probe is
    /// strictly local. It reads two places where the base game keeps the chore
    /// of a building:
    /// <list type="bullet">
    /// <item>a private <see cref="Chore"/> field on one of the building
    /// components (<c>ManualGenerator.chore</c>, <c>ResearchCenter.chore</c>,
    /// <c>Tinkerable.chore</c>, ...),</item>
    /// <item>the <c>dataTable</c> slot of a state machine instance, which is
    /// where <c>GameStateMachine.State.ToggleChore</c> stores the chore it
    /// created for the current state.</item>
    /// </list>
    /// Nothing is patched, nothing is cached across buildings, and a building
    /// that offers no chore simply yields <c>null</c>.
    /// </summary>
    internal static class LocalChoreProbe
    {
        /// <summary>Chore fields per component type, resolved once.</summary>
        private static readonly Dictionary<Type, FieldInfo[]> ChoreFieldCache =
            new Dictionary<Type, FieldInfo[]>();

        /// <summary>Binding flags covering public and private instance fields.</summary>
        private const BindingFlags InstanceFields =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// Returns the workable of the work chore currently offered by
        /// <paramref name="building"/>, or <c>null</c> when the building has
        /// nothing to do.
        /// </summary>
        /// <param name="building">Building game object to inspect.</param>
        internal static Workable FindPendingWorkable(GameObject building)
        {
            if (building == null)
            {
                return null;
            }

            Workable fromComponents = ScanComponents(building);
            return fromComponents != null ? fromComponents : ScanStateMachines(building);
        }

        /// <summary>Scans chore fields declared by the building components.</summary>
        /// <param name="building">Building game object to inspect.</param>
        private static Workable ScanComponents(GameObject building)
        {
            Component[] components = building.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component == null)
                {
                    continue;
                }

                FieldInfo[] fields = ChoreFieldsOf(component.GetType());
                for (int f = 0; f < fields.Length; f++)
                {
                    Workable workable = Accept(fields[f].GetValue(component) as Chore, building);
                    if (workable != null)
                    {
                        return workable;
                    }
                }
            }

            return null;
        }

        /// <summary>Scans the data tables of the building state machines.</summary>
        /// <param name="building">Building game object to inspect.</param>
        private static Workable ScanStateMachines(GameObject building)
        {
            StateMachineController controller = building.GetComponent<StateMachineController>();
            if (controller == null)
            {
                return null;
            }

            foreach (StateMachine.Instance smi in controller)
            {
                if (smi == null || smi.dataTable == null)
                {
                    continue;
                }

                object[] dataTable = smi.dataTable;
                for (int i = 0; i < dataTable.Length; i++)
                {
                    Workable workable = Accept(dataTable[i] as Chore, building);
                    if (workable != null)
                    {
                        return workable;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Validates a candidate chore and returns the workable it drives.
        /// Delivery and player order chores are rejected: those belong to the
        /// Duplicants and must never be executed by the automation.
        /// </summary>
        /// <param name="chore">Candidate chore.</param>
        /// <param name="building">Building the chore has to belong to.</param>
        private static Workable Accept(Chore chore, GameObject building)
        {
            if (chore == null || chore.isComplete || chore.isNull || chore.InProgress())
            {
                return null;
            }

            if (chore is FetchChore || chore is FetchAreaChore)
            {
                return null;
            }

            Workable workable = chore.target as Workable;
            if (workable == null || workable.gameObject != building)
            {
                return null;
            }

            return workable;
        }

        /// <summary>Resolves and caches the chore fields of a component type.</summary>
        /// <param name="type">Component type to inspect.</param>
        private static FieldInfo[] ChoreFieldsOf(Type type)
        {
            FieldInfo[] cached;
            if (ChoreFieldCache.TryGetValue(type, out cached))
            {
                return cached;
            }

            List<FieldInfo> fields = new List<FieldInfo>();
            for (Type current = type; current != null && current != typeof(object);
                 current = current.BaseType)
            {
                FieldInfo[] declared = current.GetFields(InstanceFields | BindingFlags.DeclaredOnly);
                for (int i = 0; i < declared.Length; i++)
                {
                    if (typeof(Chore).IsAssignableFrom(declared[i].FieldType))
                    {
                        fields.Add(declared[i]);
                    }
                }
            }

            cached = fields.ToArray();
            ChoreFieldCache[type] = cached;
            return cached;
        }

        /// <summary>
        /// Cancels any operate chores found on the building components or state machines.
        /// </summary>
        internal static void CancelLocalOperateChores(GameObject building, HashSet<string> operateChoreIds)
        {
            if (building == null || operateChoreIds == null)
            {
                return;
            }

            List<Chore> toCancel = null;

            // 1. Scan component fields
            Component[] components = building.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component == null)
                {
                    continue;
                }

                FieldInfo[] fields = ChoreFieldsOf(component.GetType());
                for (int f = 0; f < fields.Length; f++)
                {
                    Chore chore = fields[f].GetValue(component) as Chore;
                    if (chore != null && !chore.isComplete && chore.choreType != null &&
                        operateChoreIds.Contains(chore.choreType.Id))
                    {
                        if (toCancel == null)
                        {
                            toCancel = new List<Chore>();
                        }

                        if (!toCancel.Contains(chore))
                        {
                            toCancel.Add(chore);
                        }
                    }
                }
            }

            // 2. Scan state machine data tables
            StateMachineController controller = building.GetComponent<StateMachineController>();
            if (controller != null)
            {
                // Snapshot the state machine instances to avoid concurrent modification issues
                List<StateMachine.Instance> smis = new List<StateMachine.Instance>();
                foreach (StateMachine.Instance smi in controller)
                {
                    if (smi != null)
                    {
                        smis.Add(smi);
                    }
                }

                for (int s = 0; s < smis.Count; s++)
                {
                    StateMachine.Instance smi = smis[s];
                    if (smi == null || smi.dataTable == null)
                    {
                        continue;
                    }

                    object[] dataTable = smi.dataTable;
                    for (int i = 0; i < dataTable.Length; i++)
                    {
                        Chore chore = dataTable[i] as Chore;
                        if (chore != null && !chore.isComplete && chore.choreType != null &&
                            operateChoreIds.Contains(chore.choreType.Id))
                        {
                            if (toCancel == null)
                            {
                                toCancel = new List<Chore>();
                            }

                            if (!toCancel.Contains(chore))
                            {
                                toCancel.Add(chore);
                            }
                        }
                    }
                }
            }

            // 3. Cancel all gathered operate chores
            if (toCancel != null)
            {
                for (int i = 0; i < toCancel.Count; i++)
                {
                    Chore chore = toCancel[i];
                    if (chore != null && !chore.isComplete)
                    {
                        chore.Cancel("Automated by AutoMachine Rebuilt");
                    }
                }
            }
        }
    }
}
