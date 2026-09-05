// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace AutoMachineRebuilt.Automation
{
    /// <summary>
    /// Table of every building driven by the generic work driver.
    ///
    /// Adding a building only requires one row here, one option property in
    /// <see cref="AutoMachineRebuilt.Config.AutoMachineOptions"/> and one
    /// English string in <see cref="AutoMachineRebuilt.Localization.Translations"/>;
    /// the patching, the option lookup and the localized option title are all
    /// derived from the row.
    /// </summary>
    internal static class AutomationRegistry
    {
        /// <summary>Option key used for the geyser first-study automation.</summary>
        internal const string GeyserStudyKey = "GEYSERSTUDY";

        /// <summary>All table driven automation targets.</summary>
        internal static readonly AutomationEntry[] Entries =
        {
            // --- Manually operated buildings (no room requirement) --------
            new AutomationEntry("MANUALGENERATOR", "ManualGeneratorConfig", false, AutomationMechanism.ManualPower),
            new AutomationEntry("TELESCOPE", "TelescopeConfig", false, AutomationMechanism.Telescope),
            new AutomationEntry("CLUSTERTELESCOPE", "ClusterTelescopeConfig", false, AutomationMechanism.Telescope),
            new AutomationEntry("CLUSTERTELESCOPEENCLOSED", "ClusterTelescopeEnclosedConfig", false, AutomationMechanism.Telescope),
            // The Manual Radbolt Generator is a ComplexFabricator behind its
            // own state machine. Running it as a fabricator keeps the vanilla
            // recipe timer, the progress meter and the machine animation,
            // which a hand driven Workable tick cannot reproduce.
            new AutomationEntry("MANUALHIGHENERGYPARTICLESPAWNER", "ManualHighEnergyParticleSpawnerConfig", false, AutomationMechanism.Fabricator),
            new AutomationEntry("RESETSKILLSSTATION", "ResetSkillsStationConfig", false),
            new AutomationEntry("ICEKETTLE", "IceKettleConfig", false, AutomationMechanism.Release),
            new AutomationEntry("CAMPFIRE", "CampfireConfig", false),
            new AutomationEntry("ICECOOLEDFAN", "IceCooledFanConfig", false, AutomationMechanism.IceCooledFan),
            new AutomationEntry("COMPOST", "CompostConfig", false, AutomationMechanism.ChoreHook),
            new AutomationEntry("FOODDEHYDRATOR", "FoodDehydratorConfig", false, AutomationMechanism.Release),

            // --- Research and analysis buildings --------------------------
            new AutomationEntry("RESEARCHCENTER", "ResearchCenterConfig", false, AutomationMechanism.Research),
            new AutomationEntry("ADVANCEDRESEARCHCENTER", "AdvancedResearchCenterConfig", false, AutomationMechanism.Research),
            new AutomationEntry("COSMICRESEARCHCENTER", "CosmicResearchCenterConfig", false, AutomationMechanism.Research),
            new AutomationEntry("DLC1COSMICRESEARCHCENTER", "DLC1CosmicResearchCenterConfig", false, AutomationMechanism.Research),
            new AutomationEntry("NUCLEARRESEARCHCENTER", "NuclearResearchCenterConfig", false, AutomationMechanism.NuclearResearch),
            new AutomationEntry("ORBITALRESEARCHCENTER", "OrbitalResearchCenterConfig", false, AutomationMechanism.Fabricator),
            new AutomationEntry("GENETICANALYSISSTATION", "GeneticAnalysisStationConfig", false, AutomationMechanism.GeneticAnalysis),
            // --- Buildings with a mandatory vanilla room ------------------
            new AutomationEntry("MISSIONCONTROL", "MissionControlConfig", true, AutomationMechanism.MissionControl),
            new AutomationEntry("MISSIONCONTROLCLUSTER", "MissionControlClusterConfig", true, AutomationMechanism.MissionControl),
            new AutomationEntry("FARMSTATION", "FarmStationConfig", true, AutomationMechanism.Tinker),
            new AutomationEntry("SPICEGRINDER", "SpiceGrinderConfig", true, AutomationMechanism.SpiceGrinder),
            new AutomationEntry("POWERCONTROLSTATION", "PowerControlStationConfig", true, AutomationMechanism.Tinker),

            // --- Additional fabricators ----------------------------------
            new AutomationEntry("FABRICATEDWOODMAKER", "FabricatedWoodMakerConfig", false, AutomationMechanism.Fabricator),
            new AutomationEntry("SUSHIBAR", "SushiBarConfig", false, AutomationMechanism.Fabricator),

            // --- Buildings that only need their output carried out --------
            new AutomationEntry("DESALINATOR", "DesalinatorConfig", false, AutomationMechanism.Release),
            new AutomationEntry("MILKFATSEPARATOR", "MilkFatSeparatorConfig", false, AutomationMechanism.ChoreHook)
        };

        /// <summary>Fast lookup of an entry by its option key.</summary>
        private static readonly Dictionary<string, AutomationEntry> ByKey = BuildIndex();

        /// <summary>Builds the option key index once.</summary>
        private static Dictionary<string, AutomationEntry> BuildIndex()
        {
            Dictionary<string, AutomationEntry> index =
                new Dictionary<string, AutomationEntry>(StringComparer.Ordinal);

            foreach (AutomationEntry entry in Entries)
            {
                index[entry.OptionKey] = entry;
            }

            return index;
        }

        /// <summary>Returns the entry of an option key, or <c>null</c>.</summary>
        /// <param name="optionKey">Option key stored on the driver.</param>
        internal static AutomationEntry Find(string optionKey)
        {
            AutomationEntry entry;
            return string.IsNullOrEmpty(optionKey) || !ByKey.TryGetValue(optionKey, out entry)
                ? null
                : entry;
        }

        /// <summary>Prefab identifier belonging to an entry.</summary>
        /// <param name="entry">Registry row.</param>
        internal static string PrefabIdOf(AutomationEntry entry)
        {
            string name = entry.ConfigTypeName;
            return name.EndsWith("Config", StringComparison.Ordinal)
                ? name.Substring(0, name.Length - "Config".Length)
                : name;
        }
    }
}
