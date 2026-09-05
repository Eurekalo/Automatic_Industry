// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.
// Original mod concept: "AutoMachine" (Steam Workshop id 2992024030).

using System;
using AutoMachineRebuilt.Automation;
using AutoMachineRebuilt.Config;
using AutoMachineRebuilt.Util;
using UnityEngine;

namespace AutoMachineRebuilt.Components
{
    /// <summary>
    /// Automates tinker stations, currently the Farm Station.
    ///
    /// A tinker station is a plain <see cref="Workable"/> whose completion
    /// consumes one portion of the input material and spawns one tool item
    /// (Micronutrient Fertilizer for the Farm Station). The vanilla completion
    /// runs through <c>Workable.OnCompleteWork(worker)</c>, so it cannot be
    /// called with a <c>null</c> worker; the controller therefore reproduces
    /// the production step itself with the same materials, temperature and
    /// output prefab, and never touches the Duplicant related code paths.
    ///
    /// Production conditions are the vanilla ones:
    /// <list type="bullet">
    /// <item>the building is operational (power and room),</item>
    /// <item>the storage holds at least <c>massPerTinker</c> input material,</item>
    /// <item>a plant actually requests the tool, unless the player enabled the
    /// "produce without demand" option.</item>
    /// </list>
    /// </summary>
    public sealed class AutoTinkerStationController : AutoWorkControllerBase
    {
        private TinkerStation station;
        private Storage storage;
        private KSelectable selectable;

        /// <summary>Seconds of production already spent.</summary>
        private float elapsed;

        /// <summary>Whether the producing status item is currently shown.</summary>
        private bool statusShown;

        /// <summary>Caches the vanilla components of the station.</summary>
        protected override void Prepare()
        {
            base.Prepare();
            station = GetComponent<TinkerStation>();
            storage = GetComponent<Storage>();
            selectable = GetComponent<KSelectable>();
            ApplySkillOverride();
        }

        /// <summary>
        /// Removes the vanilla skill perk requirement when the player waived
        /// it for this building. Only the perk of this single instance is
        /// cleared; the skill system itself is untouched.
        /// </summary>
        private void ApplySkillOverride()
        {
            if (station == null || string.IsNullOrEmpty(station.requiredSkillPerk) ||
                !AutoMachineOptions.IsSkillRequirementIgnored(optionKey))
            {
                return;
            }

            station.requiredSkillPerk = null;
            Log.Verbose("Skill requirement waived for " + optionKey);
        }

        /// <summary>Runs one production step.</summary>
        /// <param name="dt">Seconds since the previous step.</param>
        protected override void Step(float dt)
        {
            if (station == null || storage == null)
            {
                return;
            }

            // A Duplicant is using the station: never run in parallel.
            if (station.worker != null)
            {
                Abort();
                return;
            }

            // Ensure any lingering duplicant operate chores are suppressed while automated
            ChoreSuppression.CancelOperateChores(gameObject);

            if (!IsOperational() || !HasMaterial() || !IsProductWanted())
            {
                Abort();
                return;
            }

            SetActive(true);
            StartAnimation();
            ShowStatus(true);
            UpdateProgressBar(true);

            elapsed += dt;
            if (elapsed < Mathf.Max(1f, station.toolProductionTime))
            {
                return;
            }

            elapsed = 0f;
            Produce();

            // If we no longer have material or demand, abort cleanly; otherwise stay active
            if (!HasMaterial() || !IsProductWanted())
            {
                Abort();
            }
        }

        private ProgressBar progressBar;

        private void UpdateProgressBar(bool isWorking)
        {
            bool isPowerStation = optionKey != null && optionKey.IndexOf("POWER", StringComparison.OrdinalIgnoreCase) >= 0;
            bool optionEnabled = isPowerStation
                ? AutoMachineOptions.Instance.ProgressBarPowerControlStation
                : AutoMachineOptions.Instance.ProgressBarFabricators;

            bool show = optionEnabled && isWorking;
            if (show)
            {
                if (progressBar == null)
                {
                    progressBar = ProgressBar.CreateProgressBar(gameObject, () =>
                    {
                        if (station == null || station.toolProductionTime <= 0f)
                        {
                            return 0f;
                        }

                        return Mathf.Clamp01(elapsed / Mathf.Max(1f, station.toolProductionTime));
                    });
                }
                progressBar.SetVisibility(true);
            }
            else if (progressBar != null)
            {
                progressBar.gameObject.DeleteObject();
                progressBar = null;
            }
        }

        /// <summary>Whether the storage holds enough input material.</summary>
        private bool HasMaterial()
        {
            return storage.FindFirstWithMass(station.inputMaterial, station.massPerTinker) != null;
        }

        /// <summary>
        /// Vanilla demand check: a plant asked for the tool and none is stored
        /// on the world yet. The player can waive it per option, which turns
        /// the station into a continuous producer.
        /// </summary>
        private bool IsProductWanted()
        {
            bool isFarm = optionKey != null && optionKey.IndexOf("FARM", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isPower = optionKey != null && optionKey.IndexOf("POWER", StringComparison.OrdinalIgnoreCase) >= 0;

            if (station.alwaysTinker ||
                (isFarm && AutoMachineOptions.Instance.IgnoreCropDemandFarmStation) ||
                (isPower && AutoMachineOptions.Instance.IgnorePowerDemandPowerControlStation))
            {
                return true;
            }

            if (MaterialNeeds.GetAmount(station.outputPrefab, gameObject.GetMyWorldId(), false) <= 0f)
            {
                return false;
            }

            WorldContainer world = this.GetMyWorld();
            return world != null &&
                   world.worldInventory.GetAmount(station.outputPrefab, true) <= 0f;
        }

        /// <summary>
        /// Consumes the input material and spawns the tool item, mirroring
        /// <c>TinkerStation.OnCompleteWork</c>.
        /// </summary>
        private void Produce()
        {
            PrimaryElement input = storage.FindFirstWithMass(station.inputMaterial, station.massPerTinker);
            if (input == null)
            {
                return;
            }

            SimHashes element = input.ElementID;
            float temperature;
            float consumedMass;
            Klei.SimUtil.DiseaseInfo disease;
            storage.ConsumeAndGetDisease(element.CreateTag(), station.massPerTinker,
                out consumedMass, out disease, out temperature);
            if (consumedMass <= 0f)
            {
                return;
            }

            GameObject product = GameUtil.KInstantiate(Assets.GetPrefab(station.outputPrefab),
                transform.GetPosition() + Vector3.up, Grid.SceneLayer.Ore);
            PrimaryElement productElement = product.GetComponent<PrimaryElement>();
            if (productElement != null)
            {
                PrimaryElement buildingPe = GetComponent<PrimaryElement>();
                float fallbackTemp = (buildingPe != null && buildingPe.Temperature > 0.1f && !float.IsNaN(buildingPe.Temperature)) 
                    ? buildingPe.Temperature 
                    : 293.15f;
                float finalTemp = (temperature > 0.1f && !float.IsNaN(temperature)) 
                    ? temperature 
                    : fallbackTemp;

                productElement.SetElement(element);
                productElement.Temperature = finalTemp;
            }

            product.SetActive(true);
            Log.Verbose("Automated tinker station produced " + station.outputPrefab + " on " + name);
        }

        /// <summary>Stops the production and clears the vanilla status item.</summary>
        private void Abort()
        {
            elapsed = 0f;
            SetActive(false);
            StopAnimation();
            ShowStatus(false);
            UpdateProgressBar(false);
        }

        /// <summary>Shows or hides the vanilla "producing" status item.</summary>
        /// <param name="show">Whether the status item must be visible.</param>
        private void ShowStatus(bool show)
        {
            if (selectable == null || statusShown == show)
            {
                return;
            }

            statusShown = show;
            StatusItem item = Db.Get().BuildingStatusItems.ComplexFabricatorProducing;
            if (show)
            {
                selectable.AddStatusItem(item, this);
            }
            else
            {
                selectable.RemoveStatusItem(item, false);
            }
        }

        /// <summary>Clears the production when the automation stops.</summary>
        public override void StopAutomation()
        {
            base.StopAutomation();
            SafeInvoke.Try("clearing tinker status", delegate { ShowStatus(false); });
            UpdateProgressBar(false);
        }

        protected override void OnCleanUp()
        {
            if (progressBar != null)
            {
                progressBar.gameObject.DeleteObject();
                progressBar = null;
            }
            base.OnCleanUp();
        }
    }
}
