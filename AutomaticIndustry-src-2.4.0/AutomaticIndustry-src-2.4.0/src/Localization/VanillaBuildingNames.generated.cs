// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.
// Original mod concept: "AutoMachine" (Steam Workshop id 2992024030).
//
// GENERATED FILE - do not edit by hand.
//
// Every string below is copied verbatim from the localization files shipped
// with Oxygen Not Included (OxygenNotIncluded_Data/StreamingAssets/strings):
//
//   English             strings_template.pot            (source strings)
//   Simplified Chinese  strings_preinstalled_zh_klei.po (official Klei)
//   Korean              strings_preinstalled_ko_klei.po (official Klei)
//   Traditional Chinese converted from the official Simplified Chinese text
//                       with OpenCC (s2twp), because Klei ships no zh-Hant file
//
// The key of every row is the mod option key; the value is the vanilla
// building name with all rich text markup removed. Japanese is intentionally
// absent: Oxygen Not Included ships no official Japanese localization, so the
// Japanese fallback chain is resolved at runtime (see BuildingDescriptions).
//
// Regenerate with: tools/generate_vanilla_names.py

using System.Collections.Generic;
using AutoMachineRebuilt.Config;

namespace AutoMachineRebuilt.Localization
{
    /// <summary>
    /// Vanilla building names in every language the game itself ships, keyed
    /// by the mod option key. Used as the single source of truth for every
    /// building label shown by the mod, so a translated label can never drift
    /// away from the wording the player sees in game.
    /// </summary>
    internal static class VanillaBuildingNames
    {
        /// <summary>Vanilla English building names (bilingual base text).</summary>
        internal static readonly Dictionary<string, string> English = new Dictionary<string, string>
        {
            { "MANUALGENERATOR", "Manual Generator" },
            { "TELESCOPE", "Telescope" },
            { "CLUSTERTELESCOPE", "Telescope" },
            { "CLUSTERTELESCOPEENCLOSED", "Enclosed Telescope" },
            { "MANUALHIGHENERGYPARTICLESPAWNER", "Manual Radbolt Generator" },
            { "RESETSKILLSSTATION", "Skill Scrubber" },
            { "APOTHECARY", "Apothecary" },
            { "ADVANCEDAPOTHECARY", "Nuclear Apothecary" },
            { "SUSHIBAR", "Sushi Bar" },
            { "ICEKETTLE", "Ice Liquefier" },
            { "CAMPFIRE", "Wood Heater" },
            { "ICECOOLEDFAN", "Ice-E Fan" },
            { "COMPOST", "Compost" },
            { "FOODDEHYDRATOR", "Dehydrator" },
            { "RESEARCHCENTER", "Research Station" },
            { "ADVANCEDRESEARCHCENTER", "Super Computer" },
            { "COSMICRESEARCHCENTER", "Virtual Planetarium" },
            { "DLC1COSMICRESEARCHCENTER", "Virtual Planetarium" },
            { "NUCLEARRESEARCHCENTER", "Materials Study Terminal" },
            { "ORBITALRESEARCHCENTER", "Orbital Data Collection Lab" },
            { "GENETICANALYSISSTATION", "Botanical Analyzer" },
            { "MORBROVERMAKER", "Biobot Builder" },
            { "MISSIONCONTROL", "Mission Control Station" },
            { "MISSIONCONTROLCLUSTER", "Mission Control Station" },
            { "FARMSTATION", "Farm Station" },
            { "SPICEGRINDER", "Spice Grinder" },
            { "COOKINGSTATION", "Electric Grill" },
            { "GOURMETCOOKINGSTATION", "Gas Range" },
            { "MICROBEMUSHER", "Microbe Musher" },
            { "DEEPFRYER", "Deep Fryer" },
            { "MILKPRESS", "Plant Pulverizer" },
            { "SMOKER", "Smoker" },
            { "ROCKCRUSHER", "Rock Crusher" },
            { "METALREFINERY", "Metal Refinery" },
            { "GLASSFORGE", "Glass Forge" },
            { "SUPERMATERIALREFINERY", "Molecular Forge" },
            { "SUITFABRICATOR", "Exosuit Forge" },
            { "CLOTHINGFABRICATOR", "Textile Loom" },
            { "CLOTHINGALTERATIONSTATION", "Clothing Refashionator" },
            { "CRAFTINGTABLE", "Crafting Station" },
            { "ADVANCEDCRAFTINGTABLE", "Soldering Station" },
            { "SLUDGEPRESS", "Sludge Press" },
            { "DIAMONDPRESS", "Diamond Press" },
            { "CHEMICALREFINERY", "Emulsifier" },
            { "MISSILEFABRICATOR", "Blastshot Maker" },
            { "DATAMINER", "Data Miner" },
            { "OILREFINERY", "Oil Refinery" },
            { "OILWELLCAP", "Oil Well" },
            { "GEOTUNER", "Geotuner" },
            { "DESALINATOR", "Desalinator" },
            { "FABRICATEDWOODMAKER", "Plywood Press" },
            { "POWERCONTROLSTATION", "Power Control Station" },
            { "MILKFATSEPARATOR", "Gleaner" },
            { "RANCHSTATION", "Grooming Station" },
            { "SHEARINGSTATION", "Shearing Station" },
            { "MILKINGSTATION", "Milking Station" },
            { "UNDERWATERRANCHSTATION", "Aquatic Grooming Station" },
            { "UNDERWATERSHEARINGSTATION", "Aquatic Shearing Station" },
            { "UNDERWATERMILKINGSTATION", "Aquatic Milking Station" },
            { "LIQUIDBOTTLER", "Bottle Filler" },
            { "GASBOTTLER", "Canister Filler" },
            { "LIQUIDPUMPINGSTATION", "Pitcher Pump" }
        };

        /// <summary>Official Klei Simplified Chinese building names.</summary>
        internal static readonly Dictionary<string, string> ChineseSimplified = new Dictionary<string, string>
        {
            { "MANUALGENERATOR", "人力发电机" },
            { "TELESCOPE", "望远镜" },
            { "CLUSTERTELESCOPE", "望远镜" },
            { "CLUSTERTELESCOPEENCLOSED", "隔绝式望远镜" },
            { "MANUALHIGHENERGYPARTICLESPAWNER", "人力辐射粒子发生器" },
            { "RESETSKILLSSTATION", "技能涤除器" },
            { "APOTHECARY", "配药桌" },
            { "ADVANCEDAPOTHECARY", "核能配药桌" },
            { "SUSHIBAR", "寿司台" },
            { "ICEKETTLE", "融冰壶" },
            { "CAMPFIRE", "柴火炉" },
            { "ICECOOLEDFAN", "冰冷风扇" },
            { "COMPOST", "堆肥堆" },
            { "FOODDEHYDRATOR", "脱水机" },
            { "RESEARCHCENTER", "研究站" },
            { "ADVANCEDRESEARCHCENTER", "超级计算机" },
            { "COSMICRESEARCHCENTER", "虚拟天象仪" },
            { "DLC1COSMICRESEARCHCENTER", "虚拟天象仪" },
            { "NUCLEARRESEARCHCENTER", "材料研究终端" },
            { "ORBITALRESEARCHCENTER", "轨道数据收集实验仪" },
            { "GENETICANALYSISSTATION", "植物分析仪" },
            { "MORBROVERMAKER", "生机组构仪" },
            { "MISSIONCONTROL", "航天指挥站" },
            { "MISSIONCONTROLCLUSTER", "航天指挥站" },
            { "FARMSTATION", "农业站" },
            { "SPICEGRINDER", "香料研磨器" },
            { "COOKINGSTATION", "电动烤炉" },
            { "GOURMETCOOKINGSTATION", "燃气灶" },
            { "MICROBEMUSHER", "食物压制器" },
            { "DEEPFRYER", "油炸锅" },
            { "MILKPRESS", "植物粉碎机" },
            { "SMOKER", "熏炉" },
            { "ROCKCRUSHER", "碎石机" },
            { "METALREFINERY", "金属精炼器" },
            { "GLASSFORGE", "玻璃熔炉" },
            { "SUPERMATERIALREFINERY", "分子熔炉" },
            { "SUITFABRICATOR", "太空服锻造台" },
            { "CLOTHINGFABRICATOR", "纺织机" },
            { "CLOTHINGALTERATIONSTATION", "时装翻新器" },
            { "CRAFTINGTABLE", "工作台" },
            { "ADVANCEDCRAFTINGTABLE", "焊接台" },
            { "SLUDGEPRESS", "泥浆分离器" },
            { "DIAMONDPRESS", "钻石压机" },
            { "CHEMICALREFINERY", "乳化器" },
            { "MISSILEFABRICATOR", "爆破弹组装机" },
            { "DATAMINER", "数据挖掘仪" },
            { "OILREFINERY", "原油精炼器" },
            { "OILWELLCAP", "油井" },
            { "GEOTUNER", "地质调谐仪" },
            { "DESALINATOR", "脱盐器" },
            { "FABRICATEDWOODMAKER", "胶合板压机" },
            { "POWERCONTROLSTATION", "电控站" },
            { "MILKFATSEPARATOR", "提炼器" },
            { "RANCHSTATION", "梳理站" },
            { "SHEARINGSTATION", "修剪站" },
            { "MILKINGSTATION", "挤奶站" },
            { "UNDERWATERRANCHSTATION", "水生梳理站" },
            { "UNDERWATERSHEARINGSTATION", "水生修剪站" },
            { "UNDERWATERMILKINGSTATION", "水生挤奶站" },
            { "LIQUIDBOTTLER", "储液罐空罐器" },
            { "GASBOTTLER", "储气罐空罐器" },
            { "LIQUIDPUMPINGSTATION", "压水泵" }
        };

        /// <summary>Traditional Chinese, converted from the official Simplified Chinese text.</summary>
        internal static readonly Dictionary<string, string> ChineseTraditional = new Dictionary<string, string>
        {
            { "MANUALGENERATOR", "人力發電機" },
            { "TELESCOPE", "望遠鏡" },
            { "CLUSTERTELESCOPE", "望遠鏡" },
            { "CLUSTERTELESCOPEENCLOSED", "隔絕式望遠鏡" },
            { "MANUALHIGHENERGYPARTICLESPAWNER", "人力輻射粒子發生器" },
            { "RESETSKILLSSTATION", "技能滌除器" },
            { "APOTHECARY", "配藥桌" },
            { "ADVANCEDAPOTHECARY", "核能配藥桌" },
            { "SUSHIBAR", "壽司臺" },
            { "ICEKETTLE", "融冰壺" },
            { "CAMPFIRE", "柴火爐" },
            { "ICECOOLEDFAN", "冰冷風扇" },
            { "COMPOST", "堆肥堆" },
            { "FOODDEHYDRATOR", "脫水機" },
            { "RESEARCHCENTER", "研究站" },
            { "ADVANCEDRESEARCHCENTER", "超級計算機" },
            { "COSMICRESEARCHCENTER", "虛擬天象儀" },
            { "DLC1COSMICRESEARCHCENTER", "虛擬天象儀" },
            { "NUCLEARRESEARCHCENTER", "材料研究終端" },
            { "ORBITALRESEARCHCENTER", "軌道資料收集實驗儀" },
            { "GENETICANALYSISSTATION", "植物分析儀" },
            { "MORBROVERMAKER", "生機組構儀" },
            { "MISSIONCONTROL", "航天指揮站" },
            { "MISSIONCONTROLCLUSTER", "航天指揮站" },
            { "FARMSTATION", "農業站" },
            { "SPICEGRINDER", "香料研磨器" },
            { "COOKINGSTATION", "電動烤爐" },
            { "GOURMETCOOKINGSTATION", "燃氣灶" },
            { "MICROBEMUSHER", "食物壓制器" },
            { "DEEPFRYER", "油炸鍋" },
            { "MILKPRESS", "植物粉碎機" },
            { "SMOKER", "燻爐" },
            { "ROCKCRUSHER", "碎石機" },
            { "METALREFINERY", "金屬精煉器" },
            { "GLASSFORGE", "玻璃熔爐" },
            { "SUPERMATERIALREFINERY", "分子熔爐" },
            { "SUITFABRICATOR", "太空服鍛造臺" },
            { "CLOTHINGFABRICATOR", "紡織機" },
            { "CLOTHINGALTERATIONSTATION", "時裝翻新器" },
            { "CRAFTINGTABLE", "工作臺" },
            { "ADVANCEDCRAFTINGTABLE", "焊接臺" },
            { "SLUDGEPRESS", "泥漿分離器" },
            { "DIAMONDPRESS", "鑽石壓機" },
            { "CHEMICALREFINERY", "乳化器" },
            { "MISSILEFABRICATOR", "爆破彈組裝機" },
            { "DATAMINER", "資料探勘儀" },
            { "OILREFINERY", "原油精煉器" },
            { "OILWELLCAP", "油井" },
            { "GEOTUNER", "地質調諧儀" },
            { "DESALINATOR", "脫鹽器" },
            { "FABRICATEDWOODMAKER", "膠合板壓機" },
            { "POWERCONTROLSTATION", "電控站" },
            { "MILKFATSEPARATOR", "提煉器" },
            { "RANCHSTATION", "梳理站" },
            { "SHEARINGSTATION", "修剪站" },
            { "MILKINGSTATION", "擠奶站" },
            { "UNDERWATERRANCHSTATION", "水生梳理站" },
            { "UNDERWATERSHEARINGSTATION", "水生修剪站" },
            { "UNDERWATERMILKINGSTATION", "水生擠奶站" },
            { "LIQUIDBOTTLER", "儲液罐空罐器" },
            { "GASBOTTLER", "儲氣罐空罐器" },
            { "LIQUIDPUMPINGSTATION", "壓水泵" }
        };

        /// <summary>Official Klei Korean building names.</summary>
        internal static readonly Dictionary<string, string> Korean = new Dictionary<string, string>
        {
            { "MANUALGENERATOR", "수동 발전기" },
            { "TELESCOPE", "망원경" },
            { "CLUSTERTELESCOPE", "망원경" },
            { "CLUSTERTELESCOPEENCLOSED", "에워싸인 망원경" },
            { "MANUALHIGHENERGYPARTICLESPAWNER", "수동 래드볼트 생성기" },
            { "RESETSKILLSSTATION", "기술 소제기" },
            { "APOTHECARY", "약제대" },
            { "ADVANCEDAPOTHECARY", "핵 약제대" },
            { "SUSHIBAR", "스시바" },
            { "ICEKETTLE", "얼음 액화기" },
            { "CAMPFIRE", "목재 히터" },
            { "ICECOOLEDFAN", "아이스-E 팬" },
            { "COMPOST", "퇴비" },
            { "FOODDEHYDRATOR", "탈수기" },
            { "RESEARCHCENTER", "연구소" },
            { "ADVANCEDRESEARCHCENTER", "슈퍼 컴퓨터" },
            { "COSMICRESEARCHCENTER", "가상 플라네타륨" },
            { "DLC1COSMICRESEARCHCENTER", "가상 플라네타륨" },
            { "NUCLEARRESEARCHCENTER", "재료 연구 터미널" },
            { "ORBITALRESEARCHCENTER", "궤도 데이터 수집 연구실" },
            { "GENETICANALYSISSTATION", "식물 분석기" },
            { "MORBROVERMAKER", "바이오봇 빌더" },
            { "MISSIONCONTROL", "임무통제 스테이션" },
            { "MISSIONCONTROLCLUSTER", "미션 컨트롤 스테이션" },
            { "FARMSTATION", "재배스테이션" },
            { "SPICEGRINDER", "향신료 분쇄기" },
            { "COOKINGSTATION", "전기 그릴" },
            { "GOURMETCOOKINGSTATION", "가스레인지" },
            { "MICROBEMUSHER", "미생물 걸죽기" },
            { "DEEPFRYER", "튀김기" },
            { "MILKPRESS", "식물 분쇄기" },
            { "SMOKER", "스모커" },
            { "ROCKCRUSHER", "쇄석기" },
            { "METALREFINERY", "금속 제련소" },
            { "GLASSFORGE", "유리 제조소" },
            { "SUPERMATERIALREFINERY", "분자 제조소" },
            { "SUITFABRICATOR", "특수복 제조소" },
            { "CLOTHINGFABRICATOR", "직물 직조기" },
            { "CLOTHINGALTERATIONSTATION", "의류 리패션에이터" },
            { "CRAFTINGTABLE", "제작 스테이션" },
            { "ADVANCEDCRAFTINGTABLE", "땜질 스테이션" },
            { "SLUDGEPRESS", "슬러지 프레스" },
            { "DIAMONDPRESS", "다이아몬드 프레스" },
            { "CHEMICALREFINERY", "유화기" },
            { "MISSILEFABRICATOR", "블래스트숏 메이커" },
            { "DATAMINER", "데이터 마이너" },
            { "OILREFINERY", "석유 정제소" },
            { "OILWELLCAP", "유정" },
            { "GEOTUNER", "지오튜너" },
            { "DESALINATOR", "탈염기" },
            { "FABRICATEDWOODMAKER", "합판 압착기" },
            { "POWERCONTROLSTATION", "전력 관리소" },
            { "MILKFATSEPARATOR", "글리너" },
            { "RANCHSTATION", "치장스테이션" },
            { "SHEARINGSTATION", "털 깎기 스테이션" },
            { "MILKINGSTATION", "착유 스테이션" },
            { "UNDERWATERRANCHSTATION", "수생 치장 스테이션" },
            { "UNDERWATERSHEARINGSTATION", "수생 털 깎기 스테이션" },
            { "UNDERWATERMILKINGSTATION", "수생 착유 스테이션" },
            { "LIQUIDBOTTLER", "액체 병 포장기" },
            { "GASBOTTLER", "기체 캔 포장기" },
            { "LIQUIDPUMPINGSTATION", "물 펌프" }
        };

        /// <summary>
        /// Returns the vanilla name table of a language, or <c>null</c> when
        /// the game ships no localization for it and the runtime fallback
        /// chain has to be used instead.
        /// </summary>
        /// <param name="language">Resolved options language.</param>
        internal static Dictionary<string, string> For(UiLanguage language)
        {
            switch (language)
            {
                case UiLanguage.ChineseSimplified:
                    return ChineseSimplified;
                case UiLanguage.ChineseTraditional:
                    return ChineseTraditional;
                case UiLanguage.Korean:
                    return Korean;
                case UiLanguage.English:
                    return English;
                default:
                    return null;
            }
        }
    }
}
