// Copyright (c) 2026 Automatic Industry contributors. Licensed under the MIT License.
// Original mod concept: "AutoMachine" (Steam Workshop id 2992024030).

using AutoMachineRebuilt.Config;
using UnityEngine;

namespace AutoMachineRebuilt.Components
{
    /// <summary>
    /// Displays a floating progress bar above the Gleaner (Milk Fat Separator)
    /// showing its stored solid output (Caviar / Brackwax) capacity percentage (0% - 100%).
    ///
    /// Driven via ISim200ms without touching the operational logic of the building.
    /// </summary>
    public class GleanerProgressBarComponent : KMonoBehaviour, ISim200ms
    {
        private MilkSeparator.Instance separator;
        private ProgressBar progressBar;

        protected override void OnSpawn()
        {
            base.OnSpawn();
            separator = gameObject.GetSMI<MilkSeparator.Instance>();
        }

        public void Sim200ms(float dt)
        {
            bool show = AutoMachineOptions.Instance != null &&
                        AutoMachineOptions.Instance.ProgressBarGleaner &&
                        separator != null &&
                        separator.IsRunning();

            if (show)
            {
                if (progressBar == null)
                {
                    progressBar = ProgressBar.CreateProgressBar(gameObject, () =>
                    {
                        return separator != null ? Mathf.Clamp01(separator.SolidOutputStoragePercentage) : 0f;
                    });
                }
                if (progressBar != null)
                {
                    progressBar.SetVisibility(true);
                }
            }
            else if (progressBar != null)
            {
                progressBar.gameObject.DeleteObject();
                progressBar = null;
            }
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
