// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System.Collections.Generic;
using AutoMachineRebuilt.Config;
using KSerialization;
using UnityEngine;

namespace AutoMachineRebuilt.Components
{
    /// <summary>
    /// Colony-level persistence and management component attached to SaveGame.
    /// Acts as dual-layer backup for building automation states and handles
    /// colony-wide and category-wide one-click reset operations.
    /// </summary>
    [SerializationConfig(MemberSerialization.OptIn)]
    [AddComponentMenu("KMonoBehaviour/scripts/ColonyAutomationMasterRegistry")]
    public class ColonyAutomationMasterRegistry : KMonoBehaviour
    {
        public static ColonyAutomationMasterRegistry Instance { get; private set; }

        [Serialize]
        private Dictionary<int, bool> cellOverrides = new Dictionary<int, bool>();

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            Instance = this;
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            Instance = this;
        }

        public void RecordOverride(int cell, bool isAutomated)
        {
            if (cell >= 0)
            {
                cellOverrides[cell] = isAutomated;
            }
        }

        public void RemoveOverride(int cell)
        {
            if (cell >= 0)
            {
                cellOverrides.Remove(cell);
            }
        }

        public bool TryGetOverride(int cell, out bool isAutomated)
        {
            return cellOverrides.TryGetValue(cell, out isAutomated);
        }

        /// <summary>
        /// Resets all custom overrides across the entire colony, re-syncing every building to global settings.
        /// </summary>
        public static void ResetAllColonyOverrides()
        {
            if (Instance != null)
            {
                Instance.cellOverrides.Clear();
            }

            foreach (var building in global::Components.BuildingCompletes.Items)
            {
                if (building == null) continue;
                var customizer = building.GetComponent<AutoBuildingCustomizer>();
                if (customizer != null)
                {
                    customizer.ResetToGlobalDefault();
                }
            }
        }

        /// <summary>
        /// Resets all overrides for a specific building prefab ID across the colony.
        /// </summary>
        public static void ResetPrefabOverrides(string prefabId)
        {
            if (string.IsNullOrEmpty(prefabId)) return;

            foreach (var building in global::Components.BuildingCompletes.Items)
            {
                if (building == null) continue;
                var kpid = building.GetComponent<KPrefabID>();
                if (kpid != null && kpid.PrefabTag.Name == prefabId)
                {
                    var customizer = building.GetComponent<AutoBuildingCustomizer>();
                    if (customizer != null)
                    {
                        customizer.ResetToGlobalDefault();
                    }
                }
            }
        }

        protected override void OnCleanUp()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            base.OnCleanUp();
        }
    }
}
