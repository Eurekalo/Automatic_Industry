// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.
// Original mod concept: "AutoMachine" (Steam Workshop id 2992024030).

using System.Collections.Generic;
using AutoMachineRebuilt.Util;
using UnityEngine;

namespace AutoMachineRebuilt.Components
{
    /// <summary>
    /// Building specific "empty me" paths of the base game.
    ///
    /// Some buildings must not be emptied by dropping their storage: they keep
    /// ingredients, conduit output and the finished product inside one and the
    /// same storage component. Dropping everything spills liquids on the floor
    /// and throws away the input material - the reported Gleaner, Desalinator
    /// and Ice Liquefier problems.
    ///
    /// For those buildings the mod reproduces the vanilla emptying task: it
    /// waits for the very state in which the base game asks a Duplicant to
    /// empty the machine, then runs the vanilla drop callback and lets the
    /// state machine continue its normal cycle.
    ///
    /// All three buildings are reached through their strongly typed state
    /// machine instance instead of reflection, so a renamed private field can
    /// never silently disable the release again.
    /// </summary>
    internal static class VanillaEmptyPaths
    {
        /// <summary>
        /// Runs the vanilla release path of a building when one exists.
        /// </summary>
        /// <param name="go">Building game object.</param>
        /// <returns>
        /// <c>true</c> when this building owns a dedicated release path, in
        /// which case the generic storage drop must not run - regardless of
        /// whether the building currently had something to release.
        /// </returns>
        internal static bool TryRelease(GameObject go)
        {
            if (go == null)
            {
                return false;
            }

            IceKettle.Instance kettle = go.GetSMI<IceKettle.Instance>();
            if (kettle != null)
            {
                ReleaseIceKettle(go, kettle);
                return true;
            }

            if (go.GetComponent<Desalinator>() != null)
            {
                return HardThresholdRelease.TryReleaseDesalinator(go);
            }

            return false;
        }

        /// <summary>
        /// Empties the Ice Liquefier (Ice Kettle).
        ///
        /// The building owns three storages: lumber, ice and the finished
        /// liquid. Melting fills the liquid tank batch by batch and vanilla
        /// keeps melting until the tank cannot hold another batch - only then
        /// does a Duplicant carry the bottled liquid out. The automation
        /// mirrors that lifecycle: it stays idle while the tank still has room
        /// and drops the already bottled liquid once the tank is full, so the
        /// machine can immediately resume melting. Nothing is ever dumped as
        /// free flowing liquid and neither input storage is touched.
        /// </summary>
        /// <param name="go">Building game object.</param>
        /// <param name="smi">Ice kettle state machine instance.</param>
        private static void ReleaseIceKettle(GameObject go, IceKettle.Instance smi)
        {
            if (!smi.IsRunning() || smi.LiquidTankHasCapacityForNextBatch)
            {
                return;
            }

            Storage[] storages = go.GetComponents<Storage>();
            if (storages.Length < 3)
            {
                return;
            }

            // Vanilla component order: fuel, kettle (solids), liquid output.
            if (DropStoredItems(storages[2], Tag.Invalid) > 0)
            {
                Log.Verbose("Automated Ice Liquefier emptying: dropped the bottled liquid");
            }
        }

        /// <summary>
        /// Empties the Desalinator.
        ///
        /// Salt water is converted continuously and the leftover salt piles up
        /// inside the shared storage, which also buffers the liquid input and
        /// the water output on their way to the pipes. Vanilla only asks for a
        /// Duplicant once the salt capacity is reached
        /// (<c>SaltStorageLeft &lt;= 0</c>), and its empty callback drops the
        /// salt only. The automation reproduces exactly that: it drops the
        /// stored salt and resets the salt capacity the same way the vanilla
        /// callback does, so the machine resumes converting immediately.
        /// Emptying a machine that is not full stays a manual player
        /// interaction and is deliberately never taken over.
        /// </summary>
        /// <param name="go">Building game object.</param>
        /// <param name="desalinator">Desalinator component of the building.</param>
        private static void ReleaseDesalinator(Desalinator.StatesInstance smi)
        {
            // fullWaitingForEmpty is the only state in which vanilla asks for
            // a Duplicant because the salt capacity is used up. The
            // "earlyWaitingForEmpty" state belongs to a player requested
            // emptying and stays a manual interaction.
            if (!smi.IsRunning() || smi.GetCurrentState() != smi.sm.fullWaitingForEmpty)
            {
                return;
            }

            // Completing the real Workable preserves the WorkChore ordering:
            // OnEmptyComplete receives the live chore, drops salt, resets the
            // state and increments the vanilla maintenance counter. Calling
            // the private callback with null left full machines accumulating
            // hidden salt until a save reload.
            if (!CompleteEmptyWorkable(smi, "DesalinatorWorkableEmpty"))
            {
                return;
            }

            Log.Verbose("Automated Desalinator emptying: ran the vanilla empty callback");
        }

        /// <summary>
        /// Completes a building's exact vanilla emptying Workable without a
        /// worker. Both supported Workables are null-worker safe; their owning
        /// state machines and chores still run all vanilla completion hooks.
        /// </summary>
        /// <param name="smi">Owning state machine instance.</param>
        /// <param name="expectedTypeName">Exact empty Workable type name.</param>
        /// <returns><c>true</c> when the Workable was completed.</returns>
        private static bool CompleteEmptyWorkable(StateMachine.Instance smi, string expectedTypeName)
        {
            if (smi == null || smi.gameObject == null)
            {
                return false;
            }

            Component[] components = smi.gameObject.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                Workable workable = component as Workable;
                if (workable == null || component.GetType().Name != expectedTypeName)
                {
                    continue;
                }

                workable.CompleteWork(null);
                return true;
            }

            Log.Warn("Could not resolve vanilla " + expectedTypeName + "; automated emptying disabled");
            return false;
        }

        /// <summary>
        /// Drops stored items one by one, which keeps every item a real
        /// pickupable - unlike <c>Storage.DropAll</c> with liquid dumping, which
        /// spills bottled liquid onto the floor - and restores their normal
        /// pickup interaction afterwards so the colony counts them again.
        /// </summary>
        /// <param name="storage">Storage to empty.</param>
        /// <param name="filter">Tag to drop, or <see cref="Tag.Invalid"/> for every item.</param>
        /// <returns>Number of items that were dropped.</returns>
        private static int DropStoredItems(Storage storage, Tag filter)
        {
            if (storage == null || storage.IsEmpty())
            {
                return 0;
            }

            List<GameObject> candidates = new List<GameObject>(storage.items);
            List<GameObject> dropped = new List<GameObject>(candidates.Count);
            for (int i = 0; i < candidates.Count; i++)
            {
                GameObject item = candidates[i];
                if (item == null || (filter.IsValid && !item.HasTag(filter)))
                {
                    continue;
                }

                GameObject result = storage.Drop(item);
                if (result != null)
                {
                    dropped.Add(result);
                }
            }

            RestoreDroppedInteractions(dropped);
            return dropped.Count;
        }

        /// <summary>
        /// Restores the normal pickup behaviour of items the mod dropped.
        ///
        /// Bottled liquids stored inside a building carry the
        /// <see cref="GameTags.LiquidSource"/> tag and point their pickup
        /// interaction at the building's own workable. Once the bottle lies on
        /// the floor it has to behave like any other pickupable again,
        /// otherwise Duplicants cannot fetch it and the resource never shows up
        /// in the colony statistics.
        /// </summary>
        /// <param name="items">Items that were just dropped.</param>
        internal static void RestoreDroppedInteractions(List<GameObject> items)
        {
            if (items == null)
            {
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                GameObject item = items[i];
                if (item == null)
                {
                    continue;
                }

                Pickupable pickupable = item.GetComponent<Pickupable>();
                if (pickupable != null)
                {
                    bool wasLiquidSource = pickupable.HasTag(GameTags.LiquidSource);
                    pickupable.RemoveTag(GameTags.LiquidSource);
                    pickupable.targetWorkable = pickupable;

                    if (wasLiquidSource)
                    {
                        pickupable.SetOffsetTable(OffsetGroups.InvertedStandardTable);
                    }
                }

                // If dropped inside a solid foundation tile, lift it up into open space so it falls naturally
                int cell = Grid.PosToCell(item.transform.position);
                if (Grid.IsValidCell(cell) && Grid.Solid[cell])
                {
                    int aboveCell = Grid.CellAbove(cell);
                    if (Grid.IsValidCell(aboveCell) && !Grid.Solid[aboveCell])
                    {
                        item.transform.position = Grid.CellToPosCBC(aboveCell, Grid.SceneLayer.Ore);
                    }
                }
            }
        }
    }
}
