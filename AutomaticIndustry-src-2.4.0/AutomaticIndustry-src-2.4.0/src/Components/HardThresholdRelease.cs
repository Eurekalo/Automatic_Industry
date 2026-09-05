// Copyright (c) 2026 Automatic Industry contributors. Licensed under the MIT License.
// Original mod concept: "AutoMachine" (Steam Workshop id 2992024030).

using System.Collections.Generic;
using AutoMachineRebuilt.Util;
using UnityEngine;

namespace AutoMachineRebuilt.Components
{
    /// <summary>
    /// Threshold driven emptying for the two buildings whose vanilla emptying
    /// chore cannot be completed safely without a Duplicant.
    ///
    /// Both the Desalinator and the Gleaner (Milk Fat Separator) share a single
    /// <see cref="Storage"/> for their liquid input, their piped output and the
    /// solid leftover a Duplicant is asked to carry away. Completing their
    /// vanilla <c>Workable</c> with a <c>null</c> worker does not reliably run
    /// the chore callbacks, which is why earlier releases either dropped
    /// nothing (hidden accumulation that spilled on the next save load) or
    /// dropped the whole storage including inputs.
    ///
    /// This class instead reproduces the original mod's approach, which was
    /// crude but correct: watch the building's own capacity counter and, once
    /// the hard threshold is reached, run the exact vanilla drop routine on the
    /// solid product tags only.
    ///
    /// The thresholds are the vanilla capacities themselves:
    /// <list type="bullet">
    /// <item>Desalinator: <c>SaltStorageLeft &lt;= 0</c> (bounded by
    /// <c>maxSalt</c>), the same condition that makes vanilla request an
    /// emptying chore.</item>
    /// <item>Gleaner: <c>MilkFatLimitReached</c>, i.e. the stored solid output
    /// reached <c>MILK_FAT_CAPACITY</c>.</item>
    /// </list>
    /// </summary>
    internal static class HardThresholdRelease
    {
        /// <summary>
        /// Relative safety margin on the salt threshold. The counter is written
        /// by the simulation in discrete steps and can overshoot slightly, so
        /// the release triggers at 99% of the vanilla capacity instead of
        /// waiting for an exact zero that a busy machine may skip.
        /// </summary>
        private const float SaltFillMargin = 0.90f;

        /// <summary>
        /// Empties the Desalinator once its salt capacity is used up.
        /// </summary>
        /// <param name="go">Desalinator game object.</param>
        /// <returns><c>true</c> when the building was handled here.</returns>
        internal static bool TryReleaseDesalinator(GameObject go)
        {
            Desalinator desalinator = go.GetComponent<Desalinator>();
            if (desalinator == null)
            {
                return false;
            }

            Desalinator.StatesInstance smi = go.GetSMI<Desalinator.StatesInstance>();
            Storage storage = go.GetComponent<Storage>();
            if (storage == null)
            {
                return true;
            }

            float capacity = Mathf.Max(0f, desalinator.maxSalt);
            float stored = StoredSolidMass(storage);
            Tag saltTag = GameTagExtensions.Create(SimHashes.Salt);
            float saltMass = storage.GetMassAvailable(saltTag);
            float totalSolid = Mathf.Max(stored, saltMass);

            // Two independent triggers:
            // 1. Hard mass threshold (90% capacity)
            // 2. Vanilla capacity depleted (SaltStorageLeft <= 0)
            // 3. State machine in full/early waiting-for-empty states
            bool thresholdReached = capacity > 0f && totalSolid >= capacity * SaltFillMargin;
            bool vanillaFull = desalinator.SaltStorageLeft <= 0f && totalSolid > 0f;
            bool isWaitingForEmpty = smi != null && (smi.GetCurrentState() == smi.sm.full ||
                                                    smi.GetCurrentState() == smi.sm.fullWaitingForEmpty ||
                                                    smi.GetCurrentState() == smi.sm.earlyEmpty ||
                                                    smi.GetCurrentState() == smi.sm.earlyWaitingForEmpty);

            if (!thresholdReached && !vanillaFull && !isWaitingForEmpty)
            {
                return true;
            }

            // Cancel dupe chore first to prevent dupe pathing conflicts
            if (smi != null && smi.IsRunning())
            {
                SafeInvoke.Try("cancelling Desalinator empty chore", smi.CancelEmptyChore);
            }

            int dropped = DropRemainingSolids(go);
            dropped += DropTagged(storage, saltTag);

            if (smi != null && smi.IsRunning())
            {
                desalinator.SaltStorageLeft = desalinator.maxSalt;
                SafeInvoke.Try("refreshing Desalinator salt capacity", smi.UpdateStorageLeft);
                SafeInvoke.Try("transitioning Desalinator to empty", delegate
                {
                    smi.GoTo(smi.sm.empty);
                });
                desalinator.Trigger((int)GameHashes.OnStorageChange, storage);
            }

            Log.Verbose("Automated Desalinator emptying: dropped " + dropped + " solid item(s)");
            return true;
        }

        /// <summary>
        /// Empties the Desalinator immediately and refreshes its salt
        /// capacity, so the state machine leaves its "waiting to be
        /// emptied" state in the same update.
        /// </summary>
        /// <param name="smi">Desalinator state machine instance.</param>
        /// <returns>Number of released items.</returns>
        internal static int ForceReleaseDesalinator(Desalinator.StatesInstance smi)
        {
            if (smi == null)
            {
                return 0;
            }

            int dropped = DropRemainingSolids(smi.gameObject);
            Tag saltTag = GameTagExtensions.Create(SimHashes.Salt);
            Storage storage = smi.gameObject.GetComponent<Storage>();
            if (storage != null)
            {
                dropped += DropTagged(storage, saltTag);
            }

            smi.master.SaltStorageLeft = smi.master.maxSalt;
            SafeInvoke.Try("cancelling Desalinator empty chore", smi.CancelEmptyChore);
            SafeInvoke.Try("refreshing Desalinator salt capacity", smi.UpdateStorageLeft);
            SafeInvoke.Try("transitioning Desalinator to empty", delegate
            {
                smi.GoTo(smi.sm.empty);
            });
            if (storage != null)
            {
                smi.master.Trigger((int)GameHashes.OnStorageChange, storage);
            }
            return dropped;
        }

        /// <summary>
        /// Total mass of every solid item stored in a building.
        /// </summary>
        /// <param name="storage">Storage of the building.</param>
        private static float StoredSolidMass(Storage storage)
        {
            if (storage == null || storage.items == null)
            {
                return 0f;
            }

            float mass = 0f;
            for (int i = 0; i < storage.items.Count; i++)
            {
                GameObject item = storage.items[i];
                if (item == null || !IsSolid(item))
                {
                    continue;
                }

                PrimaryElement element = item.GetComponent<PrimaryElement>();
                if (element != null)
                {
                    mass += element.Mass;
                }
            }

            return mass;
        }

        /// <summary>
        /// Drops every stored solid item of a building, whatever its tag is.
        ///
        /// Used as the generic fallback of the Gleaner: its recipe list may
        /// grow with future updates and a product that no hard coded tag
        /// matches must still leave the machine instead of piling up (or being
        /// destroyed) inside the shared storage. Liquids and gases are never
        /// touched, so the liquid ingredients and the piped output stay in.
        /// </summary>
        /// <param name="go">Building game object.</param>
        /// <returns>Number of dropped items.</returns>
        internal static int DropRemainingSolids(GameObject go)
        {
            if (go == null)
            {
                return 0;
            }

            Storage[] storages = go.GetComponents<Storage>();
            List<GameObject> dropped = new List<GameObject>();

            for (int s = 0; s < storages.Length; s++)
            {
                Storage storage = storages[s];
                if (storage == null || storage.items == null)
                {
                    continue;
                }

                for (int i = storage.items.Count - 1; i >= 0; i--)
                {
                    GameObject item = storage.items[i];
                    if (item == null || !IsSolid(item))
                    {
                        continue;
                    }

                    GameObject result = storage.Drop(item, true);
                    if (result != null)
                    {
                        dropped.Add(result);
                    }
                }
            }

            VanillaEmptyPaths.RestoreDroppedInteractions(dropped);
            return dropped.Count;
        }

        /// <summary>
        /// Whether a stored item is a solid, which is what a Duplicant would
        /// carry out of the machine.
        /// </summary>
        /// <param name="item">Stored item.</param>
        private static bool IsSolid(GameObject item)
        {
            PrimaryElement element = item.GetComponent<PrimaryElement>();
            if (element == null || element.Element == null)
            {
                return false;
            }

            return element.Element.IsSolid;
        }

        /// <summary>
        /// Drops every stored item carrying the given tag, exactly like the
        /// vanilla empty callbacks do, and restores their pickup interaction.
        /// </summary>
        /// <param name="storage">Storage of the building.</param>
        /// <param name="tag">Product tag that may leave the building.</param>
        /// <returns>Number of dropped items.</returns>
        private static int DropTagged(Storage storage, Tag tag)
        {
            List<GameObject> found = new List<GameObject>();
            storage.Find(tag, found);

            List<GameObject> dropped = new List<GameObject>(found.Count);
            for (int i = 0; i < found.Count; i++)
            {
                GameObject item = found[i];
                if (item == null)
                {
                    continue;
                }

                GameObject result = storage.Drop(item, true);
                if (result != null)
                {
                    dropped.Add(result);
                }
            }

            VanillaEmptyPaths.RestoreDroppedInteractions(dropped);
            return dropped.Count;
        }
    }
}
