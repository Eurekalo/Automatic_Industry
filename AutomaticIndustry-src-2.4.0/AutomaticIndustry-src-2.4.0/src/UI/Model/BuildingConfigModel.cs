// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System;
using System.Collections.Generic;
using AutoMachineRebuilt.Config;
using AutoMachineRebuilt.Localization;
using UnityEngine;

namespace AutoMachineRebuilt.UI.Model
{
    public enum BuildingCategory
    {
        All,
        Fabricators,
        Refining,
        Special,
        Ranching,
        Stations,
        Research,
        Manual
    }

    public class BuildingSubOption
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public string Tooltip { get; set; }
        public Func<bool> Getter { get; set; }
        public Action<bool> Setter { get; set; }
        public bool DefaultValue { get; set; }

        public bool IsEnabled => Getter != null && Getter();
        public void SetEnabled(bool value)
        {
            Setter?.Invoke(value);
            BuildingConfigItem.AutoSave();
        }
        public void ResetToDefault() => SetEnabled(DefaultValue);
    }

    public class BuildingConfigItem
    {
        public string Id { get; set; }
        public BuildingCategory Category { get; set; }
        public string LocalizationKey { get; set; }
        public string DescriptionKey { get; set; }
        public Func<bool> MasterGetter { get; set; }
        public Action<bool> MasterSetter { get; set; }
        public bool MasterDefault { get; set; }
        public List<BuildingSubOption> SubOptions { get; set; } = new List<BuildingSubOption>();

        public bool IsEnabled => MasterGetter != null && MasterGetter();
        public void SetEnabled(bool value)
        {
            MasterSetter?.Invoke(value);
            AutoSave();
        }
        public bool HasSubOptions => SubOptions != null && SubOptions.Count > 0;

        public static void AutoSave()
        {
            try
            {
                if (AutoMachineOptions.Instance != null)
                {
                    PeterHan.PLib.Options.POptions.WriteSettings(AutoMachineOptions.Instance);
                }
            }
            catch (Exception ex)
            {
                Util.Log.Warn("AutoSave failed: " + ex.Message);
            }
        }

        public void ResetToDefault()
        {
            SetEnabled(MasterDefault);
            if (SubOptions != null)
            {
                foreach (BuildingSubOption sub in SubOptions)
                {
                    sub.ResetToDefault();
                }
            }
            AutoSave();
        }

        private static readonly Dictionary<string, string> PreferredSpritePrefabIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Valve", "LiquidValve" }
        };

        private static readonly Dictionary<string, string> SpriteFallbackPrefabIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "CosmicResearchCenter", "DLC1CosmicResearchCenter" },
            { "DLC1CosmicResearchCenter", "CosmicResearchCenter" },
            { "Telescope", "ClusterTelescopeEnclosed" },
            { "ClusterTelescope", "Telescope" },
            { "ClusterTelescopeEnclosed", "Telescope" },
            { "MissionControl", "MissionControlCluster" },
            { "MissionControlCluster", "MissionControl" },
            { "GeyserStudy", "GeyserGeneric" },
            { "Valve", "LiquidValve" },
            { "LiquidValve", "Valve" }
        };

        public static Tuple<Sprite, Color> GetBuildingSprite(string buildingId)
        {
            if (string.IsNullOrEmpty(buildingId)) return null;

            // Prefer mapped building prefab ID (e.g. "Valve" -> "LiquidValve")
            // because vanilla ONI defines an internal conduit port sprite named "Valve"
            // which returns an irrelevant solid purple circle instead of the Liquid Valve building machinery icon.
            if (PreferredSpritePrefabIds.TryGetValue(buildingId, out string preferredId))
            {
                try
                {
                    Tuple<Sprite, Color> preferredSprite = Def.GetUISprite(preferredId, "ui", false);
                    if (preferredSprite != null && preferredSprite.first != null && preferredSprite.first.name != "unknown")
                    {
                        return preferredSprite;
                    }
                }
                catch { }
            }

            try
            {
                Tuple<Sprite, Color> uisprite = Def.GetUISprite(buildingId, "ui", false);
                if (uisprite != null && uisprite.first != null && uisprite.first.name != "unknown")
                {
                    return uisprite;
                }
            }
            catch { }

            if (SpriteFallbackPrefabIds.TryGetValue(buildingId, out string fallbackId))
            {
                try
                {
                    Tuple<Sprite, Color> fallbackSprite = Def.GetUISprite(fallbackId, "ui", false);
                    if (fallbackSprite != null && fallbackSprite.first != null && fallbackSprite.first.name != "unknown")
                    {
                        return fallbackSprite;
                    }
                }
                catch { }
            }

            return null;
        }

        public string GetDisplayName()
        {
            UiLanguage lang = AutoMachineOptions.Instance != null ? AutoMachineOptions.Instance.OptionsLanguage : UiLanguage.Auto;
            UiLanguage resolved = OptionTextBinder.Resolve(lang);
            bool bilingual = AutoMachineOptions.Instance != null && AutoMachineOptions.Instance.BilingualLabels;

            string localized = null;

            // 1. Check custom translations first (e.g. GeyserStudy, LiquidReservoir, etc.)
            if (!string.IsNullOrEmpty(LocalizationKey))
            {
                string trans = Translations.Get(LocalizationKey, resolved);
                if (!string.IsNullOrEmpty(trans) && trans != LocalizationKey)
                {
                    localized = trans;
                }
            }

            // 2. Check localized building names table
            if (string.IsNullOrEmpty(localized))
            {
                Dictionary<string, string> names = BuildingDescriptions.NamesFor(resolved);
                if (names != null)
                {
                    string upperId = Id.ToUpperInvariant();
                    if (names.TryGetValue(upperId, out string translatedName) && !string.IsNullOrEmpty(translatedName))
                    {
                        localized = translatedName;
                    }
                }
            }

            // 3. Fallback to game strings if resolved language matches game language
            if (string.IsNullOrEmpty(localized))
            {
                string stringKey = "STRINGS.BUILDINGS.PREFABS." + Id.ToUpperInvariant() + ".NAME";
                if (Strings.TryGet(new StringKey(stringKey), out StringEntry entry) && !string.IsNullOrEmpty(entry.String))
                {
                    localized = StripLink(entry.String);
                }
            }

            if (string.IsNullOrEmpty(localized))
            {
                localized = Id;
            }

            // 4. Bilingual formatting if requested
            if (bilingual && resolved != UiLanguage.English)
            {
                string english = null;
                if (!string.IsNullOrEmpty(LocalizationKey))
                {
                    english = Translations.Get(LocalizationKey, UiLanguage.English);
                }
                if (string.IsNullOrEmpty(english) || english == LocalizationKey)
                {
                    VanillaBuildingNames.English.TryGetValue(Id.ToUpperInvariant(), out english);
                }
                if (!string.IsNullOrEmpty(english) && !string.Equals(english, localized, StringComparison.OrdinalIgnoreCase))
                {
                    localized = OptionTextBinder.Combine(english, localized, true, false);
                }
            }

            // 5. DLC vs Base Game Suffix
            string suffix = GetDlcSuffix(Id, resolved);
            if (!string.IsNullOrEmpty(suffix))
            {
                localized += suffix;
            }

            return localized;
        }

        private static string GetDlcSuffix(string buildingId, UiLanguage lang)
        {
            bool isDlc = buildingId.Equals("ClusterTelescope", StringComparison.OrdinalIgnoreCase)
                      || buildingId.Equals("ClusterTelescopeEnclosed", StringComparison.OrdinalIgnoreCase)
                      || buildingId.Equals("MissionControlCluster", StringComparison.OrdinalIgnoreCase);

            bool isBase = buildingId.Equals("Telescope", StringComparison.OrdinalIgnoreCase)
                       || buildingId.Equals("MissionControl", StringComparison.OrdinalIgnoreCase);

            if (isDlc)
            {
                switch (lang)
                {
                    case UiLanguage.ChineseSimplified: return "（眼冒金星!）";
                    case UiLanguage.ChineseTraditional: return "（眼冒金星!）";
                    case UiLanguage.Korean: return " (스페이스 아웃!)";
                    case UiLanguage.Japanese: return " (スペース・アウト!)";
                    default: return " (Spaced Out!)";
                }
            }

            if (isBase)
            {
                switch (lang)
                {
                    case UiLanguage.ChineseSimplified: return "（原版）";
                    case UiLanguage.ChineseTraditional: return "（原版）";
                    case UiLanguage.Korean: return " (오리지널)";
                    case UiLanguage.Japanese: return " (バニラ)";
                    default: return " (Base Game)";
                }
            }

            return null;
        }

        public string GetDescription()
        {
            if (!string.IsNullOrEmpty(DescriptionKey))
            {
                UiLanguage lang = AutoMachineOptions.Instance != null ? AutoMachineOptions.Instance.OptionsLanguage : UiLanguage.Auto;
                UiLanguage resolved = OptionTextBinder.Resolve(lang);
                var dict = BuildingDescriptions.DescriptionsFor(resolved);
                if (dict != null && dict.TryGetValue(DescriptionKey, out string desc) && !string.IsNullOrEmpty(desc))
                {
                    return desc;
                }
                if (BuildingDescriptions.DescriptionsEnglish.TryGetValue(DescriptionKey, out string fallbackDesc))
                {
                    return fallbackDesc;
                }
            }
            return "";
        }

        private static string StripLink(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            int start = text.IndexOf("<link=", StringComparison.OrdinalIgnoreCase);
            if (start >= 0)
            {
                int closeTag = text.IndexOf('>', start);
                if (closeTag >= 0)
                {
                    text = text.Substring(closeTag + 1);
                }
            }
            int endLink = text.IndexOf("</link>", StringComparison.OrdinalIgnoreCase);
            if (endLink >= 0)
            {
                text = text.Substring(0, endLink);
            }
            return text.Trim();
        }
    }

    public static class BuildingConfigRegistry
    {
        private static List<BuildingConfigItem> cachedItems;
        private static readonly AutoMachineOptions fallbackOptions = new AutoMachineOptions();
        private static AutoMachineOptions opt => AutoMachineOptions.Instance ?? fallbackOptions;

        public static void InvalidateCache()
        {
            cachedItems = null;
        }

        public static List<BuildingConfigItem> GetAllItems(bool forceRefresh = false)
        {
            if (cachedItems != null && !forceRefresh) return cachedItems;

            UiLanguage lang = opt.OptionsLanguage;
            List<BuildingConfigItem> list = new List<BuildingConfigItem>();

            // ==========================================
            // 1. Food & Cooking Fabricators
            // ==========================================
            list.Add(new BuildingConfigItem
            {
                Id = "CookingStation",
                Category = BuildingCategory.Fabricators,
                LocalizationKey = "BUILDING.COOKINGSTATION",
                DescriptionKey = "BUILDINGDESC.COOKINGSTATION",
                MasterGetter = () => opt.UnmannedCookingStation,
                MasterSetter = v => opt.UnmannedCookingStation = v,
                MasterDefault = false
            });

            list.Add(new BuildingConfigItem
            {
                Id = "GourmetCookingStation",
                Category = BuildingCategory.Fabricators,
                LocalizationKey = "BUILDING.GOURMETCOOKINGSTATION",
                DescriptionKey = "BUILDINGDESC.GOURMETCOOKINGSTATION",
                MasterGetter = () => opt.UnmannedGourmetCookingStation,
                MasterSetter = v => opt.UnmannedGourmetCookingStation = v,
                MasterDefault = false
            });

            list.Add(new BuildingConfigItem
            {
                Id = "MicrobeMusher",
                Category = BuildingCategory.Fabricators,
                LocalizationKey = "BUILDING.MICROBEMUSHER",
                DescriptionKey = "BUILDINGDESC.MICROBEMUSHER",
                MasterGetter = () => opt.UnmannedMicrobeMusher,
                MasterSetter = v => opt.UnmannedMicrobeMusher = v,
                MasterDefault = false
            });

            list.Add(new BuildingConfigItem
            {
                Id = "Deepfryer",
                Category = BuildingCategory.Fabricators,
                LocalizationKey = "BUILDING.DEEPFRYER",
                DescriptionKey = "BUILDINGDESC.DEEPFRYER",
                MasterGetter = () => opt.UnmannedDeepfryer,
                MasterSetter = v => opt.UnmannedDeepfryer = v,
                MasterDefault = false
            });

            list.Add(new BuildingConfigItem
            {
                Id = "Smoker",
                Category = BuildingCategory.Fabricators,
                LocalizationKey = "BUILDING.SMOKER",
                DescriptionKey = "BUILDINGDESC.SMOKER",
                MasterGetter = () => opt.UnmannedSmoker,
                MasterSetter = v => opt.UnmannedSmoker = v,
                MasterDefault = false
            });

            list.Add(new BuildingConfigItem
            {
                Id = "SushiBar",
                Category = BuildingCategory.Fabricators,
                LocalizationKey = "BUILDING.SUSHIBAR",
                DescriptionKey = "BUILDINGDESC.SUSHIBAR",
                MasterGetter = () => opt.UnmannedSushiBar,
                MasterSetter = v => opt.UnmannedSushiBar = v,
                MasterDefault = false
            });

            list.Add(new BuildingConfigItem
            {
                Id = "MilkPress",
                Category = BuildingCategory.Fabricators,
                LocalizationKey = "BUILDING.MILKPRESS",
                DescriptionKey = "BUILDINGDESC.MILKPRESS",
                MasterGetter = () => opt.UnmannedMilkPress,
                MasterSetter = v => opt.UnmannedMilkPress = v,
                MasterDefault = false
            });

            list.Add(new BuildingConfigItem
            {
                Id = "FoodDehydrator",
                Category = BuildingCategory.Fabricators,
                LocalizationKey = "BUILDING.FOODDEHYDRATOR",
                DescriptionKey = "BUILDINGDESC.FOODDEHYDRATOR",
                MasterGetter = () => opt.UnmannedFoodDehydrator,
                MasterSetter = v => opt.UnmannedFoodDehydrator = v,
                MasterDefault = false
            });

            // ==========================================
            // 2. Refining & Manufacturing
            // ==========================================
            list.Add(new BuildingConfigItem
            {
                Id = "RockCrusher",
                Category = BuildingCategory.Refining,
                LocalizationKey = "BUILDING.ROCKCRUSHER",
                DescriptionKey = "BUILDINGDESC.ROCKCRUSHER",
                MasterGetter = () => opt.UnmannedRockCrusher,
                MasterSetter = v => opt.UnmannedRockCrusher = v,
                MasterDefault = false
            });

            list.Add(new BuildingConfigItem
            {
                Id = "MetalRefinery",
                Category = BuildingCategory.Refining,
                LocalizationKey = "BUILDING.METALREFINERY",
                DescriptionKey = "BUILDINGDESC.METALREFINERY",
                MasterGetter = () => opt.UnmannedMetalRefinery,
                MasterSetter = v => opt.UnmannedMetalRefinery = v,
                MasterDefault = false
            });

            list.Add(new BuildingConfigItem
            {
                Id = "GlassForge",
                Category = BuildingCategory.Refining,
                LocalizationKey = "BUILDING.GLASSFORGE",
                DescriptionKey = "BUILDINGDESC.GLASSFORGE",
                MasterGetter = () => opt.UnmannedGlassForge,
                MasterSetter = v => opt.UnmannedGlassForge = v,
                MasterDefault = false
            });

            list.Add(new BuildingConfigItem
            {
                Id = "DiamondPress",
                Category = BuildingCategory.Refining,
                LocalizationKey = "BUILDING.DIAMONDPRESS",
                DescriptionKey = "BUILDINGDESC.DIAMONDPRESS",
                MasterGetter = () => opt.UnmannedDiamondPress,
                MasterSetter = v => opt.UnmannedDiamondPress = v,
                MasterDefault = false
            });

            list.Add(new BuildingConfigItem
            {
                Id = "SludgePress",
                Category = BuildingCategory.Refining,
                LocalizationKey = "BUILDING.SLUDGEPRESS",
                DescriptionKey = "BUILDINGDESC.SLUDGEPRESS",
                MasterGetter = () => opt.UnmannedSludgePress,
                MasterSetter = v => opt.UnmannedSludgePress = v,
                MasterDefault = false
            });

            list.Add(new BuildingConfigItem
            {
                Id = "ChemicalRefinery",
                Category = BuildingCategory.Refining,
                LocalizationKey = "BUILDING.CHEMICALREFINERY",
                DescriptionKey = "BUILDINGDESC.CHEMICALREFINERY",
                MasterGetter = () => opt.UnmannedChemicalRefinery,
                MasterSetter = v => opt.UnmannedChemicalRefinery = v,
                MasterDefault = false
            });

            list.Add(new BuildingConfigItem
            {
                Id = "SupermaterialRefinery",
                Category = BuildingCategory.Refining,
                LocalizationKey = "BUILDING.SUPERMATERIALREFINERY",
                DescriptionKey = "BUILDINGDESC.SUPERMATERIALREFINERY",
                MasterGetter = () => opt.UnmannedSupermaterialRefinery,
                MasterSetter = v => opt.UnmannedSupermaterialRefinery = v,
                MasterDefault = false
            });

            list.Add(new BuildingConfigItem
            {
                Id = "CraftingTable",
                Category = BuildingCategory.Refining,
                LocalizationKey = "BUILDING.CRAFTINGTABLE",
                DescriptionKey = "BUILDINGDESC.CRAFTINGTABLE",
                MasterGetter = () => opt.UnmannedCraftingTable,
                MasterSetter = v => opt.UnmannedCraftingTable = v,
                MasterDefault = false
            });

            list.Add(new BuildingConfigItem
            {
                Id = "AdvancedCraftingTable",
                Category = BuildingCategory.Refining,
                LocalizationKey = "BUILDING.ADVANCEDCRAFTINGTABLE",
                DescriptionKey = "BUILDINGDESC.ADVANCEDCRAFTINGTABLE",
                MasterGetter = () => opt.UnmannedAdvancedCraftingTable,
                MasterSetter = v => opt.UnmannedAdvancedCraftingTable = v,
                MasterDefault = false
            });

            list.Add(new BuildingConfigItem
            {
                Id = "ClothingFabricator",
                Category = BuildingCategory.Refining,
                LocalizationKey = "BUILDING.CLOTHINGFABRICATOR",
                DescriptionKey = "BUILDINGDESC.CLOTHINGFABRICATOR",
                MasterGetter = () => opt.UnmannedClothingFabricator,
                MasterSetter = v => opt.UnmannedClothingFabricator = v,
                MasterDefault = false
            });

            list.Add(new BuildingConfigItem
            {
                Id = "ClothingAlterationStation",
                Category = BuildingCategory.Refining,
                LocalizationKey = "BUILDING.CLOTHINGALTERATIONSTATION",
                DescriptionKey = "BUILDINGDESC.CLOTHINGALTERATIONSTATION",
                MasterGetter = () => opt.UnmannedClothingAlterationStation,
                MasterSetter = v => opt.UnmannedClothingAlterationStation = v,
                MasterDefault = false
            });

            list.Add(new BuildingConfigItem
            {
                Id = "SuitFabricator",
                Category = BuildingCategory.Refining,
                LocalizationKey = "BUILDING.SUITFABRICATOR",
                DescriptionKey = "BUILDINGDESC.SUITFABRICATOR",
                MasterGetter = () => opt.UnmannedSuitFabricator,
                MasterSetter = v => opt.UnmannedSuitFabricator = v,
                MasterDefault = false
            });

            list.Add(new BuildingConfigItem
            {
                Id = "MissileFabricator",
                Category = BuildingCategory.Refining,
                LocalizationKey = "BUILDING.MISSILEFABRICATOR",
                DescriptionKey = "BUILDINGDESC.MISSILEFABRICATOR",
                MasterGetter = () => opt.UnmannedMissileFabricator,
                MasterSetter = v => opt.UnmannedMissileFabricator = v,
                MasterDefault = false
            });

            list.Add(new BuildingConfigItem
            {
                Id = "DataMiner",
                Category = BuildingCategory.Refining,
                LocalizationKey = "BUILDING.DATAMINER",
                DescriptionKey = "BUILDINGDESC.DATAMINER",
                MasterGetter = () => opt.UnmannedDataMiner,
                MasterSetter = v => opt.UnmannedDataMiner = v,
                MasterDefault = false
            });

            list.Add(new BuildingConfigItem
            {
                Id = "Apothecary",
                Category = BuildingCategory.Refining,
                LocalizationKey = "BUILDING.APOTHECARY",
                DescriptionKey = "BUILDINGDESC.APOTHECARY",
                MasterGetter = () => opt.UnmannedApothecary,
                MasterSetter = v => opt.UnmannedApothecary = v,
                MasterDefault = false
            });

            list.Add(new BuildingConfigItem
            {
                Id = "AdvancedApothecary",
                Category = BuildingCategory.Refining,
                LocalizationKey = "BUILDING.ADVANCEDAPOTHECARY",
                DescriptionKey = "BUILDINGDESC.ADVANCEDAPOTHECARY",
                MasterGetter = () => opt.UnmannedAdvancedApothecary,
                MasterSetter = v => opt.UnmannedAdvancedApothecary = v,
                MasterDefault = false
            });

            list.Add(new BuildingConfigItem
            {
                Id = "FabricatedWoodMaker",
                Category = BuildingCategory.Refining,
                LocalizationKey = "BUILDING.FABRICATEDWOODMAKER",
                DescriptionKey = "BUILDINGDESC.FABRICATEDWOODMAKER",
                MasterGetter = () => opt.UnmannedFabricatedWoodMaker,
                MasterSetter = v => opt.UnmannedFabricatedWoodMaker = v,
                MasterDefault = false
            });

            // ==========================================
            // 3. Special & Custom Machinery
            // ==========================================
            BuildingConfigItem oilRefinery = new BuildingConfigItem
            {
                Id = "OilRefinery",
                Category = BuildingCategory.Special,
                LocalizationKey = "BUILDING.OILREFINERY",
                DescriptionKey = "BUILDINGDESC.OILREFINERY",
                MasterGetter = () => opt.UnmannedOilRefinery,
                MasterSetter = v => opt.UnmannedOilRefinery = v,
                MasterDefault = false
            };
            oilRefinery.SubOptions.Add(new BuildingSubOption
            {
                Id = "OilRefineryEfficiency",
                Label = Translations.Get("OPTION.OILREFINERYEFFICIENCY", lang),
                Tooltip = Translations.Get("TOOLTIP.OILREFINERYEFFICIENCY", lang),
                Getter = () => opt.OilRefineryEfficiency == OilRefineryEfficiency.Full100,
                Setter = v => opt.OilRefineryEfficiency = v ? OilRefineryEfficiency.Full100 : OilRefineryEfficiency.Vanilla50,
                DefaultValue = false
            });
            list.Add(oilRefinery);

            list.Add(new BuildingConfigItem
            {
                Id = "OilWellCap",
                Category = BuildingCategory.Special,
                LocalizationKey = "BUILDING.OILWELLCAP",
                DescriptionKey = "BUILDINGDESC.OILWELLCAP",
                MasterGetter = () => opt.UnmannedOilWellCap,
                MasterSetter = v => opt.UnmannedOilWellCap = v,
                MasterDefault = false
            });

            // --- Valve Split ---
            list.Add(new BuildingConfigItem
            {
                Id = "Valve",
                Category = BuildingCategory.Special,
                LocalizationKey = "BUILDING.LIQUIDVALVE",
                DescriptionKey = "BUILDINGDESC.LIQUIDVALVE",
                MasterGetter = () => opt.UnmannedLiquidValve,
                MasterSetter = v => opt.UnmannedLiquidValve = v,
                MasterDefault = false
            });

            list.Add(new BuildingConfigItem
            {
                Id = "GasValve",
                Category = BuildingCategory.Special,
                LocalizationKey = "BUILDING.GASVALVE",
                DescriptionKey = "BUILDINGDESC.GASVALVE",
                MasterGetter = () => opt.UnmannedGasValve,
                MasterSetter = v => opt.UnmannedGasValve = v,
                MasterDefault = false
            });






            list.Add(new BuildingConfigItem
            {
                Id = "Desalinator",
                Category = BuildingCategory.Special,
                LocalizationKey = "BUILDING.DESALINATOR",
                DescriptionKey = "BUILDINGDESC.DESALINATOR",
                MasterGetter = () => opt.UnmannedDesalinator,
                MasterSetter = v => opt.UnmannedDesalinator = v,
                MasterDefault = false
            });

            BuildingConfigItem milkFatSeparator = new BuildingConfigItem
            {
                Id = "MilkFatSeparator",
                Category = BuildingCategory.Special,
                LocalizationKey = "BUILDING.MILKFATSEPARATOR",
                DescriptionKey = "BUILDINGDESC.MILKFATSEPARATOR",
                MasterGetter = () => opt.UnmannedMilkFatSeparator,
                MasterSetter = v => opt.UnmannedMilkFatSeparator = v,
                MasterDefault = false
            };
            milkFatSeparator.SubOptions.Add(new BuildingSubOption
            {
                Id = "ProgressBarGleaner",
                Label = Translations.Get("OPTION.PROGRESSBAR.GLEANER", lang),
                Tooltip = Translations.Get("TOOLTIP.PROGRESSBAR.GLEANER", lang),
                Getter = () => opt.ProgressBarGleaner,
                Setter = v => opt.ProgressBarGleaner = v,
                DefaultValue = true
            });
            list.Add(milkFatSeparator);

            // ==========================================
            // 4. Critter Ranching
            // ==========================================
            list.Add(new BuildingConfigItem
            {
                Id = "RanchStation",
                Category = BuildingCategory.Ranching,
                LocalizationKey = "BUILDING.RANCHSTATION",
                DescriptionKey = "BUILDINGDESC.RANCHSTATION",
                MasterGetter = () => opt.UnmannedGroomingStation,
                MasterSetter = v => opt.UnmannedGroomingStation = v,
                MasterDefault = false
            });

            list.Add(new BuildingConfigItem
            {
                Id = "ShearingStation",
                Category = BuildingCategory.Ranching,
                LocalizationKey = "BUILDING.SHEARINGSTATION",
                DescriptionKey = "BUILDINGDESC.SHEARINGSTATION",
                MasterGetter = () => opt.UnmannedShearingStation,
                MasterSetter = v => opt.UnmannedShearingStation = v,
                MasterDefault = false
            });

            list.Add(new BuildingConfigItem
            {
                Id = "MilkingStation",
                Category = BuildingCategory.Ranching,
                LocalizationKey = "BUILDING.MILKINGSTATION",
                DescriptionKey = "BUILDINGDESC.MILKINGSTATION",
                MasterGetter = () => opt.UnmannedMilkingStation,
                MasterSetter = v => opt.UnmannedMilkingStation = v,
                MasterDefault = false
            });

            list.Add(new BuildingConfigItem
            {
                Id = "UnderwaterRanchStation",
                Category = BuildingCategory.Ranching,
                LocalizationKey = "BUILDING.UNDERWATERRANCHSTATION",
                DescriptionKey = "BUILDINGDESC.UNDERWATERRANCHSTATION",
                MasterGetter = () => opt.UnmannedUnderwaterGroomingStation,
                MasterSetter = v => opt.UnmannedUnderwaterGroomingStation = v,
                MasterDefault = false
            });

            list.Add(new BuildingConfigItem
            {
                Id = "UnderwaterShearingStation",
                Category = BuildingCategory.Ranching,
                LocalizationKey = "BUILDING.UNDERWATERSHEARINGSTATION",
                DescriptionKey = "BUILDINGDESC.UNDERWATERSHEARINGSTATION",
                MasterGetter = () => opt.UnmannedUnderwaterShearingStation,
                MasterSetter = v => opt.UnmannedUnderwaterShearingStation = v,
                MasterDefault = false
            });

            list.Add(new BuildingConfigItem
            {
                Id = "UnderwaterMilkingStation",
                Category = BuildingCategory.Ranching,
                LocalizationKey = "BUILDING.UNDERWATERMILKINGSTATION",
                DescriptionKey = "BUILDINGDESC.UNDERWATERMILKINGSTATION",
                MasterGetter = () => opt.UnmannedUnderwaterMilkingStation,
                MasterSetter = v => opt.UnmannedUnderwaterMilkingStation = v,
                MasterDefault = false
            });

            // ==========================================
            // 5. Workstations & Room-Bound Buildings
            // ==========================================
            BuildingConfigItem powerControl = new BuildingConfigItem
            {
                Id = "PowerControlStation",
                Category = BuildingCategory.Stations,
                LocalizationKey = "BUILDING.POWERCONTROLSTATION",
                DescriptionKey = "BUILDINGDESC.POWERCONTROLSTATION",
                MasterGetter = () => opt.UnmannedPowerControlStation,
                MasterSetter = v => opt.UnmannedPowerControlStation = v,
                MasterDefault = false
            };
            powerControl.SubOptions.Add(new BuildingSubOption
            {
                Id = "IgnoreSkillPowerControlStation",
                Label = Translations.Get("OPTION.IGNORESKILL", lang),
                Tooltip = Translations.Get("TOOLTIP.IGNORESKILL", lang),
                Getter = () => opt.IgnoreSkillPowerControlStation,
                Setter = v => opt.IgnoreSkillPowerControlStation = v,
                DefaultValue = false
            });
            powerControl.SubOptions.Add(new BuildingSubOption
            {
                Id = "IgnoreRoomPowerControlStation",
                Label = Translations.Get("OPTION.IGNOREROOM", lang),
                Tooltip = Translations.Get("TOOLTIP.IGNOREROOM", lang),
                Getter = () => opt.IgnoreRoomPowerControlStation,
                Setter = v => opt.IgnoreRoomPowerControlStation = v,
                DefaultValue = false
            });
            powerControl.SubOptions.Add(new BuildingSubOption
            {
                Id = "IgnorePowerDemandPowerControlStation",
                Label = Translations.Get("OPTION.IGNOREDEMAND.POWERCONTROLSTATION", lang),
                Tooltip = Translations.Get("TOOLTIP.IGNOREDEMAND.POWERCONTROLSTATION", lang),
                Getter = () => opt.IgnorePowerDemandPowerControlStation,
                Setter = v => opt.IgnorePowerDemandPowerControlStation = v,
                DefaultValue = false
            });
            powerControl.SubOptions.Add(new BuildingSubOption
            {
                Id = "ProgressBarPowerControlStation",
                Label = Translations.Get("OPTION.PROGRESSBAR.POWERCONTROLSTATION", lang),
                Tooltip = Translations.Get("TOOLTIP.PROGRESSBAR.POWERCONTROLSTATION", lang),
                Getter = () => opt.ProgressBarPowerControlStation,
                Setter = v => opt.ProgressBarPowerControlStation = v,
                DefaultValue = true
            });
            list.Add(powerControl);

            BuildingConfigItem farmStation = new BuildingConfigItem
            {
                Id = "FarmStation",
                Category = BuildingCategory.Stations,
                LocalizationKey = "BUILDING.FARMSTATION",
                DescriptionKey = "BUILDINGDESC.FARMSTATION",
                MasterGetter = () => opt.UnmannedFarmStation,
                MasterSetter = v => opt.UnmannedFarmStation = v,
                MasterDefault = false
            };
            farmStation.SubOptions.Add(new BuildingSubOption
            {
                Id = "IgnoreRoomFarmStation",
                Label = Translations.Get("OPTION.IGNOREROOM", lang),
                Tooltip = Translations.Get("TOOLTIP.IGNOREROOM", lang),
                Getter = () => opt.IgnoreRoomFarmStation,
                Setter = v => opt.IgnoreRoomFarmStation = v,
                DefaultValue = false
            });
            farmStation.SubOptions.Add(new BuildingSubOption
            {
                Id = "IgnoreCropDemandFarmStation",
                Label = Translations.Get("OPTION.IGNORECROPDEMAND", lang),
                Tooltip = Translations.Get("TOOLTIP.IGNORECROPDEMAND", lang),
                Getter = () => opt.IgnoreCropDemandFarmStation,
                Setter = v => opt.IgnoreCropDemandFarmStation = v,
                DefaultValue = false
            });
            list.Add(farmStation);

            BuildingConfigItem missionControl = new BuildingConfigItem
            {
                Id = "MissionControl",
                Category = BuildingCategory.Stations,
                LocalizationKey = "BUILDING.MISSIONCONTROL",
                DescriptionKey = "BUILDINGDESC.MISSIONCONTROL",
                MasterGetter = () => opt.UnmannedMissionControl,
                MasterSetter = v => opt.UnmannedMissionControl = v,
                MasterDefault = false
            };
            missionControl.SubOptions.Add(new BuildingSubOption
            {
                Id = "IgnoreRoomMissionControl",
                Label = Translations.Get("OPTION.IGNOREROOM", lang),
                Tooltip = Translations.Get("TOOLTIP.IGNOREROOM", lang),
                Getter = () => opt.IgnoreRoomMissionControl,
                Setter = v => opt.IgnoreRoomMissionControl = v,
                DefaultValue = false
            });
            list.Add(missionControl);

            BuildingConfigItem missionControlCluster = new BuildingConfigItem
            {
                Id = "MissionControlCluster",
                Category = BuildingCategory.Stations,
                LocalizationKey = "BUILDING.MISSIONCONTROLCLUSTER",
                DescriptionKey = "BUILDINGDESC.MISSIONCONTROLCLUSTER",
                MasterGetter = () => opt.UnmannedMissionControlCluster,
                MasterSetter = v => opt.UnmannedMissionControlCluster = v,
                MasterDefault = false
            };
            missionControlCluster.SubOptions.Add(new BuildingSubOption
            {
                Id = "IgnoreRoomMissionControlCluster",
                Label = Translations.Get("OPTION.IGNOREROOM", lang),
                Tooltip = Translations.Get("TOOLTIP.IGNOREROOM", lang),
                Getter = () => opt.IgnoreRoomMissionControlCluster,
                Setter = v => opt.IgnoreRoomMissionControlCluster = v,
                DefaultValue = false
            });
            list.Add(missionControlCluster);

            BuildingConfigItem spiceGrinder = new BuildingConfigItem
            {
                Id = "SpiceGrinder",
                Category = BuildingCategory.Stations,
                LocalizationKey = "BUILDING.SPICEGRINDER",
                DescriptionKey = "BUILDINGDESC.SPICEGRINDER",
                MasterGetter = () => opt.UnmannedSpiceGrinder,
                MasterSetter = v => opt.UnmannedSpiceGrinder = v,
                MasterDefault = false
            };
            spiceGrinder.SubOptions.Add(new BuildingSubOption
            {
                Id = "IgnoreRoomSpiceGrinder",
                Label = Translations.Get("OPTION.IGNOREROOM", lang),
                Tooltip = Translations.Get("TOOLTIP.IGNOREROOM", lang),
                Getter = () => opt.IgnoreRoomSpiceGrinder,
                Setter = v => opt.IgnoreRoomSpiceGrinder = v,
                DefaultValue = true
            });
            spiceGrinder.SubOptions.Add(new BuildingSubOption
            {
                Id = "ProgressBarSpiceGrinder",
                Label = Translations.Get("OPTION.PROGRESSBAR.SPICEGRINDER", lang),
                Tooltip = Translations.Get("TOOLTIP.PROGRESSBAR.SPICEGRINDER", lang),
                Getter = () => opt.ProgressBarSpiceGrinder,
                Setter = v => opt.ProgressBarSpiceGrinder = v,
                DefaultValue = true
            });
            list.Add(spiceGrinder);

            BuildingConfigItem geoTuner = new BuildingConfigItem
            {
                Id = "GeoTuner",
                Category = BuildingCategory.Stations,
                LocalizationKey = "BUILDING.GEOTUNER",
                DescriptionKey = "BUILDINGDESC.GEOTUNER",
                MasterGetter = () => opt.UnmannedGeoTuner,
                MasterSetter = v => opt.UnmannedGeoTuner = v,
                MasterDefault = false
            };
            geoTuner.SubOptions.Add(new BuildingSubOption
            {
                Id = "IgnoreRoomGeoTuner",
                Label = Translations.Get("OPTION.IGNOREROOM", lang),
                Tooltip = Translations.Get("TOOLTIP.IGNOREROOM", lang),
                Getter = () => opt.IgnoreRoomGeoTuner,
                Setter = v => opt.IgnoreRoomGeoTuner = v,
                DefaultValue = true
            });
            geoTuner.SubOptions.Add(new BuildingSubOption
            {
                Id = "ProgressBarGeoTuner",
                Label = Translations.Get("OPTION.PROGRESSBAR.GEOTUNER", lang),
                Tooltip = Translations.Get("TOOLTIP.PROGRESSBAR.GEOTUNER", lang),
                Getter = () => opt.ProgressBarGeoTuner,
                Setter = v => opt.ProgressBarGeoTuner = v,
                DefaultValue = true
            });
            geoTuner.SubOptions.Add(new BuildingSubOption
            {
                Id = "ProgressBarGeyserTuning",
                Label = Translations.Get("OPTION.PROGRESSBAR.GEYSERTUNING", lang),
                Tooltip = Translations.Get("TOOLTIP.PROGRESSBAR.GEYSERTUNING", lang),
                Getter = () => opt.ProgressBarGeyserTuning,
                Setter = v => opt.ProgressBarGeyserTuning = v,
                DefaultValue = true
            });
            list.Add(geoTuner);

            // ==========================================
            // 6. Research & Exploration
            // ==========================================
            BuildingConfigItem resBasic = new BuildingConfigItem
            {
                Id = "ResearchCenter",
                Category = BuildingCategory.Research,
                LocalizationKey = "BUILDING.RESEARCHCENTER",
                DescriptionKey = "BUILDINGDESC.RESEARCHCENTER",
                MasterGetter = () => opt.UnmannedResearchCenter,
                MasterSetter = v => opt.UnmannedResearchCenter = v,
                MasterDefault = false
            };
            resBasic.SubOptions.Add(new BuildingSubOption
            {
                Id = "SweeperResearchDelivery",
                Label = Translations.Get("OPTION.SWEEPERRESEARCH", lang),
                Tooltip = Translations.Get("TOOLTIP.SWEEPERRESEARCH", lang),
                Getter = () => opt.SweeperResearchDelivery,
                Setter = v => opt.SweeperResearchDelivery = v,
                DefaultValue = false
            });
            resBasic.SubOptions.Add(new BuildingSubOption
            {
                Id = "ProgressBarResearch",
                Label = Translations.Get("OPTION.PROGRESSBAR.RESEARCH", lang),
                Tooltip = Translations.Get("TOOLTIP.PROGRESSBAR.RESEARCH", lang),
                Getter = () => opt.ProgressBarResearch,
                Setter = v => opt.ProgressBarResearch = v,
                DefaultValue = true
            });
            list.Add(resBasic);

            BuildingConfigItem resAdv = new BuildingConfigItem
            {
                Id = "AdvancedResearchCenter",
                Category = BuildingCategory.Research,
                LocalizationKey = "BUILDING.ADVANCEDRESEARCHCENTER",
                DescriptionKey = "BUILDINGDESC.ADVANCEDRESEARCHCENTER",
                MasterGetter = () => opt.UnmannedAdvancedResearchCenter,
                MasterSetter = v => opt.UnmannedAdvancedResearchCenter = v,
                MasterDefault = false
            };
            resAdv.SubOptions.Add(new BuildingSubOption
            {
                Id = "SweeperResearchDelivery",
                Label = Translations.Get("OPTION.SWEEPERRESEARCH", lang),
                Tooltip = Translations.Get("TOOLTIP.SWEEPERRESEARCH", lang),
                Getter = () => opt.SweeperResearchDelivery,
                Setter = v => opt.SweeperResearchDelivery = v,
                DefaultValue = false
            });
            resAdv.SubOptions.Add(new BuildingSubOption
            {
                Id = "ProgressBarResearch",
                Label = Translations.Get("OPTION.PROGRESSBAR.RESEARCH", lang),
                Tooltip = Translations.Get("TOOLTIP.PROGRESSBAR.RESEARCH", lang),
                Getter = () => opt.ProgressBarResearch,
                Setter = v => opt.ProgressBarResearch = v,
                DefaultValue = true
            });
            list.Add(resAdv);

            BuildingConfigItem resCosmic = new BuildingConfigItem
            {
                Id = "DLC1CosmicResearchCenter",
                Category = BuildingCategory.Research,
                LocalizationKey = "BUILDING.DLC1COSMICRESEARCHCENTER",
                DescriptionKey = "BUILDINGDESC.DLC1COSMICRESEARCHCENTER",
                MasterGetter = () => opt.UnmannedDlc1CosmicResearchCenter || opt.UnmannedCosmicResearchCenter,
                MasterSetter = v => { opt.UnmannedDlc1CosmicResearchCenter = v; opt.UnmannedCosmicResearchCenter = v; },
                MasterDefault = false
            };
            resCosmic.SubOptions.Add(new BuildingSubOption
            {
                Id = "SweeperResearchDelivery",
                Label = Translations.Get("OPTION.SWEEPERRESEARCH", lang),
                Tooltip = Translations.Get("TOOLTIP.SWEEPERRESEARCH", lang),
                Getter = () => opt.SweeperResearchDelivery,
                Setter = v => opt.SweeperResearchDelivery = v,
                DefaultValue = false
            });
            resCosmic.SubOptions.Add(new BuildingSubOption
            {
                Id = "ProgressBarResearch",
                Label = Translations.Get("OPTION.PROGRESSBAR.RESEARCH", lang),
                Tooltip = Translations.Get("TOOLTIP.PROGRESSBAR.RESEARCH", lang),
                Getter = () => opt.ProgressBarResearch,
                Setter = v => opt.ProgressBarResearch = v,
                DefaultValue = true
            });
            list.Add(resCosmic);

            BuildingConfigItem resNuclear = new BuildingConfigItem
            {
                Id = "NuclearResearchCenter",
                Category = BuildingCategory.Research,
                LocalizationKey = "BUILDING.NUCLEARRESEARCHCENTER",
                DescriptionKey = "BUILDINGDESC.NUCLEARRESEARCHCENTER",
                MasterGetter = () => opt.UnmannedNuclearResearchCenter,
                MasterSetter = v => opt.UnmannedNuclearResearchCenter = v,
                MasterDefault = false
            };
            resNuclear.SubOptions.Add(new BuildingSubOption
            {
                Id = "SweeperResearchDelivery",
                Label = Translations.Get("OPTION.SWEEPERRESEARCH", lang),
                Tooltip = Translations.Get("TOOLTIP.SWEEPERRESEARCH", lang),
                Getter = () => opt.SweeperResearchDelivery,
                Setter = v => opt.SweeperResearchDelivery = v,
                DefaultValue = false
            });
            resNuclear.SubOptions.Add(new BuildingSubOption
            {
                Id = "ProgressBarResearch",
                Label = Translations.Get("OPTION.PROGRESSBAR.RESEARCH", lang),
                Tooltip = Translations.Get("TOOLTIP.PROGRESSBAR.RESEARCH", lang),
                Getter = () => opt.ProgressBarResearch,
                Setter = v => opt.ProgressBarResearch = v,
                DefaultValue = true
            });
            list.Add(resNuclear);

            list.Add(new BuildingConfigItem
            {
                Id = "OrbitalResearchCenter",
                Category = BuildingCategory.Research,
                LocalizationKey = "BUILDING.ORBITALRESEARCHCENTER",
                DescriptionKey = "BUILDINGDESC.ORBITALRESEARCHCENTER",
                MasterGetter = () => opt.UnmannedOrbitalResearchCenter,
                MasterSetter = v => opt.UnmannedOrbitalResearchCenter = v,
                MasterDefault = false
            });

            list.Add(new BuildingConfigItem
            {
                Id = "GeneticAnalysisStation",
                Category = BuildingCategory.Research,
                LocalizationKey = "BUILDING.GENETICANALYSISSTATION",
                DescriptionKey = "BUILDINGDESC.GENETICANALYSISSTATION",
                MasterGetter = () => opt.UnmannedGeneticAnalysisStation,
                MasterSetter = v => opt.UnmannedGeneticAnalysisStation = v,
                MasterDefault = false
            });

            list.Add(new BuildingConfigItem
            {
                Id = "MorbRoverMaker",
                Category = BuildingCategory.Research,
                LocalizationKey = "BUILDING.MORBROVERMAKER",
                DescriptionKey = "BUILDINGDESC.MORBROVERMAKER",
                MasterGetter = () => opt.UnmannedMorbRoverMaker,
                MasterSetter = v => opt.UnmannedMorbRoverMaker = v,
                MasterDefault = false
            });

            BuildingConfigItem tele = new BuildingConfigItem
            {
                Id = "Telescope",
                Category = BuildingCategory.Research,
                LocalizationKey = "BUILDING.TELESCOPE",
                DescriptionKey = "BUILDINGDESC.TELESCOPE",
                MasterGetter = () => opt.UnmannedTelescope,
                MasterSetter = v => opt.UnmannedTelescope = v,
                MasterDefault = false
            };
            tele.SubOptions.Add(new BuildingSubOption
            {
                Id = "ProgressBarTelescopes",
                Label = Translations.Get("OPTION.PROGRESSBAR.TELESCOPES", lang),
                Tooltip = Translations.Get("TOOLTIP.PROGRESSBAR.TELESCOPES", lang),
                Getter = () => opt.ProgressBarTelescopes,
                Setter = v => opt.ProgressBarTelescopes = v,
                DefaultValue = true
            });
            list.Add(tele);

            BuildingConfigItem clusterTele = new BuildingConfigItem
            {
                Id = "ClusterTelescope",
                Category = BuildingCategory.Research,
                LocalizationKey = "BUILDING.CLUSTERTELESCOPE",
                DescriptionKey = "BUILDINGDESC.CLUSTERTELESCOPE",
                MasterGetter = () => opt.UnmannedClusterTelescope,
                MasterSetter = v => opt.UnmannedClusterTelescope = v,
                MasterDefault = false
            };
            clusterTele.SubOptions.Add(new BuildingSubOption
            {
                Id = "ProgressBarTelescopes",
                Label = Translations.Get("OPTION.PROGRESSBAR.TELESCOPES", lang),
                Tooltip = Translations.Get("TOOLTIP.PROGRESSBAR.TELESCOPES", lang),
                Getter = () => opt.ProgressBarTelescopes,
                Setter = v => opt.ProgressBarTelescopes = v,
                DefaultValue = true
            });
            list.Add(clusterTele);

            BuildingConfigItem encTele = new BuildingConfigItem
            {
                Id = "ClusterTelescopeEnclosed",
                Category = BuildingCategory.Research,
                LocalizationKey = "BUILDING.CLUSTERTELESCOPEENCLOSED",
                DescriptionKey = "BUILDINGDESC.CLUSTERTELESCOPEENCLOSED",
                MasterGetter = () => opt.UnmannedClusterTelescopeEnclosed,
                MasterSetter = v => opt.UnmannedClusterTelescopeEnclosed = v,
                MasterDefault = false
            };
            encTele.SubOptions.Add(new BuildingSubOption
            {
                Id = "ProgressBarTelescopes",
                Label = Translations.Get("OPTION.PROGRESSBAR.TELESCOPES", lang),
                Tooltip = Translations.Get("TOOLTIP.PROGRESSBAR.TELESCOPES", lang),
                Getter = () => opt.ProgressBarTelescopes,
                Setter = v => opt.ProgressBarTelescopes = v,
                DefaultValue = true
            });
            list.Add(encTele);


            // ==========================================
            // 7. Manual & Utility
            // ==========================================
            list.Add(new BuildingConfigItem
            {
                Id = "ManualGenerator",
                Category = BuildingCategory.Manual,
                LocalizationKey = "BUILDING.MANUALGENERATOR",
                DescriptionKey = "BUILDINGDESC.MANUALGENERATOR",
                MasterGetter = () => opt.UnmannedManualGenerator,
                MasterSetter = v => opt.UnmannedManualGenerator = v,
                MasterDefault = false
            });

            list.Add(new BuildingConfigItem
            {
                Id = "ManualHighEnergyParticleSpawner",
                Category = BuildingCategory.Manual,
                LocalizationKey = "BUILDING.MANUALHIGHENERGYPARTICLESPAWNER",
                DescriptionKey = "BUILDINGDESC.MANUALHIGHENERGYPARTICLESPAWNER",
                MasterGetter = () => opt.UnmannedManualHighEnergyParticleSpawner,
                MasterSetter = v => opt.UnmannedManualHighEnergyParticleSpawner = v,
                MasterDefault = false
            });

            list.Add(new BuildingConfigItem
            {
                Id = "ResetSkillsStation",
                Category = BuildingCategory.Manual,
                LocalizationKey = "BUILDING.RESETSKILLSSTATION",
                DescriptionKey = "BUILDINGDESC.RESETSKILLSSTATION",
                MasterGetter = () => opt.UnmannedResetSkillsStation,
                MasterSetter = v => opt.UnmannedResetSkillsStation = v,
                MasterDefault = false
            });

            list.Add(new BuildingConfigItem
            {
                Id = "IceKettle",
                Category = BuildingCategory.Manual,
                LocalizationKey = "BUILDING.ICEKETTLE",
                DescriptionKey = "BUILDINGDESC.ICEKETTLE",
                MasterGetter = () => opt.UnmannedIceKettle,
                MasterSetter = v => opt.UnmannedIceKettle = v,
                MasterDefault = false
            });

            list.Add(new BuildingConfigItem
            {
                Id = "Campfire",
                Category = BuildingCategory.Manual,
                LocalizationKey = "BUILDING.CAMPFIRE",
                DescriptionKey = "BUILDINGDESC.CAMPFIRE",
                MasterGetter = () => opt.UnmannedCampfire,
                MasterSetter = v => opt.UnmannedCampfire = v,
                MasterDefault = false
            });

            BuildingConfigItem iceFan = new BuildingConfigItem
            {
                Id = "IceCooledFan",
                Category = BuildingCategory.Manual,
                LocalizationKey = "BUILDING.ICECOOLEDFAN",
                DescriptionKey = "BUILDINGDESC.ICECOOLEDFAN",
                MasterGetter = () => opt.UnmannedIceCooledFan,
                MasterSetter = v => opt.UnmannedIceCooledFan = v,
                MasterDefault = false
            };
            iceFan.SubOptions.Add(new BuildingSubOption
            {
                Id = "IgnoreTooColdIceCooledFan",
                Label = Translations.Get("OPTION.IGNORETOOCOLD.ICECOOLEDFAN", lang),
                Tooltip = Translations.Get("TOOLTIP.IGNORETOOCOLD.ICECOOLEDFAN", lang),
                Getter = () => opt.IgnoreTooColdIceCooledFan,
                Setter = v => opt.IgnoreTooColdIceCooledFan = v,
                DefaultValue = false
            });
            list.Add(iceFan);

            list.Add(new BuildingConfigItem
            {
                Id = "Compost",
                Category = BuildingCategory.Manual,
                LocalizationKey = "BUILDING.COMPOST",
                DescriptionKey = "BUILDINGDESC.COMPOST",
                MasterGetter = () => opt.UnmannedCompost,
                MasterSetter = v => opt.UnmannedCompost = v,
                MasterDefault = false
            });

            BuildingConfigItem liqBottler = new BuildingConfigItem
            {
                Id = "LiquidBottler",
                Category = BuildingCategory.Manual,
                LocalizationKey = "BUILDING.LIQUIDBOTTLER",
                DescriptionKey = "BUILDINGDESC.LIQUIDBOTTLER",
                MasterGetter = () => opt.UnmannedLiquidBottler,
                MasterSetter = v => opt.UnmannedLiquidBottler = v,
                MasterDefault = false
            };
            liqBottler.SubOptions.Add(new BuildingSubOption
            {
                Id = "ProgressBarBottler",
                Label = Translations.Get("OPTION.PROGRESSBAR.BOTTLER", lang),
                Tooltip = Translations.Get("TOOLTIP.PROGRESSBAR.BOTTLER", lang),
                Getter = () => opt.ProgressBarBottler,
                Setter = v => opt.ProgressBarBottler = v,
                DefaultValue = true
            });
            liqBottler.SubOptions.Add(new BuildingSubOption
            {
                Id = "AutoSweeperBottlerDirectPickup",
                Label = Translations.Get("OPTION.AUTOSWEEPERBOTTLERDIRECTPICKUP", lang),
                Tooltip = Translations.Get("TOOLTIP.AUTOSWEEPERBOTTLERDIRECTPICKUP", lang),
                Getter = () => opt.AutoSweeperBottlerDirectPickup,
                Setter = v => opt.AutoSweeperBottlerDirectPickup = v,
                DefaultValue = true
            });
            list.Add(liqBottler);

            BuildingConfigItem gasBottler = new BuildingConfigItem
            {
                Id = "GasBottler",
                Category = BuildingCategory.Manual,
                LocalizationKey = "BUILDING.GASBOTTLER",
                DescriptionKey = "BUILDINGDESC.GASBOTTLER",
                MasterGetter = () => opt.UnmannedGasBottler,
                MasterSetter = v => opt.UnmannedGasBottler = v,
                MasterDefault = false
            };
            gasBottler.SubOptions.Add(new BuildingSubOption
            {
                Id = "ProgressBarBottler",
                Label = Translations.Get("OPTION.PROGRESSBAR.BOTTLER", lang),
                Tooltip = Translations.Get("TOOLTIP.PROGRESSBAR.BOTTLER", lang),
                Getter = () => opt.ProgressBarBottler,
                Setter = v => opt.ProgressBarBottler = v,
                DefaultValue = true
            });
            gasBottler.SubOptions.Add(new BuildingSubOption
            {
                Id = "AutoSweeperBottlerDirectPickup",
                Label = Translations.Get("OPTION.AUTOSWEEPERBOTTLERDIRECTPICKUP", lang),
                Tooltip = Translations.Get("TOOLTIP.AUTOSWEEPERBOTTLERDIRECTPICKUP", lang),
                Getter = () => opt.AutoSweeperBottlerDirectPickup,
                Setter = v => opt.AutoSweeperBottlerDirectPickup = v,
                DefaultValue = true
            });
            list.Add(gasBottler);

            list.Add(new BuildingConfigItem
            {
                Id = "LiquidPumpingStation",
                Category = BuildingCategory.Manual,
                LocalizationKey = "BUILDING.LIQUIDPUMPINGSTATION",
                DescriptionKey = "BUILDINGDESC.LIQUIDPUMPINGSTATION",
                MasterGetter = () => opt.UnmannedLiquidPumpingStation,
                MasterSetter = v => opt.UnmannedLiquidPumpingStation = v,
                MasterDefault = false
            });

            cachedItems = list;
            return cachedItems;
        }

        public static void ResetAllToDefault()
        {
            foreach (BuildingConfigItem item in GetAllItems())
            {
                item.ResetToDefault();
            }
            BuildingConfigItem.AutoSave();
        }

        public static void SetAllEnabled(bool enabled)
        {
            foreach (BuildingConfigItem item in GetAllItems())
            {
                item.SetEnabled(enabled);
            }
            BuildingConfigItem.AutoSave();
        }

        public static bool AreAllEnabled()
        {
            var all = GetAllItems();
            if (all.Count == 0) return false;
            foreach (BuildingConfigItem item in all)
            {
                if (!item.IsEnabled) return false;
            }
            return true;
        }
    }
}
