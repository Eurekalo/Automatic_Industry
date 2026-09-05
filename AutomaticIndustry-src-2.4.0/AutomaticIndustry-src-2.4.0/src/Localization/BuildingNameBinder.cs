// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.RegularExpressions;
using AutoMachineRebuilt.Config;
using AutoMachineRebuilt.Util;

namespace AutoMachineRebuilt.Localization
{
    /// <summary>
    /// Copies vanilla building names into the mod's option titles so the
    /// options screen uses the exact wording of the player's game language,
    /// including for DLC buildings.
    /// </summary>
    internal static class BuildingNameBinder
    {
        /// <summary>Matches the &lt;link&gt; markup used by building names.</summary>
        private static readonly Regex MarkupPattern = new Regex("<[^>]*>", RegexOptions.Compiled);

        /// <summary>Removes rich text markup from a building name.</summary>
        /// <param name="text">Raw localized building name.</param>
        private static string StripMarkup(string text)
        {
            return string.IsNullOrEmpty(text) ? text : MarkupPattern.Replace(text, string.Empty);
        }

        /// <summary>
        /// Option string key (mod side) mapped to the vanilla building
        /// prefab identifier whose name should be displayed.
        /// </summary>
        private static readonly Dictionary<string, string> OptionKeyToPrefabId =
            new Dictionary<string, string>
            {
                { "MANUALGENERATOR", "ManualGenerator" },
                { "TELESCOPE", "Telescope" },
                { "CLUSTERTELESCOPE", "ClusterTelescope" },
                { "CLUSTERTELESCOPEENCLOSED", "ClusterTelescopeEnclosed" },
                { "MANUALHIGHENERGYPARTICLESPAWNER", "ManualHighEnergyParticleSpawner" },
                { "RESETSKILLSSTATION", "ResetSkillsStation" },
                { "APOTHECARY", "Apothecary" },
                { "ADVANCEDAPOTHECARY", "AdvancedApothecary" },
                { "SUSHIBAR", "SushiBar" },
                { "ICEKETTLE", "IceKettle" },
                { "CAMPFIRE", "Campfire" },
                { "ICECOOLEDFAN", "IceCooledFan" },
                { "COMPOST", "Compost" },
                { "FOODDEHYDRATOR", "FoodDehydrator" },
                { "RESEARCHCENTER", "ResearchCenter" },
                { "ADVANCEDRESEARCHCENTER", "AdvancedResearchCenter" },
                { "COSMICRESEARCHCENTER", "CosmicResearchCenter" },
                { "DLC1COSMICRESEARCHCENTER", "DLC1CosmicResearchCenter" },
                { "NUCLEARRESEARCHCENTER", "NuclearResearchCenter" },
                { "ORBITALRESEARCHCENTER", "OrbitalResearchCenter" },
                { "GENETICANALYSISSTATION", "GeneticAnalysisStation" },
                { "MORBROVERMAKER", "MorbRoverMaker" },
                { "MISSIONCONTROL", "MissionControl" },
                { "MISSIONCONTROLCLUSTER", "MissionControlCluster" },
                { "FARMSTATION", "FarmStation" },
                { "SPICEGRINDER", "SpiceGrinder" },
                { "POWERCONTROLSTATION", "PowerControlStation" },
                { "FABRICATEDWOODMAKER", "FabricatedWoodMaker" },
                { "COOKINGSTATION", "CookingStation" },
                { "GOURMETCOOKINGSTATION", "GourmetCookingStation" },
                { "MICROBEMUSHER", "MicrobeMusher" },
                { "DEEPFRYER", "Deepfryer" },
                { "MILKPRESS", "MilkPress" },
                { "SMOKER", "Smoker" },
                { "ROCKCRUSHER", "RockCrusher" },
                { "METALREFINERY", "MetalRefinery" },
                { "GLASSFORGE", "GlassForge" },
                { "SUPERMATERIALREFINERY", "SupermaterialRefinery" },
                { "SUITFABRICATOR", "SuitFabricator" },
                { "CLOTHINGFABRICATOR", "ClothingFabricator" },
                { "CLOTHINGALTERATIONSTATION", "ClothingAlterationStation" },
                { "CRAFTINGTABLE", "CraftingTable" },
                { "ADVANCEDCRAFTINGTABLE", "AdvancedCraftingTable" },
                { "SLUDGEPRESS", "SludgePress" },
                { "DIAMONDPRESS", "DiamondPress" },
                { "CHEMICALREFINERY", "ChemicalRefinery" },
                { "MISSILEFABRICATOR", "MissileFabricator" },
                { "DATAMINER", "DataMiner" },
                { "OILREFINERY", "OilRefinery" },
                { "OILWELLCAP", "OilWellCap" },
                { "GEOTUNER", "GeoTuner" },
                { "DESALINATOR", "Desalinator" },
                { "MILKFATSEPARATOR", "MilkFatSeparator" },
                { "RANCHSTATION", "RanchStation" },
                { "SHEARINGSTATION", "ShearingStation" },
                { "MILKINGSTATION", "MilkingStation" },
                { "UNDERWATERRANCHSTATION", "UnderwaterRanchStation" },
                { "UNDERWATERSHEARINGSTATION", "UnderwaterShearingStation" },
                { "UNDERWATERMILKINGSTATION", "UnderwaterMilkingStation" },
                { "LIQUIDBOTTLER", "LiquidBottler" },
                { "GASBOTTLER", "GasBottler" },
                { "LIQUIDPUMPINGSTATION", "LiquidPumpingStation" }
            };

        /// <summary>
        /// Buildings whose vanilla name is shared with a base game variant.
        /// The suffix keeps the options screen unambiguous.
        /// </summary>
        private static readonly Dictionary<string, string> TitleSuffix =
            new Dictionary<string, string>
            {
                { "CLUSTERTELESCOPE", " (Spaced Out!)" },
                { "DLC1COSMICRESEARCHCENTER", " (Spaced Out!)" },
                { "MISSIONCONTROLCLUSTER", " (Spaced Out!)" }
            };

        /// <summary>
        /// Option keys of the buildings that expose an additional
        /// "ignore room requirement" toggle.
        /// </summary>
        private static readonly string[] RoomBoundKeys =
        {
            "MISSIONCONTROL", "MISSIONCONTROLCLUSTER", "FARMSTATION", "SPICEGRINDER",
            "POWERCONTROLSTATION", "GEOTUNER"
        };

        /// <summary>
        /// Option keys of the buildings that expose an additional
        /// "ignore skill requirement" toggle.
        /// </summary>
        private static readonly string[] SkillBoundKeys =
        {
            "POWERCONTROLSTATION"
        };

        /// <summary>
        /// Applies the vanilla names. Missing entries (a DLC that is not
        /// installed) simply keep the built-in English fallback.
        /// </summary>
        /// <param name="bilingual">
        /// When <c>true</c> the English name is prepended as
        /// "English / localized name".
        /// </param>
        /// <param name="language">
        /// Language selected for the options screen. When a translated name
        /// exists for it that name wins over the game supplied one, so the
        /// options screen stays in the chosen language even when the game
        /// itself runs in another one.
        /// </param>
        internal static void Apply(bool bilingual, UiLanguage language)
        {
            Dictionary<string, string> names = BuildingDescriptions.NamesFor(language);

            // Japanese is the one language the game ships no strings for, so
            // the mod carries a community table for it. When the game itself
            // already runs in Japanese - a Japanese translation mod is loaded -
            // its strings are the authoritative ones and win over the table.
            if (language == UiLanguage.Japanese && OptionTextBinder.GameRunsIn(UiLanguage.Japanese))
            {
                names = null;
            }

            foreach (KeyValuePair<string, string> entry in OptionKeyToPrefabId)
            {
                string vanillaKey = "STRINGS.BUILDINGS.PREFABS." + entry.Value.ToUpperInvariant() + ".NAME";
                string optionKey = "STRINGS.AUTOMACHINEREBUILT.BUILDING." + entry.Key;

                string localized = null;
                if (names != null)
                {
                    string translatedName;
                    if (names.TryGetValue(entry.Key, out translatedName) &&
                        !string.IsNullOrEmpty(translatedName))
                    {
                        localized = translatedName;
                    }
                }

                if (localized == null)
                {
                    StringEntry vanillaName;
                    if (!Strings.TryGet(vanillaKey, out vanillaName))
                    {
                        continue;
                    }

                    // Building names may contain formatting links such as
                    // <link="ROCKCRUSHER">Rock Crusher</link>; strip them so
                    // the options screen shows plain readable text.
                    localized = StripMarkup(vanillaName.String);
                }

                string english;
                if (bilingual && Translations.EnglishBuildingNames.TryGetValue(entry.Key, out english))
                {
                    localized = OptionTextBinder.Combine(english, localized, true, false);
                }

                string suffix;
                if (TitleSuffix.TryGetValue(entry.Key, out suffix))
                {
                    localized += suffix;
                }

                Strings.Add(optionKey, localized);
            }

            ApplyRoomOverrideTitles();
            ApplySkillOverrideTitles();
        }

        /// <summary>
        /// Builds the titles of the "ignore room requirement" toggles from
        /// the localized prefix and the vanilla building name, so those rows
        /// stay short and readable in every language.
        /// </summary>
        private static void ApplyRoomOverrideTitles()
        {
            StringEntry prefix;
            string prefixText = Strings.TryGet("STRINGS.AUTOMACHINEREBUILT.OPTION.IGNOREROOMPREFIX", out prefix)
                ? prefix.String
                : "Ignore room";

            foreach (string key in RoomBoundKeys)
            {
                StringEntry buildingName;
                if (!Strings.TryGet("STRINGS.AUTOMACHINEREBUILT.BUILDING." + key, out buildingName))
                {
                    continue;
                }

                Strings.Add("STRINGS.AUTOMACHINEREBUILT.OPTION.IGNOREROOM." + key,
                    prefixText + ": " + buildingName.String);
            }
        }

        /// <summary>
        /// Builds the titles of the "ignore skill requirement" toggles the
        /// same way the room waiver titles are built.
        /// </summary>
        private static void ApplySkillOverrideTitles()
        {
            StringEntry prefix;
            string prefixText = Strings.TryGet("STRINGS.AUTOMACHINEREBUILT.OPTION.IGNORESKILLPREFIX", out prefix)
                ? prefix.String
                : "Ignore skill";

            foreach (string key in SkillBoundKeys)
            {
                StringEntry buildingName;
                if (!Strings.TryGet("STRINGS.AUTOMACHINEREBUILT.BUILDING." + key, out buildingName))
                {
                    continue;
                }

                Strings.Add("STRINGS.AUTOMACHINEREBUILT.OPTION.IGNORESKILL." + key,
                    prefixText + ": " + buildingName.String);
            }
        }

        /// <summary>Applies the vanilla names without bilingual titles.</summary>
        internal static void Apply()
        {
            Apply(false, UiLanguage.Auto);
        }
    }
}
