// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using AutoMachineRebuilt.Util;
using HarmonyLib;
using UnityEngine;

namespace AutoMachineRebuilt.Patches
{
    /// <summary>
    /// Defensive null-safety patches for GeneticAnalysisStationWorkable to ensure complete
    /// compatibility with FastTrack, modded seeds, and automated analysis sessions.
    /// </summary>
    internal static class GeneticAnalysisStationPatches
    {
        [HarmonyPatch(typeof(GeneticAnalysisStationWorkable), "IdentifyMutant")]
        public static class GeneticAnalysisStationWorkable_IdentifyMutant_Patch
        {
            [HarmonyPriority(Priority.High)]
            public static bool Prefix(GeneticAnalysisStationWorkable __instance)
            {
                if (__instance == null || __instance.storage == null)
                {
                    return false;
                }

                try
                {
                    GameObject seedGO = __instance.storage.FindFirst(GameTags.UnidentifiedSeed);
                    if (seedGO == null)
                    {
                        return false;
                    }

                    Pickupable component = seedGO.GetComponent<Pickupable>();
                    if (component == null)
                    {
                        return false;
                    }

                    Pickupable pickupable = (component.PrimaryElement == null || !(component.PrimaryElement.Units > 1f))
                        ? __instance.storage.Drop(seedGO)?.GetComponent<Pickupable>()
                        : component.TakeUnit(1f);

                    if (pickupable == null)
                    {
                        return false;
                    }

                    pickupable.transform.SetPosition(__instance.transform.GetPosition() + __instance.finishedSeedDropOffset);

                    MutantPlant mutant = pickupable.GetComponent<MutantPlant>();
                    if (mutant != null)
                    {
                        if (PlantSubSpeciesCatalog.Instance != null && mutant.SubSpeciesID != null)
                        {
                            PlantSubSpeciesCatalog.Instance.IdentifySubSpecies(mutant.SubSpeciesID);
                        }

                        mutant.Analyze();

                        if (SaveGame.Instance != null && SaveGame.Instance.ColonyAchievementTracker != null && mutant.SpeciesID != null)
                        {
                            SaveGame.Instance.ColonyAchievementTracker.LogAnalyzedSeed(mutant.SpeciesID);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Log.Warn("Handled unexpected error in GeneticAnalysisStationWorkable.IdentifyMutant: " + ex.Message);
                }

                return false;
            }
        }
    }
}
