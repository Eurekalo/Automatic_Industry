// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Reflection;
using AutoMachineRebuilt.Config;
using AutoMachineRebuilt.Util;
using HarmonyLib;
using UnityEngine;

namespace AutoMachineRebuilt.Components
{
    /// <summary>
    /// Automates crop and plant harvesting for Auto-Sweepers (Solid Transfer Arms).
    /// Features:
    /// - High-performance spatial partitioning retrieval via GameScenePartitioner (zero global list lag).
    /// - Strict physical line-of-sight & wall raycast reachability check via SolidTransferArm.IsCellReachable.
    /// - Full compatibility with any custom building mod that increases Auto-Sweeper pickup range.
    /// - Respects player "Do Not Harvest" / "Harvest When Ready" designation on Bonbon trees, Arbor trees, and wild crops.
    /// - Proactively cancels active Duplicant harvest chores to prevent ghost harvests and empty air animations.
    /// </summary>
    public sealed class AutoSweeperHarvestController : KMonoBehaviour, ISim1000ms
    {
        private static readonly MethodInfo RotateArmMethod =
            AccessTools.Method(typeof(SolidTransferArm), "RotateArm", new Type[] { typeof(Vector3), typeof(bool), typeof(float) });

        private static readonly Action<SolidTransferArm, Vector3, bool, float> FastRotateArm =
            RotateArmMethod != null ? AccessTools.MethodDelegate<Action<SolidTransferArm, Vector3, bool, float>>(RotateArmMethod) : null;

        private static readonly Func<object, AutoSweeperHarvestController, global::Util.IterationInstruction> PlantVisitor =
            delegate (object obj, AutoSweeperHarvestController controller)
            {
                if (obj == null) return global::Util.IterationInstruction.Continue;

                Harvestable harvestable = null;
                if (obj is GameObject go)
                {
                    harvestable = go.GetComponent<Harvestable>();
                }
                else if (obj is Component comp)
                {
                    harvestable = comp.GetComponent<Harvestable>();
                }
                else if (obj is KMonoBehaviour kmb)
                {
                    harvestable = kmb.GetComponent<Harvestable>();
                }

                if (harvestable != null && harvestable.CanBeHarvested && !controller.candidates.Contains(harvestable))
                {
                    controller.candidates.Add(harvestable);
                }
                return global::Util.IterationInstruction.Continue;
            };

        private SolidTransferArm arm;
        private Operational operational;
        private float scanTimer;
        private readonly List<Harvestable> candidates = new List<Harvestable>();

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            arm = GetComponent<SolidTransferArm>();
            operational = GetComponent<Operational>();
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            if (arm == null)
            {
                arm = GetComponent<SolidTransferArm>();
            }
            if (operational == null)
            {
                operational = GetComponent<Operational>();
            }
            scanTimer = UnityEngine.Random.Range(0f, 0.5f);
        }

        public void Sim1000ms(float dt)
        {
            if (arm == null || operational == null || !operational.IsOperational)
            {
                return;
            }

            var options = AutoMachineOptions.Instance;
            if (options == null)
            {
                return;
            }

            bool harvestEnabled = options.EnableAllAutomation || options.AutoSweeperHarvest;
            if (!harvestEnabled)
            {
                return;
            }

            scanTimer += dt;
            float interval = Mathf.Max(0.5f, options.AutoSweeperScanInterval);
            if (scanTimer < interval)
            {
                return;
            }
            scanTimer = 0f;

            PerformHarvestScan(options);
        }

        private void PerformHarvestScan(AutoMachineOptions options)
        {
            int armCell = Grid.PosToCell(transform.position);
            if (!Grid.IsValidCell(armCell))
            {
                return;
            }

            int range = arm.pickupRange;
            if (range <= 0)
            {
                return;
            }

            Grid.CellToXY(armCell, out int armX, out int armY);
            candidates.Clear();

            // Calculate bounding box, dynamically expanding to cover custom Zoned Solid Transfer Arm zones if configured
            int minX = Mathf.Max(0, armX - range);
            int minY = Mathf.Max(0, armY - range);
            int maxX = Mathf.Min(Grid.WidthInCells - 1, armX + range);
            int maxY = Mathf.Min(Grid.HeightInCells - 1, armY + range);

            if (TryGetCustomZoneBounds(out int zMinX, out int zMinY, out int zMaxX, out int zMaxY))
            {
                minX = Mathf.Max(0, Mathf.Min(minX, zMinX));
                minY = Mathf.Max(0, Mathf.Min(minY, zMinY));
                maxX = Mathf.Min(Grid.WidthInCells - 1, Mathf.Max(maxX, zMaxX));
                maxY = Mathf.Min(Grid.HeightInCells - 1, Mathf.Max(maxY, zMaxY));
            }

            int width = maxX - minX + 1;
            int height = maxY - minY + 1;

            GameScenePartitioner.Instance.ReadonlyVisitEntries(minX, minY, width, height, GameScenePartitioner.Instance.plants, PlantVisitor, this);

            if (candidates.Count == 0)
            {
                return;
            }

            bool respectDesignation = options.AutoSweeperRespectHarvestDesignation;
            bool cancelDupeChore = options.AutoSweeperCancelDupeHarvestChore;
            bool rotated = false;

            foreach (Harvestable plant in candidates)
            {
                if (plant == null || !plant.CanBeHarvested)
                {
                    continue;
                }

                int plantCell = Grid.PosToCell(plant.gameObject);
                if (!Grid.IsValidCell(plantCell)) plantCell = Grid.PosToCell(plant.transform.position);
                if (!Grid.IsValidCell(plantCell))
                {
                    continue;
                }

                // Verify reachability using arm reachability (check plant cell and underlying farm tile cell)
                bool isReachable = arm.IsCellReachable(plantCell);
                if (!isReachable)
                {
                    int belowCell = Grid.CellBelow(plantCell);
                    if (Grid.IsValidCell(belowCell) && arm.IsCellReachable(belowCell))
                    {
                        isReachable = true;
                    }
                }
                if (!isReachable)
                {
                    continue;
                }

                // Respect Zoned Solid Transfer Arm item filter if enabled
                if (!IsCropAllowedByZonedFilter(plant))
                {
                    continue;
                }

                // Check player harvest designation
                if (respectDesignation)
                {
                    HarvestDesignatable designatable = plant.harvestDesignatable ?? plant.GetComponent<HarvestDesignatable>();
                    if (designatable != null)
                    {
                        if (!designatable.HarvestWhenReady && !designatable.MarkedForHarvest)
                        {
                            // Player explicitly disabled harvest on this plant (e.g. Bonbon tree for nectar or decor)
                            continue;
                        }
                    }
                }

                // Suppress Duplicant chore to prevent ghost harvests
                if (cancelDupeChore && plant.HasChore())
                {
                    try
                    {
                        plant.Trigger(2127324410, (object)true); // GameHashes.Cancel
                    }
                    catch
                    {
                    }
                }

                // Visual target rotation towards the first harvest target
                if (!rotated)
                {
                    try
                    {
                        Vector3 targetPos = Grid.CellToPosCCC(plantCell, Grid.SceneLayer.Front);
                        Vector3 dir = targetPos - arm.transform.position;
                        dir.z = 0f;
                        if (dir.sqrMagnitude > 0.001f && FastRotateArm != null)
                        {
                            FastRotateArm(arm, dir, false, 0f);
                        }
                    }
                    catch
                    {
                    }
                    rotated = true;
                }

                // Execute plant harvest
                try
                {
                    plant.Harvest();

                    if (options.EnableDiagnosticLogging && options.LogBuildingInteractions)
                    {
                        Log.Info($"[AutoSweeper] Harvested {plant.gameObject.name} at cell {plantCell}");
                    }
                }
                catch (Exception ex)
                {
                    if (options.EnableDiagnosticLogging)
                    {
                        Log.Warn($"[AutoSweeper] Error harvesting plant at cell {plantCell}: {ex.Message}");
                    }
                }
            }

            candidates.Clear();
        }

        /// <summary>
        /// Reads custom zone cell boundaries from ZonedSolidTransferArm if installed and configured.
        /// </summary>
        private bool TryGetCustomZoneBounds(out int minX, out int minY, out int maxX, out int maxY)
        {
            minX = minY = int.MaxValue;
            maxX = maxY = int.MinValue;
            bool found = false;

            try
            {
                var components = GetComponents<KMonoBehaviour>();
                if (components != null)
                {
                    for (int i = 0; i < components.Length; i++)
                    {
                        var c = components[i];
                        if (c == null) continue;
                        var type = c.GetType();
                        if (type.Name == "ZonedSolidTransferArmControl")
                        {
                            var prop = type.GetProperty("Cells", BindingFlags.Public | BindingFlags.Instance);
                            if (prop != null && prop.GetValue(c, null) is IEnumerable<int> cells)
                            {
                                foreach (int cell in cells)
                                {
                                    if (Grid.IsValidCell(cell))
                                    {
                                        Grid.CellToXY(cell, out int x, out int y);
                                        if (x < minX) minX = x;
                                        if (y < minY) minY = y;
                                        if (x > maxX) maxX = x;
                                        if (y > maxY) maxY = y;
                                        found = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // Fallback gracefully
            }

            return found;
        }

        /// <summary>
        /// Checks if a crop's product tag is allowed by ZonedSolidTransferArm's item filter if active.
        /// </summary>
        private bool IsCropAllowedByZonedFilter(Harvestable plant)
        {
            if (plant == null) return true;
            try
            {
                var filterCmp = GetComponent("ZonedSolidTransferArmPickFilter");
                if (filterCmp != null)
                {
                    var method = filterCmp.GetType().GetMethod("IsFilterEnabled", BindingFlags.Public | BindingFlags.Instance);
                    if (method != null && (bool)method.Invoke(filterCmp, null))
                    {
                        var crop = plant.GetComponent<Crop>();
                        if (crop != null && !string.IsNullOrEmpty(crop.cropId))
                        {
                            var treeFilterable = GetComponent<TreeFilterable>();
                            if (treeFilterable != null && !treeFilterable.AcceptedTags.Contains(new Tag(crop.cropId)))
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            return true;
        }
    }
}
