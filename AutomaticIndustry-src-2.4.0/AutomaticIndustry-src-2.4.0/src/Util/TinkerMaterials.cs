// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using UnityEngine;

namespace AutoMachineRebuilt.Util
{
    /// <summary>
    /// Helper to identify items and tags that belong to maintenance/tinkering systems
    /// (e.g. Mechanics Station custom parts, Power Control Station microchips, Farm Station fertilizer).
    /// These materials must NEVER be dropped, expelled, or released by automated emptying routines.
    /// </summary>
    internal static class TinkerMaterials
    {
        internal static readonly Tag MachinePartsTag = new Tag("MachineParts");
        internal static readonly Tag PowerStationToolsTag = new Tag("PowerStationTools");
        internal static readonly Tag FarmStationToolsTag = new Tag("FarmStationTools");

        /// <summary>
        /// Checks whether the given tag represents a tinker / tuning material.
        /// </summary>
        internal static bool IsTinkerMaterialTag(Tag tag, GameObject building = null)
        {
            if (!tag.IsValid) return false;
            if (tag == MachinePartsTag || tag == PowerStationToolsTag || tag == FarmStationToolsTag)
            {
                return true;
            }

            if (building != null)
            {
                Tinkerable[] tinkerables = building.GetComponents<Tinkerable>();
                if (tinkerables != null)
                {
                    for (int i = 0; i < tinkerables.Length; i++)
                    {
                        if (tinkerables[i] != null && tinkerables[i].tinkerMaterialTag.IsValid &&
                            tinkerables[i].tinkerMaterialTag == tag)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Checks whether the given item is a tinker / tuning material.
        /// </summary>
        internal static bool IsTinkerMaterial(GameObject item, GameObject building = null)
        {
            if (item == null) return false;
            KPrefabID kpid = item.GetComponent<KPrefabID>();
            if (kpid == null) return false;

            if (kpid.HasTag(MachinePartsTag) || kpid.HasTag(PowerStationToolsTag) || kpid.HasTag(FarmStationToolsTag))
            {
                return true;
            }

            if (building != null)
            {
                Tinkerable[] tinkerables = building.GetComponents<Tinkerable>();
                if (tinkerables != null)
                {
                    for (int i = 0; i < tinkerables.Length; i++)
                    {
                        if (tinkerables[i] != null && tinkerables[i].tinkerMaterialTag.IsValid &&
                            kpid.HasTag(tinkerables[i].tinkerMaterialTag))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Checks whether a storage contains any tinker materials.
        /// </summary>
        internal static bool ContainsTinkerMaterial(Storage storage, GameObject building = null)
        {
            if (storage == null || storage.items == null) return false;
            for (int i = 0; i < storage.items.Count; i++)
            {
                if (IsTinkerMaterial(storage.items[i], building))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
