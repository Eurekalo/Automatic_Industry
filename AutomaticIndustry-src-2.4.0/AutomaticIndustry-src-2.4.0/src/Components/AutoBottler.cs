// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using AutoMachineRebuilt.Config;
using AutoMachineRebuilt.Util;
using UnityEngine;

namespace AutoMachineRebuilt.Components
{
    /// <summary>
    /// Automates the Bottle Filler (LiquidBottler) and Canister Filler (GasBottler):
    /// enables direct pickup by Auto-Sweepers (Solid Transfer Arm) without needing
    /// a Duplicant or dropping liquids/gases onto the floor, and manages the optional
    /// filling progress bar.
    /// </summary>
    public sealed class AutoBottler : KMonoBehaviour, ISim1000ms
    {
        private Bottler bottler;
        private Storage storage;
        private ProgressBar progressBar;
        private string optionKey;

        public void Configure(string key)
        {
            optionKey = key;
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            bottler = GetComponent<Bottler>();
            storage = bottler?.storage ?? GetComponent<Storage>();
            if (string.IsNullOrEmpty(optionKey))
            {
                optionKey = GetComponent<KPrefabID>()?.PrefabTag.ToString()?.ToUpperInvariant() ?? "LIQUIDBOTTLER";
            }
        }

        public void Sim1000ms(float dt)
        {
            if (storage == null || bottler == null)
            {
                return;
            }

            bool enabled = AutoMachineOptions.IsEnabledFor(gameObject, optionKey) ||
                           AutoMachineOptions.IsEnabledFor(gameObject, "LIQUIDBOTTLER") ||
                           AutoMachineOptions.IsEnabledFor(gameObject, "GASBOTTLER") ||
                           (AutoMachineOptions.Instance != null && AutoMachineOptions.Instance.AutoSweeperBottlerDirectPickup);

            if (enabled && storage.items != null && storage.items.Count > 0)
            {
                storage.allowItemRemoval = true;
                for (int i = 0; i < storage.items.Count; i++)
                {
                    GameObject item = storage.items[i];
                    if (item == null)
                    {
                        continue;
                    }

                    Pickupable pickupable = item.GetComponent<Pickupable>();
                    if (pickupable != null)
                    {
                        pickupable.targetWorkable = pickupable;
                    }
                }
            }

            UpdateProgressBar();
        }

        private void UpdateProgressBar()
        {
            bool show = AutoMachineOptions.Instance.ProgressBarBottler &&
                        storage != null &&
                        storage.MassStored() > 0f &&
                        storage.capacityKg > 0f;

            if (show)
            {
                if (progressBar == null)
                {
                    progressBar = ProgressBar.CreateProgressBar(gameObject, GetPercentComplete);
                }
                progressBar.SetVisibility(true);
            }
            else if (progressBar != null)
            {
                progressBar.gameObject.DeleteObject();
                progressBar = null;
            }
        }

        private float GetPercentComplete()
        {
            if (storage == null || storage.capacityKg <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp01(storage.MassStored() / storage.capacityKg);
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
