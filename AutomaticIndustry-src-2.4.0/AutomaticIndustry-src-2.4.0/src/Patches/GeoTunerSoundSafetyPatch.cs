// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System;
using HarmonyLib;
using UnityEngine;

namespace AutoMachineRebuilt.Patches
{
    /// <summary>
    /// Prevents the game-crashing NullReferenceException in GeoTuner.TriggerSoundsForGeyserChange.
    /// In vanilla ONI, GeoTuner has static fields:
    ///   public static string liquidGeyserTuningSoundPath = GlobalAssets.GetSound("GeoTuner_Tuning_Geyser");
    ///   public static string gasGeyserTuningSoundPath = GlobalAssets.GetSound("GeoTuner_Tuning_Vent");
    ///   public static string metalGeyserTuningSoundPath = GlobalAssets.GetSound("GeoTuner_Tuning_Volcano");
    /// When any mod references GeoTuner during OnLoad, its static constructor executes before
    /// GlobalAssets has loaded audio banks, permanently assigning null to these sound paths.
    /// Later in-game, SoundEvent.BeginOneShot(null) calls FMODUnity.RuntimeManager.PathToGUID(null),
    /// crashing the GeoTuner state machine into a black hole error.
    /// This patch auto-heals the sound paths once GlobalAssets is ready and guards against null event paths.
    /// </summary>
    [HarmonyPatch(typeof(GeoTuner), "TriggerSoundsForGeyserChange")]
    public static class GeoTunerSoundSafetyPatch
    {
        public static void EnsureSoundPathsPopulated()
        {
            try
            {
                if (string.IsNullOrEmpty(GeoTuner.liquidGeyserTuningSoundPath))
                {
                    GeoTuner.liquidGeyserTuningSoundPath = GlobalAssets.GetSound("GeoTuner_Tuning_Geyser");
                }
                if (string.IsNullOrEmpty(GeoTuner.gasGeyserTuningSoundPath))
                {
                    GeoTuner.gasGeyserTuningSoundPath = GlobalAssets.GetSound("GeoTuner_Tuning_Vent");
                }
                if (string.IsNullOrEmpty(GeoTuner.metalGeyserTuningSoundPath))
                {
                    GeoTuner.metalGeyserTuningSoundPath = GlobalAssets.GetSound("GeoTuner_Tuning_Volcano");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[AutoMachineRebuilt] Exception populating GeoTuner sound paths: " + ex.Message);
            }
        }

        public static bool Prefix(GeoTuner.Instance smi)
        {
            try
            {
                EnsureSoundPathsPopulated();

                Geyser assignedGeyser = smi?.GetAssignedGeyser();
                if (assignedGeyser == null || smi == null)
                {
                    return false;
                }

                if (assignedGeyser.configuration == null || assignedGeyser.configuration.geyserType == null)
                {
                    return false;
                }

                string soundPath = null;
                switch (assignedGeyser.configuration.geyserType.shape)
                {
                    case GeyserConfigurator.GeyserShape.Liquid:
                        soundPath = GeoTuner.liquidGeyserTuningSoundPath;
                        break;
                    case GeyserConfigurator.GeyserShape.Gas:
                        soundPath = GeoTuner.gasGeyserTuningSoundPath;
                        break;
                    case GeyserConfigurator.GeyserShape.Molten:
                        soundPath = GeoTuner.metalGeyserTuningSoundPath;
                        break;
                }

                if (!string.IsNullOrEmpty(soundPath))
                {
                    SoundEvent.PlayOneShot(soundPath, smi.transform.GetPosition(), 1f);
                }

                return false; // Skip original un-guarded method
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[AutoMachineRebuilt] Handled GeoTuner sound trigger safely: " + ex.Message);
                return false; // Suppress crash
            }
        }
    }
}