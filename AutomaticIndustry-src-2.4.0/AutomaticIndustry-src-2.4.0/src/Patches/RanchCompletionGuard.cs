// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.
// Original mod concept: "AutoMachine" (Steam Workshop id 2992024030).

using System;
using System.Collections.Generic;
using AutoMachineRebuilt.Util;
using Klei.AI;
using UnityEngine;

namespace AutoMachineRebuilt.Patches
{
    /// <summary>
    /// Makes the vanilla ranching completion callbacks safe when no Duplicant
    /// performed the work.
    ///
    /// Every station stores its result logic in
    /// <c>RanchStation.Def.OnRanchCompleteCb(creature, rancher)</c>. The
    /// vanilla delegates dereference <c>rancher</c> to read the Ranching
    /// attribute (grooming, shearing), so calling them with a <c>null</c>
    /// rancher throws inside the state machine and can corrupt the critter's
    /// chore. Instead of copying the whole vanilla logic, the callback of the
    /// prefab is wrapped once:
    ///
    /// <list type="bullet">
    /// <item>a Duplicant did the work: the untouched vanilla callback runs,
    /// including the rancher's attribute bonus,</item>
    /// <item>the automation did the work: an equivalent callback runs that
    /// applies the same effects with the base (no bonus) values.</item>
    /// </list>
    ///
    /// This keeps mod behaviour identical to vanilla for real ranchers and
    /// guarantees that an automated station can never throw inside
    /// <c>RanchStation.RanchCreature</c>.
    /// </summary>
    internal static class RanchCompletionGuard
    {
        /// <summary>Vanilla effect applied by a grooming station.</summary>
        private const string RanchedEffectId = "Ranched";

        /// <summary>Prefabs whose callback is already wrapped.</summary>
        private static readonly HashSet<string> Wrapped = new HashSet<string>(StringComparer.Ordinal);

        /// <summary>
        /// Installs the null safe wrapper on a ranch station prefab.
        /// </summary>
        /// <param name="prefabId">Prefab identifier of the station.</param>
        /// <param name="prefab">Completed building prefab.</param>
        internal static void Install(string prefabId, GameObject prefab)
        {
            if (prefab == null || string.IsNullOrEmpty(prefabId) || Wrapped.Contains(prefabId))
            {
                return;
            }

            StateMachineController controller = prefab.GetComponent<StateMachineController>();
            RanchStation.Def def = controller == null ? null : controller.GetDef<RanchStation.Def>();
            if (def == null || def.OnRanchCompleteCb == null)
            {
                return;
            }

            Wrapped.Add(prefabId);

            Action<GameObject, WorkerBase> vanilla = def.OnRanchCompleteCb;
            string station = prefabId;

            def.OnRanchCompleteCb = delegate(GameObject creature, WorkerBase rancher)
            {
                if (rancher != null)
                {
                    vanilla(creature, rancher);
                    return;
                }

                SafeInvoke.Try("automated ranch completion (" + station + ")", delegate
                {
                    CompleteWithoutRancher(station, creature);
                });
            };

            Log.Verbose("Installed null safe ranching completion for " + prefabId);
        }

        /// <summary>
        /// Applies the station specific vanilla result without a rancher.
        /// </summary>
        /// <param name="prefabId">Prefab identifier of the station.</param>
        /// <param name="creature">Critter that was tended.</param>
        private static void CompleteWithoutRancher(string prefabId, GameObject creature)
        {
            if (creature == null)
            {
                return;
            }

            switch (prefabId)
            {
                case "RanchStation":
                case "UnderwaterRanchStation":
                    Groom(creature);
                    break;

                case "ShearingStation":
                case "UnderwaterShearingStation":
                    Shear(creature, prefabId == "UnderwaterShearingStation");
                    break;

                case "MilkingStation":
                case "UnderwaterMilkingStation":
                    Milk(creature);
                    break;

                default:
                    Log.Warn("No automated ranching completion for " + prefabId);
                    break;
            }
        }

        /// <summary>
        /// Grooming: applies the vanilla "Ranched" effect with its base
        /// duration (a rancher would extend it by 10% per attribute point) and
        /// heals the critter, exactly like the vanilla callback does.
        /// </summary>
        /// <param name="creature">Critter that was groomed.</param>
        private static void Groom(GameObject creature)
        {
            Effects effects = creature.GetComponent<Effects>();
            if (effects != null)
            {
                effects.Add(RanchedEffectId, true);
            }

            AmountInstance hitPoints = Db.Get().Amounts.HitPoints.Lookup(creature);
            if (hitPoints != null)
            {
                hitPoints.ApplyDelta(hitPoints.GetMax() - hitPoints.value + 1f);
            }
        }

        /// <summary>
        /// Shearing: drops the vanilla shear product and resets the critter's
        /// shearing timer.
        /// </summary>
        /// <param name="creature">Critter that was sheared.</param>
        /// <param name="aquatic">Whether the aquatic station is used.</param>
        private static void Shear(GameObject creature, bool aquatic)
        {
            RanchableMonitor.Instance monitor = creature.GetSMI<RanchableMonitor.Instance>();
            IShearable shearable = creature.GetSMI<IShearable>();
            if (shearable == null)
            {
                return;
            }

            Tuple<Tag, float> dropped = shearable.GetItemDroppedOnShear();
            GameObject station = monitor == null || monitor.TargetRanchStation == null
                ? null
                : monitor.TargetRanchStation.gameObject;

            // Vanilla drops next to the Duplicant (land) or next to the critter
            // (aquatic); without a Duplicant the station is the drop origin.
            int cell = aquatic || station == null
                ? Grid.CellRight(Grid.PosToCell(creature))
                : Grid.CellLeft(Grid.PosToCell(station));

            DropProduct(creature, cell, dropped.first, dropped.second);

            // Vanilla UnderwaterShearingStationConfig calls Shear() directly
            // in OnRanchCompleteCb after spawning the product. The SeaTurtle's
            // WellFedShearable state machine does not call it independently;
            // omitting this reset leaves the scales fully grown and the critter
            // immediately eligible again, causing a station queue loop.
            shearable.Shear();

            if (station != null)
            {
                UnderwaterShearingStaion aquaticStation = station.GetComponent<UnderwaterShearingStaion>();
                if (aquaticStation != null)
                {
                    aquaticStation.HideShearableSymbol();
                }
            }
        }

        /// <summary>
        /// Milking: hands the produced liquid to the station storage, which the
        /// vanilla conduit dispenser empties.
        /// </summary>
        /// <param name="creature">Critter that was milked.</param>
        private static void Milk(GameObject creature)
        {
            RanchableMonitor.Instance monitor = creature.GetSMI<RanchableMonitor.Instance>();
            IMilkable milkable = creature.GetSMI<IMilkable>();
            if (monitor == null || milkable == null || monitor.TargetRanchStation == null)
            {
                return;
            }

            Storage storage = monitor.TargetRanchStation.GetComponent<Storage>();
            if (storage != null)
            {
                milkable.MilkingComplete(storage);
            }
        }

        /// <summary>
        /// Spawns the shear product with the critter's temperature and germs,
        /// matching the vanilla drop routine of the shearing stations.
        /// </summary>
        /// <param name="creature">Critter that was sheared.</param>
        /// <param name="cell">Target cell of the product.</param>
        /// <param name="itemTag">Product prefab tag.</param>
        /// <param name="mass">Product mass in kilograms.</param>
        private static void DropProduct(GameObject creature, int cell, Tag itemTag, float mass)
        {
            GameObject prefab = Assets.GetPrefab(itemTag);
            if (prefab == null)
            {
                return;
            }

            if (!Grid.IsValidCell(cell))
            {
                cell = Grid.PosToCell(creature);
            }

            PrimaryElement source = creature.GetComponent<PrimaryElement>();
            GameObject product = global::Util.KInstantiate(prefab);
            product.transform.SetPosition(Grid.CellToPosCCC(cell, Grid.SceneLayer.Ore));

            PrimaryElement element = product.GetComponent<PrimaryElement>();
            if (element != null)
            {
                float temp = (source != null && source.Temperature > 0.1f && !float.IsNaN(source.Temperature))
                    ? source.Temperature
                    : 293.15f;
                element.Temperature = temp;
                element.Mass = mass;
                if (source != null)
                {
                    element.AddDisease(source.DiseaseIdx, source.DiseaseCount, "Shearing");
                }
            }

            product.SetActive(true);

            Vector2 velocity = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.value * 2f + 2f);
            if (GameComps.Fallers.Has(product))
            {
                GameComps.Fallers.Remove(product);
            }

            GameComps.Fallers.Add(product, velocity);
        }
    }
}
