// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.
// Original mod concept: "AutoMachine" (Steam Workshop id 2992024030).

using System.Collections.Generic;
using System.Reflection;
using AutoMachineRebuilt.Config;
using AutoMachineRebuilt.Util;
using UnityEngine;

namespace AutoMachineRebuilt.Components
{
    /// <summary>
    /// Empties buildings that finish their production on their own but need a
    /// Duplicant to carry the result out: Dehydrator, Desalinator, Ice
    /// Liquefier, Gleaner (Milk Fat Separator) and any other building whose
    /// "empty" interaction is nothing more than dropping the output storage.
    ///
    /// Buildings with shared input, conduit and product storage are delegated
    /// to <see cref="VanillaEmptyPaths"/> before the generic output path runs.
    /// This preserves their vanilla full-capacity lifecycle and prevents input
    /// liquids or conduit buffers from being released. Other buildings use the
    /// generic output-storage path and keep bottled liquids bottled.
    /// </summary>
    public class AutoStorageReleaseController : AutoWorkControllerBase
    {
        /// <summary>Seconds between two release attempts.</summary>
        private const float ReleaseInterval = 1f;

        private readonly List<GameObject> buffer = new List<GameObject>();
        private readonly List<Storage> inputStorages = new List<Storage>();
        private Storage[] storages;
        private ComplexFabricator fabricator;
        private float cooldown;

        /// <summary>Seconds between two identical release log messages.</summary>
        private const float LogInterval = 60f;

        /// <summary>Seconds left before the next release may be logged.</summary>
        private float logCooldown;

        /// <summary>Caches the storage components of the building.</summary>
        protected override void Prepare()
        {
            base.Prepare();
            storages = GetComponents<Storage>();
            fabricator = GetComponent<ComplexFabricator>();
            CacheInputStorages();
        }

        /// <summary>
        /// Records every storage that a delivery component fills, so the
        /// controller can never dump a building's ingredients.
        ///
        /// The Ice Liquefier is the reason this exists: it owns three
        /// storages - lumber, ice and the finished liquid - and only the last
        /// one is what a Duplicant carries out. Dropping the other two made
        /// delivered lumber or ice spill onto the floor.
        /// </summary>
        private void CacheInputStorages()
        {
            inputStorages.Clear();

            ManualDeliveryKG[] deliveries = GetComponents<ManualDeliveryKG>();
            for (int i = 0; i < deliveries.Length; i++)
            {
                Storage target = ReadDeliveryStorage(deliveries[i]);
                if (target != null && !inputStorages.Contains(target))
                {
                    inputStorages.Add(target);
                }
            }
        }

        /// <summary>
        /// Reads the storage a delivery component was bound to. The field is
        /// private in the game assembly, so it is resolved by reflection and
        /// a missing field simply yields no result.
        /// </summary>
        /// <param name="delivery">Delivery component of the building.</param>
        private static Storage ReadDeliveryStorage(ManualDeliveryKG delivery)
        {
            if (delivery == null)
            {
                return null;
            }

            FieldInfo field = delivery.GetType().GetField(
                "storage",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.FlattenHierarchy);

            return field == null ? null : field.GetValue(delivery) as Storage;
        }

        /// <summary>Drops finished output at a fixed interval.</summary>
        /// <param name="dt">Seconds since the previous step.</param>
        protected override void Step(float dt)
        {
            cooldown -= dt;
            if (cooldown > 0f || storages == null)
            {
                return;
            }

            cooldown = ReleaseInterval;
            logCooldown -= ReleaseInterval;

            // These buildings share storage between ingredients, conduit
            // buffers and finished products. Their dedicated path owns the
            // entire decision, including the "not full yet" case; never fall
            // through to the generic storage scan after recognizing one.
            if (VanillaEmptyPaths.TryRelease(gameObject))
            {
                return;
            }

            int released = 0;

            for (int i = 0; i < storages.Length; i++)
            {
                Storage storage = storages[i];
                if (!ShouldRelease(storage))
                {
                    continue;
                }

                storage.DropAll(vent_gas: false, dump_liquid: false, offset: Vector3.zero,
                                do_disease_transfer: true, collect_dropped_items: buffer);
                released += buffer.Count;
                VanillaEmptyPaths.RestoreDroppedInteractions(buffer);
                buffer.Clear();
            }

            // A continuously producing building (Desalinator, Gleaner) would
            // otherwise write one line per second into Player.log, which makes
            // a debug log useless. Report the first release and then at most
            // one message per minute.
            if (released > 0 && logCooldown <= 0f)
            {
                logCooldown = LogInterval;
                Log.Verbose("Released " + released + " stored output item(s) of " + optionKey);
            }
        }

        /// <summary>
        /// Decides whether a storage holds finished output that the vanilla
        /// empty chore would carry away.
        /// </summary>
        /// <param name="storage">Storage component of the building.</param>
        private bool ShouldRelease(Storage storage)
        {
            if (storage == null || storage.IsEmpty())
            {
                return false;
            }

            // Never touch the ingredient buffer of a fabricator: the recipe
            // still needs it and dropping it would break production.
            if (fabricator != null)
            {
                return storage == fabricator.outStorage;
            }

            // A storage a delivery component keeps filled is an input buffer,
            // never finished output.
            if (inputStorages.Contains(storage))
            {
                return false;
            }

            // Everything that is left is finished output. Several vanilla
            // buildings (Desalinator, Gleaner) keep allowItemRemoval disabled
            // on that storage as well, so it cannot be used as the criterion.
            return true;
        }

        /// <summary>
        /// Release controllers only drop output items; they do not alter
        /// the operational active state or animation of self-driven machines
        /// (e.g. Desalinator, FoodDehydrator, Gleaner).
        /// </summary>
        public override void StopAutomation()
        {
            // Do not call SetActive(false) or StopAnimation() here.
        }
    }
}
