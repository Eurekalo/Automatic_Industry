// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.
// Original mod concept: "AutoMachine" (Steam Workshop id 2992024030).

using System;
using System.Collections.Generic;
using System.Reflection;
using AutoMachineRebuilt.Automation;
using AutoMachineRebuilt.Components;
using AutoMachineRebuilt.Config;
using AutoMachineRebuilt.Util;
using HarmonyLib;
using UnityEngine;

namespace AutoMachineRebuilt.Patches
{
    /// <summary>
    /// Attaches every automation component to the finished building prefabs.
    ///
    /// Why a prefab scan instead of Harmony patches on the building config
    /// classes: patching a config class forces its static constructor to run
    /// at mod load time. Several vanilla configs call <c>Db.Get()</c> in their
    /// static constructor, and calling that before the game initialised its
    /// database throws a <see cref="TypeInitializationException"/>. The failed
    /// type initializer is cached by the runtime, which then breaks
    /// <c>BuildingConfigManager.RegisterBuilding</c> for every building and
    /// crashes the game during loading.
    ///
    /// Running after <c>GeneratedBuildings.LoadGeneratedBuildings</c> keeps
    /// the mod completely passive during load: the game builds its prefabs
    /// exactly as usual and the mod only adds components afterwards. Buildings
    /// from a DLC that is not installed simply do not appear in the scan.
    /// </summary>
    [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
    internal static class BuildingPrefabInjection
    {
        /// <summary>Vanilla petroleum output of the Oil Refinery in kg/s.</summary>
        private const float VanillaPetroleumRate = 5f;

        /// <summary>Vanilla natural gas output of the Oil Refinery in kg/s.</summary>
        private const float VanillaMethaneRate = 0.09f;

        /// <summary>Injects all automation components once buildings exist.</summary>
        internal static void Postfix()
        {
            SafeInvoke.Try("CustomizeBuildings compatibility check before injection", delegate
            {
                CustomizeBuildingsCompatibility.Apply(new Harmony("AutoMachineRebuilt.Compatibility"));
            });
            SafeInvoke.Try("Injecting automation components", Inject);
        }

        /// <summary>Walks the registered building prefabs exactly once.</summary>
        private static void Inject()
        {
            Dictionary<string, AutomationEntry> driverKeys = BuildDriverKeyIndex();

            foreach (BuildingDef def in Assets.BuildingDefs)
            {
                if (def == null || def.BuildingComplete == null)
                {
                    continue;
                }

                GameObject prefab = def.BuildingComplete;
                string prefabId = def.PrefabID;

                // All ComplexFabricator based machines (Electric Grill, Gas Range, Diamond Press,
                // Metal Refinery, Rock Crusher, Microbe Musher, Suit Fabricator, Apothecary, etc.)
                // Only attach to machines that have a workable component.
                // Machines like Kiln, FoodDehydrator, Chlorinator, etc. are natively unattended and have no workable.
                ComplexFabricator fabricatorCmp = prefab.GetComponent<ComplexFabricator>();
                if (fabricatorCmp != null && prefab.GetComponent<ComplexFabricatorWorkable>() != null)
                {
                    SafeInvoke.Try("Attaching fabricator controller to " + prefabId, delegate
                    {
                        AutoFabricatorController controller = Ensure<AutoFabricatorController>(prefab);
                        if (controller != null)
                        {
                            controller.Configure(prefabId);
                            Attach<AutoBuildingCustomizer>(prefab);
                        }
                    });
                }

                AutomationEntry entry;
                if (driverKeys.TryGetValue(prefabId, out entry))
                {
                    AutomationEntry current = entry;
                    SafeInvoke.Try("Attaching driver to " + prefabId, delegate
                    {
                        AttachController(prefab, current);
                    });
                }

                InjectSpecialised(prefabId, prefab);
            }
        }

        /// <summary>
        /// Adds the valve controller to every manually set valve.
        ///
        /// The check is done on the component instead of a prefab identifier
        /// list, so the Liquid Valve, the Gas Valve and any future variant
        /// that uses the same vanilla <see cref="Valve"/> component are all
        /// covered, while the automated limit and logic valves - which have no
        /// <see cref="Valve"/> component and never ask for a Duplicant - are
        /// left untouched.
        /// </summary>
        /// <param name="prefab">Completed building prefab.</param>
        private static void InjectValve(GameObject prefab)
        {
            if (prefab.GetComponent<Valve>() == null)
            {
                return;
            }

            AutoValveController controller = Ensure<AutoValveController>(prefab);
            if (controller != null)
            {
                controller.Configure("VALVE");
                Attach<AutoBuildingCustomizer>(prefab);
            }
        }

        /// <summary>Adds the hand written components of the legacy targets.</summary>
        /// <param name="prefabId">Prefab identifier of the building.</param>
        /// <param name="prefab">Completed building prefab.</param>
        /// <remarks>
        /// See <see cref="InjectResearchDelivery"/> for the Auto-Sweeper
        /// delivery option of the research buildings.
        /// </remarks>
        private static void InjectSpecialised(string prefabId, GameObject prefab)
        {
            InjectValve(prefab);
            InjectResearchDelivery(prefabId, prefab);

            if (prefab.GetComponent<SolidTransferArm>() != null)
            {
                Attach<AutoSweeperHarvestController>(prefab);
                if (prefab.GetComponent<SymbolOverrideController>() == null)
                {
                    SymbolOverrideControllerUtil.AddToPrefab(prefab);
                }
            }

            switch (prefabId)
            {
                case "SolidTransferArm":
                    Attach<AutoSweeperHarvestController>(prefab);
                    break;

                case "OilWellCap":
                    if (prefab.GetComponent<OilWellCap>() != null)
                    {
                        Attach<AutoOilWellCap>(prefab);
                        Attach<AutoBuildingCustomizer>(prefab);
                    }
                    break;

                case "Smoker":
                    Attach<AutoFoodSmoker>(prefab);
                    Attach<AutoBuildingCustomizer>(prefab);
                    break;

                case "OilRefinery":
                    if (prefab.GetComponent<OilRefinery>() != null)
                    {
                        Attach<AutoOilRefinery>(prefab);
                        ApplyOilRefineryRatio(prefab);
                        Attach<AutoBuildingCustomizer>(prefab);
                    }
                    break;

                case "FoodDehydrator":
                    // The Dehydrator both runs a recipe queue and waits for a
                    // Duplicant to empty the finished packets. The fabricator
                    // controller comes from the registry row; the release
                    // controller added here performs the vanilla emptying so
                    // the state machine can start the next batch.
                    AutoStorageReleaseController dehydratorRelease =
                        Ensure<AutoStorageReleaseController>(prefab);
                    if (dehydratorRelease != null)
                    {
                        dehydratorRelease.Configure("FOODDEHYDRATOR");
                    }
                    Attach<AutoBuildingCustomizer>(prefab);
                    break;

                case "GeoTuner":
                    AutoGeoTuner geoTuner = Ensure<AutoGeoTuner>(prefab);
                    if (geoTuner != null)
                    {
                        geoTuner.Configure("GEOTUNER");
                        Attach<AutoBuildingCustomizer>(prefab);
                    }
                    GeoTunerSoundSafetyPatch.EnsureSoundPathsPopulated();
                    break;

                case "LiquidBottler":
                case "GasBottler":
                case "LiquidPumpingStation":
                    Attach<AutoBottler>(prefab);
                    Attach<AutoBuildingCustomizer>(prefab);
                    break;

                case "MorbRoverMaker":
                    Attach<AutoMorbRoverMaker>(prefab);
                    Attach<AutoBuildingCustomizer>(prefab);
                    break;

                case "RanchStation":
                case "ShearingStation":
                case "MilkingStation":
                case "UnderwaterRanchStation":
                case "UnderwaterShearingStation":
                case "UnderwaterMilkingStation":
                    RanchCompletionGuard.Install(prefabId, prefab);
                    AutoRanchStation ranchController = Ensure<AutoRanchStation>(prefab);
                    if (ranchController != null)
                    {
                        ranchController.Configure(prefabId);
                        Attach<AutoBuildingCustomizer>(prefab);
                    }

                    break;

                case "MilkFatSeparator":
                    Attach<GleanerProgressBarComponent>(prefab);
                    Attach<AutoBuildingCustomizer>(prefab);
                    break;

                case "Compost":
                    Attach<CompostAutomationComponent>(prefab);
                    Attach<AutoBuildingCustomizer>(prefab);
                    break;
            }
        }

        /// <summary>
        /// Adds the controller that matches the vanilla mechanism of the
        /// building. One controller per mechanism keeps every building on the
        /// code path that was audited for it, instead of a single generic
        /// driver that would have to guess.
        /// </summary>
        /// <param name="prefab">Completed building prefab.</param>
        /// <param name="entry">Registry row of the building.</param>
        private static void AttachController(GameObject prefab, AutomationEntry entry)
        {
            AutoWorkControllerBase controller = null;

            switch (entry.Mechanism)
            {
                case AutomationMechanism.Fabricator:
                    controller = Ensure<AutoFabricatorController>(prefab);
                    break;

                case AutomationMechanism.Release:
                    controller = Ensure<AutoStorageReleaseController>(prefab);
                    break;

                case AutomationMechanism.ManualPower:
                    controller = Ensure<AutoManualGeneratorController>(prefab);
                    break;

                case AutomationMechanism.Research:
                    controller = Ensure<AutoResearchController>(prefab);
                    break;

                case AutomationMechanism.Telescope:
                    controller = Ensure<AutoTelescopeController>(prefab);
                    break;

                case AutomationMechanism.ChoreHook:
                    return;

                case AutomationMechanism.Tinker:
                    controller = Ensure<AutoTinkerStationController>(prefab);
                    break;

                case AutomationMechanism.MissionControl:
                    controller = Ensure<AutoMissionControlController>(prefab);
                    break;

                case AutomationMechanism.SpiceGrinder:
                    controller = Ensure<AutoSpiceGrinderController>(prefab);
                    AutoSpiceGrinderController.ConfigurePrefabStorage(prefab);
                    break;

                case AutomationMechanism.IceCooledFan:
                    if (prefab.GetComponent<IceCooledFan>() != null)
                    {
                        controller = Ensure<AutoIceCooledFanController>(prefab);
                    }
                    break;

                case AutomationMechanism.NuclearResearch:
                    controller = Ensure<AutoNuclearResearchCenterController>(prefab);
                    break;

                case AutomationMechanism.GeneticAnalysis:
                    controller = Ensure<AutoGeneticAnalysisStationController>(prefab);
                    break;

                default:
                    controller = Ensure<AutoWorkableController>(prefab);
                    break;
            }

            if (controller != null)
            {
                controller.Configure(entry.OptionKey);
                Attach<AutoBuildingCustomizer>(prefab);
            }
        }

        /// <summary>Returns the controller of a prefab, adding it when missing.</summary>
        /// <remarks>Kept private; see <see cref="AttachController"/>.</remarks>
        /// <typeparam name="T">Controller type.</typeparam>
        /// <param name="prefab">Completed building prefab.</param>
        private static T Ensure<T>(GameObject prefab) where T : AutoWorkControllerBase
        {
            T controller = prefab.GetComponent<T>();
            return controller != null ? controller : prefab.AddComponent<T>();
        }

        /// <summary>Adds a component to a prefab when it is not present yet.</summary>
        /// <typeparam name="T">Automation component type.</typeparam>
        /// <param name="prefab">Completed building prefab.</param>
        private static void Attach<T>(GameObject prefab) where T : MonoBehaviour
        {
            SafeInvoke.Try("Attaching " + typeof(T).Name, delegate
            {
                if (prefab.GetComponent<T>() == null)
                {
                    prefab.AddComponent<T>();
                }
            });
        }

        /// <summary>
        /// Research buildings whose supply delivery may be handed to an
        /// Auto-Sweeper. Vanilla tags those deliveries as research work, a
        /// chore group the Solid Transfer Arm is not allowed to perform, so
        /// bottled water and Data Banks always needed a Duplicant.
        /// </summary>
        private static readonly string[] ResearchDeliveryTargets =
        {
            "ResearchCenter", "AdvancedResearchCenter", "CosmicResearchCenter",
            "DLC1CosmicResearchCenter", "NuclearResearchCenter", "OrbitalResearchCenter"
        };

        /// <summary>
        /// Adds a secondary Auto-Sweeper compatible delivery path to the
        /// Advanced Research Center's water input without removing the
        /// original <c>ResearchFetch</c> delivery that Duplicants use.
        ///
        /// Previous versions replaced every research delivery's chore type
        /// with <c>MachineFetch</c>, which had the side effect of making all
        /// research deliveries machine-only. The new approach adds a second
        /// <see cref="ManualDeliveryKG"/> with <c>MachineFetch</c> only on
        /// the water input of the Advanced Research Center, so both supply
        /// paths coexist: Duplicants still deliver through the original
        /// <c>ResearchFetch</c> and Auto-Sweepers use the new
        /// <c>MachineFetch</c>. No other research building is affected.
        /// </summary>
        /// <param name="prefabId">Prefab identifier of the building.</param>
        /// <param name="prefab">Completed building prefab.</param>
        private static void InjectResearchDelivery(string prefabId, GameObject prefab)
        {
            // Only the Advanced Research Center needs the dual delivery path;
            // its water input is the only research material that the game ships
            // in a bottled (Pickupable) form that an Auto-Sweeper can reach.
            if (prefabId != "AdvancedResearchCenter" || prefab == null)
            {
                return;
            }

            SafeInvoke.Try("Mixed supply for " + prefabId, delegate
            {
                if (Db.Get() == null || Db.Get().ChoreTypes == null || Db.Get().ChoreTypes.MachineFetch == null)
                {
                    return;
                }

                HashedString machineFetchHash = Db.Get().ChoreTypes.MachineFetch.IdHash;
                ManualDeliveryKG[] deliveries = prefab.GetComponents<ManualDeliveryKG>();
                if (deliveries == null)
                {
                    return;
                }

                // Check if a machine delivery is already attached to avoid duplicate components
                for (int i = 0; i < deliveries.Length; i++)
                {
                    if (deliveries[i] != null && deliveries[i].choreTypeIDHash == machineFetchHash)
                    {
                        return; // Already injected!
                    }
                }

                foreach (ManualDeliveryKG delivery in deliveries)
                {
                    if (delivery == null || delivery.RequestedItemTag != GameTags.Water)
                    {
                        continue;
                    }

                    // Add a parallel delivery with MachineFetch so
                    // Auto-Sweepers can supply bottled water. The original
                    // ResearchFetch delivery stays for Duplicants.
                    ManualDeliveryKG machineDelivery =
                        prefab.AddComponent<ManualDeliveryKG>();
                    Storage storage = prefab.GetComponent<Storage>();
                    machineDelivery.SetStorage(storage);
                    machineDelivery.RequestedItemTag = delivery.RequestedItemTag;
                    machineDelivery.refillMass = delivery.refillMass;
                    machineDelivery.capacity = delivery.capacity;
                    machineDelivery.choreTypeIDHash = machineFetchHash;
                    machineDelivery.operationalRequirement =
                        Operational.State.Functional;
                    machineDelivery.allowPause = true;

                    Log.Diagnostic("Dual supply path (Duplicant + Auto-Sweeper) enabled for " +
                             prefabId + ".");
                    break;
                }
            });
        }

        /// <summary>
        /// Applies the optional full (100 %) conversion ratio of the Oil
        /// Refinery. The default option keeps the vanilla 50 % ratio and this
        /// method returns without touching the prefab.
        /// </summary>
        /// <param name="prefab">Oil Refinery prefab.</param>
        private static void ApplyOilRefineryRatio(GameObject prefab)
        {
            SafeInvoke.Try("Oil Refinery conversion ratio override", delegate
            {
                AutoOilRefinery.SyncEfficiency(prefab);
                Log.Diagnostic("Oil Refinery conversion ratio synced.");
            });
        }

        /// <summary>
        /// Maps prefab identifier to registry option key.
        ///
        /// The identifier is read from the <c>ID</c> constant of the config
        /// class through <see cref="FieldInfo.GetRawConstantValue"/>, which
        /// reads the compiled metadata and therefore never triggers the static
        /// constructor of the config class. The class name without the
        /// "Config" suffix is used as a fallback.
        /// </summary>
        private static Dictionary<string, AutomationEntry> BuildDriverKeyIndex()
        {
            Dictionary<string, AutomationEntry> index =
                new Dictionary<string, AutomationEntry>(StringComparer.Ordinal);

            foreach (AutomationEntry entry in AutomationRegistry.Entries)
            {
                AutomationEntry current = entry;
                SafeInvoke.Try("Resolving " + current.ConfigTypeName, delegate
                {
                    Type configType = AccessTools.TypeByName(current.ConfigTypeName);
                    if (configType == null)
                    {
                        Log.Verbose("Skipped " + current.ConfigTypeName +
                                    " (content not installed).");
                        return;
                    }

                    index[ResolvePrefabId(configType, current)] = current;
                });
            }

            return index;
        }

        /// <summary>Reads the prefab identifier declared by a config class.</summary>
        /// <param name="configType">Building config class.</param>
        /// <param name="entry">Registry row of the building.</param>
        private static string ResolvePrefabId(Type configType, AutomationEntry entry)
        {
            FieldInfo idField = configType.GetField(
                "ID", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            if (idField != null && idField.FieldType == typeof(string) && idField.IsLiteral)
            {
                string value = idField.GetRawConstantValue() as string;
                if (!string.IsNullOrEmpty(value))
                {
                    return value;
                }
            }

            return AutomationRegistry.PrefabIdOf(entry);
        }
    }

    /// <summary>
    /// Fallback patch ensuring any dynamically spawned or modded SolidTransferArm
    /// instances receive the AutoSweeperHarvestController.
    /// </summary>
    [HarmonyPatch(typeof(SolidTransferArm), "OnSpawn")]
    internal static class SolidTransferArm_OnSpawn_Patch
    {
        public static void Postfix(SolidTransferArm __instance)
        {
            if (__instance != null && __instance.gameObject != null)
            {
                __instance.gameObject.AddOrGet<AutoSweeperHarvestController>();
                if (__instance.gameObject.GetComponent<SymbolOverrideController>() == null)
                {
                    SymbolOverrideControllerUtil.AddToPrefab(__instance.gameObject);
                }
            }
        }
    }

    /// <summary>
    /// Injects ColonyAutomationMasterRegistry onto SaveGame root for dual-layer colony-wide persistence.
    /// </summary>
    [HarmonyPatch(typeof(SaveGame), "OnPrefabInit")]
    internal static class SaveGameRegistryInjection
    {
        internal static void Postfix(SaveGame __instance)
        {
            if (__instance != null && __instance.GetComponent<ColonyAutomationMasterRegistry>() == null)
            {
                __instance.gameObject.AddComponent<ColonyAutomationMasterRegistry>();
            }
        }
    }
}
