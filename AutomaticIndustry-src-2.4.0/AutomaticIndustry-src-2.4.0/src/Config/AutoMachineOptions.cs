// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PeterHan.PLib.Options;
using UnityEngine;

namespace AutoMachineRebuilt.Config
{
    /// <summary>
    /// Player facing configuration. Every automated building has its own
    /// toggle so players can opt in or out per building type.
    ///
    /// Option titles are resolved at runtime from the vanilla building names
    /// (see <see cref="AutoMachineRebuilt.Localization.OptionStrings"/>), which
    /// keeps the options screen consistent with the player's game language.
    /// </summary>
    [ModInfo("https://steamcommunity.com/sharedfiles/filedetails/?id=3782701870")]
    [ConfigFile("config.json", IndentOutput: true, SharedConfigLocation: true)]
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class AutoMachineOptions : SingletonOptions<AutoMachineOptions>, IOptions
    {
        /// <summary>Options category for classic fabricator buildings.</summary>
        public const string CategoryFabricators = "STRINGS.AUTOMACHINEREBUILT.CATEGORY.FABRICATORS";

        /// <summary>Options category for buildings driven by custom logic.</summary>
        public const string CategorySpecial = "STRINGS.AUTOMACHINEREBUILT.CATEGORY.SPECIAL";

        /// <summary>Options category for critter ranching buildings.</summary>
        public const string CategoryRanching = "STRINGS.AUTOMACHINEREBUILT.CATEGORY.RANCHING";

        /// <summary>Options category for manually operated buildings.</summary>
        public const string CategoryManual = "STRINGS.AUTOMACHINEREBUILT.CATEGORY.MANUAL";

        /// <summary>Options category for research and analysis buildings.</summary>
        public const string CategoryResearch = "STRINGS.AUTOMACHINEREBUILT.CATEGORY.RESEARCH";

        /// <summary>Options category for buildings that require a room.</summary>
        public const string CategoryRoom = "STRINGS.AUTOMACHINEREBUILT.CATEGORY.ROOM";

        /// <summary>Options category for buildings with skill requirements.</summary>
        public const string CategorySkill = "STRINGS.AUTOMACHINEREBUILT.CATEGORY.SKILL";

        /// <summary>Options category for global tuning values.</summary>
        public const string CategoryGeneral = "STRINGS.AUTOMACHINEREBUILT.CATEGORY.GENERAL";

        /// <summary>Options category for progress bars.</summary>
        public const string CategoryProgressBars = "STRINGS.AUTOMACHINEREBUILT.CATEGORY.PROGRESSBARS";

        /// <summary>Options category for Auto-Sweeper automation.</summary>
        public const string CategoryAutoSweeper = "STRINGS.AUTOMACHINEREBUILT.CATEGORY.AUTOSWEEPER";

        /// <summary>Options category for Liquid Reservoir automation.</summary>
        public const string CategoryLiquidReservoir = "STRINGS.AUTOMACHINEREBUILT.CATEGORY.LIQUIDRESERVOIR";

        /// <summary>Options category for diagnostics and logging.</summary>
        public const string CategoryLogging = "STRINGS.AUTOMACHINEREBUILT.CATEGORY.LOGGING";

        // ------------------------------------------------------------------
        // General (declared first so the category appears on top)
        // ------------------------------------------------------------------

        /// <summary>
        /// Master switch: enables every automation at once without touching
        /// the individual toggles, so turning it off restores the player's
        /// hand picked selection.
        /// </summary>
        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.ENABLEALL", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.ENABLEALL", CategoryGeneral)]
        [JsonProperty]
        public bool EnableAllAutomation { get; set; }

        /// <summary>Language used for the mod's own option labels.</summary>
        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.LANGUAGE", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.LANGUAGE", CategoryGeneral)]
        [JsonProperty]
        public UiLanguage OptionsLanguage { get; set; }

        /// <summary>Shows the English text next to the translated text.</summary>
        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.BILINGUAL", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.BILINGUAL", CategoryGeneral)]
        [JsonProperty]
        public bool BilingualLabels { get; set; }

        /// <summary>Allows per-building automation toggle buttons on building info screens.</summary>
        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.ENABLEPERBUILDINGCUSTOMIZATION", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.ENABLEPERBUILDINGCUSTOMIZATION", CategoryGeneral)]
        [JsonProperty]
        public bool EnablePerBuildingCustomization { get; set; } = true;

        /// <summary>Controls how building automation is switched on/off.</summary>
        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.TOGGLEMODE", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.TOGGLEMODE", CategoryGeneral)]
        [JsonProperty]
        public AutomationToggleMode ToggleMode { get; set; } = AutomationToggleMode.SmartHybrid;

        /// <summary>
        /// Action button to open the visual dual-pane building configuration editor.
        /// </summary>
        [Option("STRINGS.AUTOMACHINEREBUILT.OPTIONS.BUILDING_EDITOR.NAME", "STRINGS.AUTOMACHINEREBUILT.OPTIONS.BUILDING_EDITOR.TOOLTIP", null)]
        [JsonIgnore]
        public Action<object> Button_OpenBuildingConfigEditor => delegate
        {
            AutoMachineRebuilt.UI.BuildingConfigEditorScreen.ShowBuildingEditor(null);
        };


        [JsonProperty]
        public bool UnmannedCookingStation { get; set; }

        [JsonProperty]
        public bool UnmannedGourmetCookingStation { get; set; }

        [JsonProperty]
        public bool UnmannedMicrobeMusher { get; set; }

        [JsonProperty]
        public bool UnmannedDeepfryer { get; set; }

        [JsonProperty]
        public bool UnmannedMilkPress { get; set; }

        [JsonProperty]
        public bool UnmannedSmoker { get; set; }

        [JsonProperty]
        public bool UnmannedRockCrusher { get; set; }

        [JsonProperty]
        public bool UnmannedMetalRefinery { get; set; }

        [JsonProperty]
        public bool UnmannedGlassForge { get; set; }

        [JsonProperty]
        public bool UnmannedSupermaterialRefinery { get; set; }

        [JsonProperty]
        public bool UnmannedSuitFabricator { get; set; }

        [JsonProperty]
        public bool UnmannedClothingFabricator { get; set; }

        [JsonProperty]
        public bool UnmannedClothingAlterationStation { get; set; }

        [JsonProperty]
        public bool UnmannedCraftingTable { get; set; }

        [JsonProperty]
        public bool UnmannedAdvancedCraftingTable { get; set; }

        [JsonProperty]
        public bool UnmannedSludgePress { get; set; }

        [JsonProperty]
        public bool UnmannedDiamondPress { get; set; }

        [JsonProperty]
        public bool UnmannedChemicalRefinery { get; set; }

        [JsonProperty]
        public bool UnmannedMissileFabricator { get; set; }

        [JsonProperty]
        public bool UnmannedDataMiner { get; set; }

        // ------------------------------------------------------------------
        // Buildings that need dedicated automation logic
        // ------------------------------------------------------------------

        [JsonProperty]
        public bool UnmannedOilRefinery { get; set; }

        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.OILREFINERYEFFICIENCY", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.OILREFINERYEFFICIENCY", CategorySpecial)]
        [JsonProperty]
        public OilRefineryEfficiency OilRefineryEfficiency { get; set; }

        [JsonProperty]
        public bool UnmannedOilWellCap { get; set; }

        [JsonProperty]
        public bool UnmannedDesalinator { get; set; }

        [JsonProperty]
        public bool UnmannedMilkFatSeparator { get; set; }

        [JsonProperty]
        public bool UnmannedGeoTuner { get; set; }

        [JsonProperty]
        public bool UnmannedLiquidBottler { get; set; }

        [JsonProperty]
        public bool UnmannedGasBottler { get; set; }

        [JsonProperty]
        public bool UnmannedLiquidPumpingStation { get; set; }

        /// <summary>
        /// Applies a new flow setting of the Liquid Valve without the vanilla
        /// chore, so liquid networks can be re-balanced automatically.
        /// </summary>
        [JsonProperty]
        public bool UnmannedLiquidValve { get; set; }

        /// <summary>
        /// Applies a new flow setting of the Gas Valve without the vanilla
        /// chore, so gas networks can be re-balanced automatically.
        /// </summary>
        [JsonProperty]
        public bool UnmannedGasValve { get; set; }

        /// <summary>
        /// Backwards compatibility property for existing saves/configs where UnmannedValves was a single boolean.
        /// </summary>
        [JsonProperty("UnmannedValves", NullValueHandling = NullValueHandling.Ignore)]
        public bool? LegacyUnmannedValves
        {
            get => null;
            set
            {
                if (value.HasValue)
                {
                    UnmannedLiquidValve = value.Value;
                    UnmannedGasValve = value.Value;
                }
            }
        }

        // ------------------------------------------------------------------
        // Ranching
        // ------------------------------------------------------------------

        [JsonProperty]
        public bool UnmannedGroomingStation { get; set; }

        [JsonProperty]
        public bool UnmannedShearingStation { get; set; }

        [JsonProperty]
        public bool UnmannedMilkingStation { get; set; }

        [JsonProperty]
        public bool UnmannedUnderwaterGroomingStation { get; set; }

        [JsonProperty]
        public bool UnmannedUnderwaterShearingStation { get; set; }

        [JsonProperty]
        public bool UnmannedUnderwaterMilkingStation { get; set; }

        // ------------------------------------------------------------------
        // General
        // ------------------------------------------------------------------

        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.RANCHINTERVAL", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.RANCHINTERVAL", CategoryGeneral)]
        [Limit(5, 600)]
        [JsonProperty]
        public int RanchIntervalSeconds { get; set; }

        // ------------------------------------------------------------------
        // Additional fabricators (duplicantOperated = false)
        // ------------------------------------------------------------------

        [JsonProperty]
        public bool UnmannedApothecary { get; set; }

        [JsonProperty]
        public bool UnmannedAdvancedApothecary { get; set; }

        [JsonProperty]
        public bool UnmannedSushiBar { get; set; }

        [JsonProperty]
        public bool UnmannedFabricatedWoodMaker { get; set; }

        // ------------------------------------------------------------------
        // Manually operated buildings driven by AutoWorkableController
        // ------------------------------------------------------------------

        [JsonProperty]
        public bool UnmannedManualGenerator { get; set; }

        [JsonProperty]
        public bool UnmannedTelescope { get; set; }

        [JsonProperty]
        public bool UnmannedClusterTelescope { get; set; }

        [JsonProperty]
        public bool UnmannedClusterTelescopeEnclosed { get; set; }

        [JsonProperty]
        public bool UnmannedManualHighEnergyParticleSpawner { get; set; }

        [JsonProperty]
        public bool UnmannedResetSkillsStation { get; set; }

        [JsonProperty]
        public bool UnmannedIceKettle { get; set; }

        [JsonProperty]
        public bool UnmannedCampfire { get; set; }

        [JsonProperty]
        public bool UnmannedIceCooledFan { get; set; }

        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.IGNORETOOCOLD.ICECOOLEDFAN", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.IGNORETOOCOLD.ICECOOLEDFAN", CategoryManual)]
        [JsonProperty]
        public bool IgnoreTooColdIceCooledFan { get; set; }

        [JsonProperty]
        public bool UnmannedCompost { get; set; }

        [JsonProperty]
        public bool UnmannedFoodDehydrator { get; set; }

        // ------------------------------------------------------------------
        // Research and analysis buildings
        // ------------------------------------------------------------------

        [JsonProperty]
        public bool UnmannedResearchCenter { get; set; }

        [JsonProperty]
        public bool UnmannedAdvancedResearchCenter { get; set; }

        [JsonProperty]
        public bool UnmannedCosmicResearchCenter { get; set; }

        [JsonProperty]
        public bool UnmannedDlc1CosmicResearchCenter { get; set; }

        [JsonProperty]
        public bool UnmannedNuclearResearchCenter { get; set; }

        [JsonProperty]
        public bool UnmannedOrbitalResearchCenter { get; set; }

        [JsonProperty]
        public bool UnmannedGeneticAnalysisStation { get; set; }

        [JsonProperty]
        public bool UnmannedMorbRoverMaker { get; set; }

        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.GEYSERSTUDY", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.GEYSERSTUDY", CategoryResearch)]
        [JsonProperty]
        public bool UnmannedGeyserStudy { get; set; }

        // ------------------------------------------------------------------
        // Buildings with a mandatory vanilla room requirement
        // ------------------------------------------------------------------

        [JsonProperty]
        public bool UnmannedMissionControl { get; set; }

        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.IGNOREROOM.MISSIONCONTROL", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.IGNOREROOM", CategoryRoom)]
        [JsonProperty]
        public bool IgnoreRoomMissionControl { get; set; }

        [JsonProperty]
        public bool UnmannedMissionControlCluster { get; set; }

        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.IGNOREROOM.MISSIONCONTROLCLUSTER", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.IGNOREROOM", CategoryRoom)]
        [JsonProperty]
        public bool IgnoreRoomMissionControlCluster { get; set; }

        [JsonProperty]
        public bool UnmannedFarmStation { get; set; }

        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.IGNOREROOM.FARMSTATION", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.IGNOREROOM", CategoryRoom)]
        [JsonProperty]
        public bool IgnoreRoomFarmStation { get; set; }

        /// <summary>
        /// Lets the Farm Station produce fertilizer even when no plant asked
        /// for it. Vanilla only produces on demand, so this is opt in.
        /// </summary>
        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.IGNORECROPDEMAND", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.IGNORECROPDEMAND", CategoryRoom)]
        [JsonProperty]
        public bool IgnoreCropDemandFarmStation { get; set; }

        /// <summary>
        /// Re-tags the delivery task of the research buildings as a machine
        /// delivery so an Auto-Sweeper may supply them. Vanilla marks those
        /// deliveries as research work, which only a Duplicant can perform.
        /// Applied while the building prefabs are built, therefore a restart
        /// is required.
        /// </summary>
        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.SWEEPERRESEARCH", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.SWEEPERRESEARCH", CategoryResearch)]
        [JsonProperty]
        public bool SweeperResearchDelivery { get; set; }

        [JsonProperty]
        public bool UnmannedSpiceGrinder { get; set; }

        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.IGNOREROOM.SPICEGRINDER", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.IGNOREROOM", CategoryRoom)]
        [JsonProperty]
        public bool IgnoreRoomSpiceGrinder { get; set; }

        [JsonProperty]
        public bool UnmannedPowerControlStation { get; set; }

        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.IGNOREROOM.POWERCONTROLSTATION", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.IGNOREROOM", CategoryRoom)]
        [JsonProperty]
        public bool IgnoreRoomPowerControlStation { get; set; }

        /// <summary>
        /// Drops the "Power Tinkering" skill perk requirement of the Power
        /// Control Station so the vanilla chore stays available to every
        /// Duplicant. The automation itself never uses a Duplicant.
        /// </summary>
        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.IGNORESKILL.POWERCONTROLSTATION", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.IGNORESKILL", CategorySkill)]
        [JsonProperty]
        public bool IgnoreSkillPowerControlStation { get; set; }

        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.IGNOREROOM.GEOTUNER", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.IGNOREROOM", CategoryRoom)]
        [JsonProperty]
        public bool IgnoreRoomGeoTuner { get; set; }

        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.IGNOREDEMAND.POWERCONTROLSTATION", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.IGNOREDEMAND.POWERCONTROLSTATION", CategoryRoom)]
        [JsonProperty]
        public bool IgnorePowerDemandPowerControlStation { get; set; }

        // ------------------------------------------------------------------
        // Progress Bars Category
        // ------------------------------------------------------------------

        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.PROGRESSBAR.RESEARCH", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.PROGRESSBAR.RESEARCH", CategoryProgressBars)]
        [JsonProperty]
        public bool ProgressBarResearch { get; set; } = true;

        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.PROGRESSBAR.GEOTUNER", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.PROGRESSBAR.GEOTUNER", CategoryProgressBars)]
        [JsonProperty]
        public bool ProgressBarGeoTuner { get; set; } = true;

        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.PROGRESSBAR.BOTTLER", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.PROGRESSBAR.BOTTLER", CategoryProgressBars)]
        [JsonProperty]
        public bool ProgressBarBottler { get; set; } = true;

        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.PROGRESSBAR.FABRICATORS", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.PROGRESSBAR.FABRICATORS", CategoryProgressBars)]
        [JsonProperty]
        public bool ProgressBarFabricators { get; set; } = true;

        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.PROGRESSBAR.TELESCOPES", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.PROGRESSBAR.TELESCOPES", CategoryProgressBars)]
        [JsonProperty]
        public bool ProgressBarTelescopes { get; set; } = true;

        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.PROGRESSBAR.SPICEGRINDER", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.PROGRESSBAR.SPICEGRINDER", CategoryProgressBars)]
        [JsonProperty]
        public bool ProgressBarSpiceGrinder { get; set; } = true;

        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.PROGRESSBAR.GLEANER", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.PROGRESSBAR.GLEANER", CategoryProgressBars)]
        [JsonProperty]
        public bool ProgressBarGleaner { get; set; } = true;

        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.PROGRESSBAR.POWERCONTROLSTATION", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.PROGRESSBAR.POWERCONTROLSTATION", CategoryProgressBars)]
        [JsonProperty]
        public bool ProgressBarPowerControlStation { get; set; } = true;

        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.PROGRESSBAR.GEYSERTUNING", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.PROGRESSBAR.GEYSERTUNING", CategoryProgressBars)]
        [JsonProperty]
        public bool ProgressBarGeyserTuning { get; set; } = true;

        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.HIDEWORLDICONSKILL", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.HIDEWORLDICONSKILL", CategoryGeneral)]
        [JsonProperty]
        public bool HideWorldIconSkillRequirement { get; set; } = false;

        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.HIDEWORLDICONROOM", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.HIDEWORLDICONROOM", CategoryGeneral)]
        [JsonProperty]
        public bool HideWorldIconRoomRequirement { get; set; } = false;

        // ------------------------------------------------------------------
        // Auto-Sweeper (Solid Transfer Arm)
        // ------------------------------------------------------------------

        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.AUTOSWEEPERHARVEST", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.AUTOSWEEPERHARVEST", CategoryAutoSweeper)]
        [JsonProperty]
        public bool AutoSweeperHarvest { get; set; } = true;

        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.AUTOSWEEPERRESPECTHARVESTDESIGNATION", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.AUTOSWEEPERRESPECTHARVESTDESIGNATION", CategoryAutoSweeper)]
        [JsonProperty]
        public bool AutoSweeperRespectHarvestDesignation { get; set; } = true;

        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.AUTOSWEEPERCANCELDUPEHARVESTCHORE", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.AUTOSWEEPERCANCELDUPEHARVESTCHORE", CategoryAutoSweeper)]
        [JsonProperty]
        public bool AutoSweeperCancelDupeHarvestChore { get; set; } = true;

        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.AUTOSWEEPERBOTTLERDIRECTPICKUP", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.AUTOSWEEPERBOTTLERDIRECTPICKUP", CategoryAutoSweeper)]
        [JsonProperty]
        public bool AutoSweeperBottlerDirectPickup { get; set; } = true;

        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.AUTOSWEEPERSCANINTERVAL", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.AUTOSWEEPERSCANINTERVAL", CategoryAutoSweeper)]
        [Limit(0.5f, 5.0f)]
        [JsonProperty]
        public float AutoSweeperScanInterval { get; set; } = 1.5f;

        // ------------------------------------------------------------------
        // Liquid Reservoir (Storage Fetch & Convey)
        // ------------------------------------------------------------------

        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.LIQUIDRESERVOIRDUPLICANTFETCH", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.LIQUIDRESERVOIRDUPLICANTFETCH", CategoryLiquidReservoir)]
        [JsonProperty]
        public bool LiquidReservoirDuplicantFetch { get; set; } = true;

        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.LIQUIDRESERVOIRAUTOSWEEPERFETCH", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.LIQUIDRESERVOIRAUTOSWEEPERFETCH", CategoryLiquidReservoir)]
        [JsonProperty]
        public bool LiquidReservoirAutoSweeperFetch { get; set; } = true;

        // ------------------------------------------------------------------
        // Diagnostics and Logging
        // ------------------------------------------------------------------

        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.ENABLEDIAGNOSTICLOGGING", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.ENABLEDIAGNOSTICLOGGING", CategoryLogging)]
        [JsonProperty]
        public bool EnableDiagnosticLogging { get; set; } = false;

        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.LOGBUILDINGAUTOMATION", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.LOGBUILDINGAUTOMATION", CategoryLogging)]
        [JsonProperty]
        public bool LogBuildingAutomation { get; set; } = false;

        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.LOGBUILDINGINTERACTIONS", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.LOGBUILDINGINTERACTIONS", CategoryLogging)]
        [JsonProperty]
        public bool LogBuildingInteractions { get; set; } = false;

        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.LOGPROBLEMATICBUILDINGS", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.LOGPROBLEMATICBUILDINGS", CategoryLogging)]
        [JsonProperty]
        public bool LogProblematicBuildings { get; set; } = false;

        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.VERBOSELOGGING", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.VERBOSELOGGING", CategoryLogging)]
        [JsonProperty]
        public bool VerboseLogging { get; set; } = false;

        /// <summary>
        /// Creates an options instance holding the shipped defaults.
        /// Buildings automated by the original mod default to enabled, newly
        /// automated buildings default to disabled so existing colonies keep
        /// behaving the way their owner expects.
        /// </summary>
        public AutoMachineOptions()
        {
            UnmannedCookingStation = true;
            UnmannedGourmetCookingStation = true;
            UnmannedMicrobeMusher = true;
            UnmannedDeepfryer = true;
            UnmannedMilkPress = true;
            UnmannedSmoker = true;
            UnmannedRockCrusher = true;
            UnmannedMetalRefinery = true;
            UnmannedGlassForge = true;
            UnmannedSupermaterialRefinery = true;
            UnmannedSuitFabricator = true;
            UnmannedClothingFabricator = true;
            UnmannedClothingAlterationStation = true;
            UnmannedCraftingTable = true;
            UnmannedAdvancedCraftingTable = true;
            UnmannedSludgePress = true;
            UnmannedDiamondPress = true;
            UnmannedChemicalRefinery = true;
            UnmannedMissileFabricator = true;
            UnmannedDataMiner = true;
            UnmannedOilRefinery = true;

            OilRefineryEfficiency = OilRefineryEfficiency.Vanilla50;

            UnmannedOilWellCap = false;
            UnmannedGeoTuner = true;
            IgnoreRoomGeoTuner = false;
            UnmannedMorbRoverMaker = true;
            UnmannedLiquidBottler = true;
            UnmannedGasBottler = true;
            UnmannedLiquidPumpingStation = true;
            UnmannedGroomingStation = false;
            UnmannedShearingStation = false;
            UnmannedMilkingStation = false;
            UnmannedUnderwaterGroomingStation = false;
            UnmannedUnderwaterShearingStation = false;
            UnmannedUnderwaterMilkingStation = false;

            UnmannedApothecary = false;
            UnmannedAdvancedApothecary = false;
            UnmannedSushiBar = false;
            UnmannedFabricatedWoodMaker = false;
            UnmannedPowerControlStation = true;
            IgnoreRoomPowerControlStation = false;
            IgnoreSkillPowerControlStation = false;
            IgnorePowerDemandPowerControlStation = false;

            ProgressBarResearch = true;
            ProgressBarGeoTuner = true;
            ProgressBarBottler = true;
            ProgressBarFabricators = true;
            ProgressBarTelescopes = true;
            ProgressBarSpiceGrinder = true;
            ProgressBarGleaner = true;
            ProgressBarPowerControlStation = true;
            ProgressBarGeyserTuning = true;
            HideWorldIconSkillRequirement = false;
            HideWorldIconRoomRequirement = false;
            EnableAllAutomation = false;
            UnmannedManualGenerator = false;
            UnmannedTelescope = false;
            UnmannedClusterTelescope = false;
            UnmannedClusterTelescopeEnclosed = false;
            UnmannedManualHighEnergyParticleSpawner = false;
            UnmannedResetSkillsStation = false;
            UnmannedIceKettle = false;
            UnmannedCampfire = false;
            UnmannedIceCooledFan = false;
            IgnoreTooColdIceCooledFan = false;
            UnmannedCompost = false;
            UnmannedFoodDehydrator = false;
            UnmannedResearchCenter = false;
            UnmannedAdvancedResearchCenter = false;
            UnmannedCosmicResearchCenter = false;
            UnmannedDlc1CosmicResearchCenter = false;
            UnmannedNuclearResearchCenter = false;
            UnmannedOrbitalResearchCenter = false;
            UnmannedGeneticAnalysisStation = false;
            UnmannedMorbRoverMaker = false;
            UnmannedMissionControl = false;
            UnmannedMissionControlCluster = false;
            UnmannedFarmStation = false;
            UnmannedSpiceGrinder = false;
            UnmannedGeyserStudy = false;
            UnmannedDesalinator = false;
            UnmannedMilkFatSeparator = false;
            UnmannedLiquidValve = false;
            UnmannedGasValve = false;
            SweeperResearchDelivery = false;
            IgnoreRoomMissionControl = false;
            IgnoreRoomMissionControlCluster = false;
            IgnoreRoomFarmStation = false;
            IgnoreCropDemandFarmStation = false;
            IgnoreRoomSpiceGrinder = true;
            AutoSweeperHarvest = true;
            AutoSweeperRespectHarvestDesignation = true;
            AutoSweeperCancelDupeHarvestChore = true;
            AutoSweeperBottlerDirectPickup = true;
            LiquidReservoirDuplicantFetch = true;
            LiquidReservoirAutoSweeperFetch = true;
            AutoSweeperScanInterval = 1.5f;

            RanchIntervalSeconds = 30;
            VerboseLogging = false;

            OptionsLanguage = UiLanguage.Auto;
            BilingualLabels = true;
        }

        /// <summary>
        /// Required by <see cref="IOptions"/>; the mod declares all of its
        /// options through attributes and adds none at runtime.
        /// </summary>
        /// <returns>An empty option collection.</returns>
        public IEnumerable<IOptionsEntry> CreateOptions()
        {
            yield break;
        }

        /// <summary>
        /// Applies the settings the player just confirmed without a restart.
        ///
        /// Every controller reads <see cref="SingletonOptions{T}.Instance"/>
        /// on each evaluation, so refreshing the singleton is enough to
        /// enable or disable a building live. The option labels are rebuilt
        /// immediately as well, which is what makes the language selector work
        /// while the game is running.
        /// </summary>
        public void OnOptionsChanged()
        {
            // PLib deserializes the dialog result into a fresh instance and
            // raises this callback on it, so "this" is the updated object.
            Instance = this;

            bool textChanged = OptionsLanguage != lastAppliedLanguage ||
                               BilingualLabels != lastAppliedBilingual;
            lastAppliedLanguage = OptionsLanguage;
            lastAppliedBilingual = BilingualLabels;

            // The master switch behaves like a button: it turns every
            // per building toggle on, clears itself and persists the result
            // so the checkboxes below show the new state.
            bool masterApplied = ApplyMasterSwitch();

            AutoMachineRebuilt.Localization.OptionTextBinder.Apply();

            if (textChanged || masterApplied)
            {
                // The dialog rows were built with the previous strings and
                // PLib has no API to relabel them in place, so the screen is
                // rebuilt once. Without this the new language would only show
                // up the next time the player opens the options.
                AutoMachineRebuilt.Localization.OptionsScreenRefresher.Reopen();
            }
        }

        /// <summary>
        /// Turns every "Unmanned*" toggle on when the player used the master
        /// switch, then clears the switch itself and writes the settings back
        /// to disk. Reflection keeps this in sync automatically whenever a new
        /// building toggle is added.
        /// </summary>
        /// <returns><c>true</c> when the switch was used this time.</returns>
        private bool ApplyMasterSwitch()
        {
            if (!EnableAllAutomation)
            {
                return false;
            }

            EnableAllAutomation = false;

            foreach (System.Reflection.PropertyInfo property in GetType().GetProperties())
            {
                if (property.PropertyType != typeof(bool) ||
                    !property.CanRead || !property.CanWrite ||
                    !property.Name.StartsWith("Unmanned", StringComparison.Ordinal))
                {
                    continue;
                }

                property.SetValue(this, true, null);
            }

            try
            {
                POptions.WriteSettings(this);
            }
            catch (Exception e)
            {
                AutoMachineRebuilt.Util.Log.Warn(
                    "Could not persist the master automation switch: " + e.Message);
            }

            return true;
        }

        /// <summary>
        /// Language the option labels were last built with. Static on
        /// purpose: PLib hands every dialog result to a freshly deserialized
        /// instance, so an instance field would always compare the new choice
        /// against the class defaults and miss a change back to "Auto".
        /// </summary>
        private static UiLanguage lastAppliedLanguage = (UiLanguage)(-1);

        /// <summary>
        /// Bilingual flag the option labels were last built with. Static for
        /// the same reason as <see cref="lastAppliedLanguage"/>.
        /// </summary>
        private static bool lastAppliedBilingual;

        /// <summary>
        /// Records the language the option labels were actually built with.
        /// Called by the text binder, including on the initial bind at load,
        /// so the first dialog the player confirms is only rebuilt when the
        /// selection really changed.
        /// </summary>
        /// <param name="language">Language the labels were built with.</param>
        /// <param name="bilingual">Bilingual flag used for the labels.</param>
        internal static void NoteAppliedLabels(UiLanguage language, bool bilingual)
        {
            lastAppliedLanguage = language;
            lastAppliedBilingual = bilingual;
        }

        /// <summary>
        /// Maps a building prefab identifier to the option property that
        /// controls it. Populated once and reused for every spawned building.
        /// </summary>
        private static readonly Dictionary<string, Func<AutoMachineOptions, bool>> ToggleByPrefabId =
            new Dictionary<string, Func<AutoMachineOptions, bool>>(StringComparer.OrdinalIgnoreCase)
            {
                { "CookingStation", o => o.UnmannedCookingStation },
                { "GourmetCookingStation", o => o.UnmannedGourmetCookingStation },
                { "MicrobeMusher", o => o.UnmannedMicrobeMusher },
                { "Deepfryer", o => o.UnmannedDeepfryer },
                { "MilkPress", o => o.UnmannedMilkPress },
                { "Smoker", o => o.UnmannedSmoker },
                { "RockCrusher", o => o.UnmannedRockCrusher },
                { "MetalRefinery", o => o.UnmannedMetalRefinery },
                { "GlassForge", o => o.UnmannedGlassForge },
                { "SupermaterialRefinery", o => o.UnmannedSupermaterialRefinery },
                { "SuitFabricator", o => o.UnmannedSuitFabricator },
                { "ClothingFabricator", o => o.UnmannedClothingFabricator },
                { "ClothingAlterationStation", o => o.UnmannedClothingAlterationStation },
                { "CraftingTable", o => o.UnmannedCraftingTable },
                { "AdvancedCraftingTable", o => o.UnmannedAdvancedCraftingTable },
                { "SludgePress", o => o.UnmannedSludgePress },
                { "DiamondPress", o => o.UnmannedDiamondPress },
                { "ChemicalRefinery", o => o.UnmannedChemicalRefinery },
                { "MissileFabricator", o => o.UnmannedMissileFabricator },
                { "DataMiner", o => o.UnmannedDataMiner },
                { "OilRefinery", o => o.UnmannedOilRefinery },
                { "OilWellCap", o => o.UnmannedOilWellCap },
                { "GeoTuner", o => o.UnmannedGeoTuner },
                { "LiquidBottler", o => o.UnmannedLiquidBottler },
                { "GasBottler", o => o.UnmannedGasBottler },
                { "LiquidPumpingStation", o => o.UnmannedLiquidPumpingStation },
                { "RanchStation", o => o.UnmannedGroomingStation },
                { "ShearingStation", o => o.UnmannedShearingStation },
                { "MilkingStation", o => o.UnmannedMilkingStation },
                { "UnderwaterRanchStation", o => o.UnmannedUnderwaterGroomingStation },
                { "UnderwaterShearingStation", o => o.UnmannedUnderwaterShearingStation },
                { "UnderwaterMilkingStation", o => o.UnmannedUnderwaterMilkingStation },
                { "Apothecary", o => o.UnmannedApothecary },
                { "AdvancedApothecary", o => o.UnmannedAdvancedApothecary },
                { "SushiBar", o => o.UnmannedSushiBar },
                { "ManualGenerator", o => o.UnmannedManualGenerator },
                { "Telescope", o => o.UnmannedTelescope },
                { "ClusterTelescope", o => o.UnmannedClusterTelescope },
                { "ClusterTelescopeEnclosed", o => o.UnmannedClusterTelescopeEnclosed },
                { "ManualHighEnergyParticleSpawner", o => o.UnmannedManualHighEnergyParticleSpawner },
                { "ResetSkillsStation", o => o.UnmannedResetSkillsStation },
                { "IceKettle", o => o.UnmannedIceKettle },
                { "Campfire", o => o.UnmannedCampfire },
                { "IceCooledFan", o => o.UnmannedIceCooledFan },
                { "Compost", o => o.UnmannedCompost },
                { "FoodDehydrator", o => o.UnmannedFoodDehydrator },
                { "ResearchCenter", o => o.UnmannedResearchCenter },
                { "AdvancedResearchCenter", o => o.UnmannedAdvancedResearchCenter },
                { "CosmicResearchCenter", o => o.UnmannedCosmicResearchCenter },
                { "DLC1CosmicResearchCenter", o => o.UnmannedDlc1CosmicResearchCenter },
                { "NuclearResearchCenter", o => o.UnmannedNuclearResearchCenter },
                { "OrbitalResearchCenter", o => o.UnmannedOrbitalResearchCenter },
                { "GeneticAnalysisStation", o => o.UnmannedGeneticAnalysisStation },
                { "MorbRoverMaker", o => o.UnmannedMorbRoverMaker },
                { "MissionControl", o => o.UnmannedMissionControl },
                { "MissionControlCluster", o => o.UnmannedMissionControlCluster },
                { "FarmStation", o => o.UnmannedFarmStation },
                { "SpiceGrinder", o => o.UnmannedSpiceGrinder },
                { "GeyserStudy", o => o.UnmannedGeyserStudy },
                { "Desalinator", o => o.UnmannedDesalinator },
                { "MilkFatSeparator", o => o.UnmannedMilkFatSeparator },
                { "FabricatedWoodMaker", o => o.UnmannedFabricatedWoodMaker },
                { "PowerControlStation", o => o.UnmannedPowerControlStation },
                { "SolidTransferArm", o => o.AutoSweeperHarvest },
                { "LiquidReservoir", o => o.LiquidReservoirDuplicantFetch || o.LiquidReservoirAutoSweeperFetch },
                { "Valve", o => o.UnmannedLiquidValve },
                { "LiquidValve", o => o.UnmannedLiquidValve },
                { "GasValve", o => o.UnmannedGasValve }
            };


        /// <summary>
        /// Maps the option key of a room bound building to the toggle that
        /// waives its vanilla room requirement.
        /// </summary>
        private static readonly Dictionary<string, Func<AutoMachineOptions, bool>> RoomOverrideByKey =
            new Dictionary<string, Func<AutoMachineOptions, bool>>(StringComparer.Ordinal)
            {
                { "MISSIONCONTROL", o => o.IgnoreRoomMissionControl },
                { "MISSIONCONTROLCLUSTER", o => o.IgnoreRoomMissionControlCluster },
                { "FARMSTATION", o => o.IgnoreRoomFarmStation },
                { "SPICEGRINDER", o => o.IgnoreRoomSpiceGrinder },
                { "POWERCONTROLSTATION", o => o.IgnoreRoomPowerControlStation },
                { "GEOTUNER", o => o.IgnoreRoomGeoTuner },
                { "GeoTuner", o => o.IgnoreRoomGeoTuner }
            };

        /// <summary>
        /// Maps the option key of a building whose vanilla work needs a skill
        /// perk to the toggle that waives that perk.
        /// </summary>
        private static readonly Dictionary<string, Func<AutoMachineOptions, bool>> SkillOverrideByKey =
            new Dictionary<string, Func<AutoMachineOptions, bool>>(StringComparer.Ordinal)
            {
                { "POWERCONTROLSTATION", o => o.IgnoreSkillPowerControlStation }
            };

        /// <summary>
        /// Returns whether the player allows the given building to be worked
        /// without its vanilla skill perk.
        /// </summary>
        /// <param name="optionKey">Registry option key of the building.</param>
        public static bool IsSkillRequirementIgnored(string optionKey)
        {
            Func<AutoMachineOptions, bool> selector;
            if (string.IsNullOrEmpty(optionKey) || !SkillOverrideByKey.TryGetValue(optionKey, out selector))
            {
                return false;
            }

            return selector(Instance);
        }

        /// <summary>
        /// Returns whether the player allows the given building to run
        /// outside of its vanilla room. Buildings without a room requirement
        /// always report <c>false</c>.
        /// </summary>
        /// <param name="optionKey">Registry option key of the building.</param>
        public static bool IsRoomRequirementIgnored(string optionKey)
        {
            Func<AutoMachineOptions, bool> selector;
            if (string.IsNullOrEmpty(optionKey) || !RoomOverrideByKey.TryGetValue(optionKey, out selector))
            {
                return false;
            }

            return selector(Instance);
        }

        /// <summary>
        /// All prefab identifiers that can be automated through the plain
        /// "no duplicant required" fabricator path.
        /// </summary>
        public static IEnumerable<string> KnownPrefabIds
        {
            get { return ToggleByPrefabId.Keys; }
        }

        /// <summary>
        /// Returns whether automation is enabled for the given building instance and prefab.
        /// If per-building customization is enabled, checks the building's customizer component first.
        /// </summary>
        /// <param name="go">Building game object instance.</param>
        /// <param name="prefabId">Prefab identifier of the building.</param>
        public static bool IsEnabledFor(GameObject go, string prefabId)
        {
            if (go != null && Instance != null && Instance.EnablePerBuildingCustomization)
            {
                var customizer = go.GetComponent<Components.AutoBuildingCustomizer>();
                if (customizer != null)
                {
                    return customizer.IsAutomatedFor(prefabId);
                }
            }

            return IsEnabledFor(prefabId);
        }

        /// <summary>
        /// Returns whether automation is enabled for the given prefab.
        /// Unknown prefabs (for example buildings added by other mods) are
        /// never automated.
        /// </summary>
        /// <param name="prefabId">Prefab identifier of the building.</param>
        public static bool IsEnabledFor(string prefabId)
        {
            Func<AutoMachineOptions, bool> selector;
            if (string.IsNullOrEmpty(prefabId) || !ToggleByPrefabId.TryGetValue(prefabId, out selector))
            {
                return false;
            }

            // The master switch only widens the selection; it never disables
            // a building the player enabled individually.
            return Instance.EnableAllAutomation || selector(Instance);
        }
    }

    /// <summary>
    /// Operating modes for changing building automation states in game.
    /// </summary>
    public enum AutomationToggleMode
    {
        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.TOGGLEMODE_DUPE", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.TOGGLEMODE")]
        DuplicantChore,

        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.TOGGLEMODE_INSTANT", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.TOGGLEMODE")]
        Instant,

        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.TOGGLEMODE_SMART", "STRINGS.AUTOMACHINEREBUILT.TOOLTIP.TOGGLEMODE")]
        SmartHybrid
    }
}
