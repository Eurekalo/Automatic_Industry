// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System.Collections.Generic;
using AutoMachineRebuilt.Config;

namespace AutoMachineRebuilt.Localization
{
    /// <summary>
    /// Per building help texts and localized building names.
    ///
    /// The option rows of this mod used to share a handful of generic
    /// tooltips. This table gives every automated building its own text that
    /// explains what the vanilla building does and what the automation adds,
    /// so players can judge each toggle on its own.
    ///
    /// Building names are also kept here: the options screen has its own
    /// language selector, which may differ from the language the game itself
    /// runs in. When a name is missing for the selected language the binder
    /// falls back to the name reported by the running game.
    ///
    /// Description keys are the suffix after
    /// <c>STRINGS.AUTOMACHINEREBUILT.</c>, for example
    /// <c>BUILDINGDESC.APOTHECARY</c>.
    /// </summary>
    internal static class BuildingDescriptions
    {
        /// <summary>Key prefix of every per building description.</summary>
        internal const string DescriptionKeyPrefix = "BUILDINGDESC.";

        /// <summary>Per building help texts (English).</summary>
        internal static readonly Dictionary<string, string> DescriptionsEnglish = new Dictionary<string, string>
        {
            { "BUILDINGDESC.FABRICATEDWOODMAKER", "Vanilla: a Duplicant presses plant fibre and resin into Plywood. Automation runs the recipe queue on its own while power and both ingredients are available." },
            { "BUILDINGDESC.POWERCONTROLSTATION", "Vanilla: a Duplicant with the Power Tinkering perk crafts microchips from refined metal in a Power Plant room. Automation crafts them without a Duplicant; the room and skill requirements can be waived separately below." },
            { "BUILDINGDESC.MANUALGENERATOR", "Vanilla: a Duplicant runs the wheel to produce 400 W while consuming stamina. Automation turns the wheel by itself only while the attached circuit still needs power, and stops when every battery on it is full." },
            { "BUILDINGDESC.TELESCOPE", "Vanilla: a Duplicant analyses a selected space destination to reveal it. Automation only runs when a valid analysable target is assigned, and never touches the telescope while it has no target." },
            { "BUILDINGDESC.CLUSTERTELESCOPE", "Vanilla (Spaced Out!): a Duplicant analyses starmap destinations from the surface. Automation analyses only the currently assigned target and idles while none is selected." },
            { "BUILDINGDESC.CLUSTERTELESCOPEENCLOSED", "Vanilla (Spaced Out!): the enclosed variant that needs no direct sky access. Automation behaves exactly like the open telescope and requires an assigned target." },
            { "BUILDINGDESC.MANUALHIGHENERGYPARTICLESPAWNER", "Vanilla: a Duplicant cranks the generator to emit radbolts, consuming stamina. Automation cranks it while the radbolt buffer has room and plays the vanilla work animation." },
            { "BUILDINGDESC.RESETSKILLSSTATION", "Vanilla: a Duplicant uses the station to refund all learned skill points. Automation only supplies the operating work; the skill reset itself still requires the player to assign a Duplicant." },
            { "BUILDINGDESC.APOTHECARY", "Vanilla: a Duplicant crafts medicine from delivered ingredients. Automation runs the recipe queue by itself while power and ingredients are available." },
            { "BUILDINGDESC.ADVANCEDAPOTHECARY", "Vanilla: crafts advanced medicine and radiation treatments from delivered ingredients. Automation runs its recipe queue without a Duplicant." },
            { "BUILDINGDESC.SUSHIBAR", "Vanilla: a Duplicant prepares raw-fish dishes from delivered food. Automation runs the recipe queue on its own." },
            { "BUILDINGDESC.ICEKETTLE", "Vanilla: burns lumber to melt delivered ice into water; a Duplicant carries the finished liquid out. Automation only releases the finished liquid storage and never dumps the lumber or ice input." },
            { "BUILDINGDESC.CAMPFIRE", "Vanilla: a Duplicant stokes the fire to burn lumber and heat the surroundings. Automation stokes it while fuel is stored and the building is enabled." },
            { "BUILDINGDESC.ICECOOLEDFAN", "Vanilla: a Duplicant operates the fan, consuming ice to cool the area. Automation operates it while ice is stored." },
            { "BUILDINGDESC.COMPOST", "Vanilla: a Duplicant periodically turns the pile so polluted dirt becomes dirt. Automation turns it on the vanilla cycle whenever a batch is ready, playing the vanilla animation." },
            { "BUILDINGDESC.FOODDEHYDRATOR", "Vanilla: burns lumber to dehydrate food, then a Duplicant empties the dried packets. Automation both runs the recipe queue and releases the finished packets from the output storage." },
            { "BUILDINGDESC.ADVANCEDRESEARCHCENTER", "Vanilla: a Duplicant researches advanced technologies, consuming water and power. Automation researches only while an active research target actually needs this station's research points." },
            { "BUILDINGDESC.COSMICRESEARCHCENTER", "Vanilla: a Duplicant converts collected space data into orbital research points. Automation runs only while data and a matching research target exist." },
            { "BUILDINGDESC.DLC1COSMICRESEARCHCENTER", "Vanilla (Spaced Out!): converts collected data banks into orbital research. Automation requires stored data and an active research target." },
            { "BUILDINGDESC.NUCLEARRESEARCHCENTER", "Vanilla: a Duplicant studies radioactive materials to earn nuclear research points. Automation studies only while an active research target consumes those points." },
            { "BUILDINGDESC.ORBITALRESEARCHCENTER", "Vanilla: a Duplicant processes orbital data banks brought back from space. Automation processes stored data banks while a matching research target exists." },
            { "BUILDINGDESC.GENETICANALYSISSTATION", "Vanilla: a Duplicant analyses plant seeds or mutations to unlock their information. Automation analyses only while a valid sample is loaded." },
            { "BUILDINGDESC.MORBROVERMAKER", "Vanilla: a Duplicant assembles Morb-based rovers from delivered materials. Automation runs its recipe queue without a Duplicant." },
            { "BUILDINGDESC.MISSIONCONTROL", "Vanilla: a Duplicant in a valid room guides a rocket in range, granting the flight speed bonus. Automation grants the same vanilla bonus only while a real rocket is actually in range." },
            { "BUILDINGDESC.MISSIONCONTROLCLUSTER", "Vanilla (Spaced Out!): staffed mission control that speeds up rockets in the cluster. Automation applies the vanilla bonus only to rockets the station itself reports as in range." },
            { "BUILDINGDESC.FARMSTATION", "Vanilla: a Duplicant in a Farm room produces Micronutrient Fertilizer used to tend plants. Automation produces it on the vanilla cycle; an extra option lets it keep producing when no plant currently asks for it." },
            { "BUILDINGDESC.SPICEGRINDER", "Vanilla: a Duplicant grinds delivered ingredients into food spices. Automation runs its recipe queue while power and ingredients are available." },
            { "BUILDINGDESC.COOKINGSTATION", "Vanilla: a Duplicant cooks food recipes using electricity. Automation runs the queued recipes without a cook." },
            { "BUILDINGDESC.GOURMETCOOKINGSTATION", "Vanilla: a Duplicant cooks advanced meals using natural gas. Automation runs the queued recipes on its own." },
            { "BUILDINGDESC.MICROBEMUSHER", "Vanilla: a Duplicant makes basic food such as Mush Bars. Automation runs the queued recipes without a cook." },
            { "BUILDINGDESC.DEEPFRYER", "Vanilla: a Duplicant fries food using oil and power. Automation runs the queued recipes on its own." },
            { "BUILDINGDESC.MILKPRESS", "Vanilla: a Duplicant presses plants into milk and by-products. Automation runs the recipe queue while power and plants are available." },
            { "BUILDINGDESC.SMOKER", "Vanilla: burns lumber to smoke fish, then a Duplicant empties the finished food. Automation runs the recipe and releases the finished food once, guarded against double release." },
            { "BUILDINGDESC.ROCKCRUSHER", "Vanilla: a Duplicant crushes rock and metal ore into refined output. Automation runs the recipe queue without a Duplicant." },
            { "BUILDINGDESC.METALREFINERY", "Vanilla: a Duplicant refines metal ore using a liquid coolant that absorbs the heat. Automation runs the recipe queue while ore and coolant are available." },
            { "BUILDINGDESC.GLASSFORGE", "Vanilla: a Duplicant melts sand into molten glass. Automation runs the recipe queue on its own." },
            { "BUILDINGDESC.SUPERMATERIALREFINERY", "Vanilla: a Duplicant crafts super materials such as Thermium and Steel alloys. Automation runs the recipe queue without a Duplicant." },
            { "BUILDINGDESC.SUITFABRICATOR", "Vanilla: a Duplicant builds and repairs exosuits from delivered materials. Automation runs the recipe queue on its own." },
            { "BUILDINGDESC.CLOTHINGFABRICATOR", "Vanilla: a Duplicant weaves reed fiber into clothing. Automation runs the recipe queue without a Duplicant." },
            { "BUILDINGDESC.CLOTHINGALTERATIONSTATION", "Vanilla: a Duplicant reworks clothing into other outfits. Automation runs the recipe queue on its own." },
            { "BUILDINGDESC.CRAFTINGTABLE", "Vanilla: an artist Duplicant carves decorative sculptures. Automation performs the crafting work itself; quality still follows the vanilla rules." },
            { "BUILDINGDESC.ADVANCEDCRAFTINGTABLE", "Vanilla: a Duplicant assembles automation and electronic components. Automation runs the recipe queue without a Duplicant." },
            { "BUILDINGDESC.SLUDGEPRESS", "Vanilla: a Duplicant presses polluted mud into water and dirt. Automation runs the recipe queue on its own." },
            { "BUILDINGDESC.DIAMONDPRESS", "Vanilla: a Duplicant compresses refined carbon into diamond. Automation runs the recipe queue without a Duplicant." },
            { "BUILDINGDESC.CHEMICALREFINERY", "Vanilla: a Duplicant produces ethanol-based chemical products. Automation runs the recipe queue while power and inputs are available." },
            { "BUILDINGDESC.MISSILEFABRICATOR", "Vanilla: a Duplicant assembles blastshot ammunition from delivered materials. Automation runs the recipe queue without a Duplicant." },
            { "BUILDINGDESC.DATAMINER", "Vanilla: a Duplicant extracts research data from stored data banks. Automation runs the extraction while data is available." },
            { "BUILDINGDESC.OILREFINERY", "Vanilla: a Duplicant refines 10 kg/s crude oil into 5 kg/s petroleum plus natural gas. Automation runs it without a Duplicant; the ratio option can restore the legacy 100 % conversion." },
            { "BUILDINGDESC.OILWELLCAP", "Vanilla: pumps crude oil with water, and a Duplicant must vent the built-up gas pressure. Automation vents the pressure automatically once the threshold is reached." },
            { "BUILDINGDESC.DESALINATOR", "Vanilla: turns salt water into water and leaves salt that a Duplicant must carry out. Automation releases only the accumulated salt output." },
            { "BUILDINGDESC.MILKFATSEPARATOR", "Vanilla: separates milk into fat and water and stores the solid output until a Duplicant empties it. Automation releases only the finished solid output." },
            { "BUILDINGDESC.GEOTUNER", "Vanilla: a scientist Duplicant tunes a targeted geyser to boost its output for a limited time. Automation performs that tuning cycle on the assigned geyser, respecting the vanilla cooldown." },
            { "BUILDINGDESC.VALVE", "Vanilla: after changing the flow setting a Duplicant has to walk over and turn the valve before the new value applies. Automation applies the new flow immediately and cancels the pending valve chore." },
            { "BUILDINGDESC.RESEARCHCENTER", "Vanilla: a Duplicant researches at the station and turns research materials into research points. Automation runs the station on its own while a matching project and research material are available." },
            { "BUILDINGDESC.RANCHSTATION", "Vanilla: a rancher grooms a critter, giving it the Groomed effect that increases reproduction. Automation follows the vanilla ranch state machine, so the critter walks in and both work animations play." },
            { "BUILDINGDESC.SHEARINGSTATION", "Vanilla: a rancher shears a Drecko for Reed Fiber or Plastic. Automation drives the vanilla shearing cycle, including the critter's own animation and cooldown." },
            { "BUILDINGDESC.MILKINGSTATION", "Vanilla: a rancher milks a critter to emit its liquid product. Automation runs the vanilla milking cycle with the critter's animation and cooldown intact." },
            { "BUILDINGDESC.UNDERWATERRANCHSTATION", "Vanilla (Aquatic Pack): grooms swimming critters in a submerged station. Automation uses the same vanilla ranch state machine as the land version." },
            { "BUILDINGDESC.UNDERWATERSHEARINGSTATION", "Vanilla (Aquatic Pack): shears aquatic critters for their fiber product. Automation drives the same vanilla shearing cycle as the land version." },
            { "BUILDINGDESC.UNDERWATERMILKINGSTATION", "Vanilla (Aquatic Pack): milks aquatic critters for their liquid product. Automation runs the same vanilla milking cycle as the land version." },
            { "BUILDINGDESC.LIQUIDBOTTLER", "Vanilla: bottles liquid from an input pipe for manual delivery. Automation automatically dispenses and releases the filled bottles for Auto-Sweepers." },
            { "BUILDINGDESC.GASBOTTLER", "Vanilla: canisters gas from an input pipe for manual delivery. Automation automatically dispenses and releases the filled canisters for Auto-Sweepers." },
            { "BUILDINGDESC.LIQUIDPUMPINGSTATION", "Vanilla: a Duplicant pumps liquid from a pool into bottles. Automation dispenses bottled liquid onto the platform for Auto-Sweepers when liquid is available." }
        };

        /// <summary>Per building help texts (ChineseSimplified).</summary>
        internal static readonly Dictionary<string, string> DescriptionsChineseSimplified = new Dictionary<string, string>
        {
            { "BUILDINGDESC.FABRICATEDWOODMAKER", "原版：由复制人把植物纤维与树脂压制成胶合板。自动化在电力与两种原料齐备时自行运行配方队列。" },
            { "BUILDINGDESC.POWERCONTROLSTATION", "原版：需要具备电力技工特长的复制人在发电站房间内用精炼金属制作微型芯片。自动化无需复制人即可制作；房间与技能要求可在下方单独忽略。" },
            { "BUILDINGDESC.MANUALGENERATOR", "原版：复制人踩踏发电轮，输出 400 W 电力并消耗体力。自动化仅在所连电路仍需要电力时自行转动，电路上所有电池充满后停止。" },
            { "BUILDINGDESC.TELESCOPE", "原版：复制人观测已选定的太空目标以完成解析。自动化仅在已指派有效可解析目标时运行，无目标时绝不驱动望远镜。" },
            { "BUILDINGDESC.CLUSTERTELESCOPE", "原版（太空探索）：复制人在地表解析星图目标。自动化只解析当前已指派的目标，未选择目标时保持空闲。" },
            { "BUILDINGDESC.CLUSTERTELESCOPEENCLOSED", "原版（太空探索）：无需直接暴露天空的封闭版本。自动化行为与开放式望远镜一致，同样要求已指派目标。" },
            { "BUILDINGDESC.MANUALHIGHENERGYPARTICLESPAWNER", "原版：复制人摇动手柄发射辐射粒子并消耗体力。自动化在粒子缓存未满时自行摇动，并播放原版工作动画。" },
            { "BUILDINGDESC.RESETSKILLSSTATION", "原版：复制人在此重置并返还已学技能点。自动化仅代为完成操作工作，技能重置本身仍需玩家指派复制人。" },
            { "BUILDINGDESC.APOTHECARY", "原版：复制人用送达的材料制作药物。自动化在电力与材料充足时自行运行配方队列。" },
            { "BUILDINGDESC.ADVANCEDAPOTHECARY", "原版：用送达材料制作高级药物与辐射治疗物。自动化无需复制人即可运行其配方队列。" },
            { "BUILDINGDESC.SUSHIBAR", "原版：复制人用送达的食材制作生鱼料理。自动化自行运行配方队列。" },
            { "BUILDINGDESC.ICEKETTLE", "原版：燃烧木材将送入的冰融化为水，成品需复制人搬出。自动化只释放成品液体仓，绝不倾倒木材或冰的输入仓。" },
            { "BUILDINGDESC.CAMPFIRE", "原版：复制人添柴燃烧木材以加热周围环境。自动化在存有燃料且建筑启用时自行添柴。" },
            { "BUILDINGDESC.ICECOOLEDFAN", "原版：复制人操作风扇，消耗冰块为区域降温。自动化在存有冰块时自行操作。" },
            { "BUILDINGDESC.COMPOST", "原版：复制人定期翻堆，将污染泥土转化为泥土。自动化在批次就绪时按原版周期翻堆，并播放原版动画。" },
            { "BUILDINGDESC.FOODDEHYDRATOR", "原版：燃烧木材脱水食物，成品脱水包需复制人清空。自动化同时运行配方队列并自动释放输出仓中的成品。" },
            { "BUILDINGDESC.ADVANCEDRESEARCHCENTER", "原版：复制人在此研究高级科技，消耗水与电力。自动化仅在当前研究目标确实需要此站点研究点时才进行研究。" },
            { "BUILDINGDESC.COSMICRESEARCHCENTER", "原版：复制人把收集到的太空数据转换为轨道研究点。自动化仅在存在数据且有匹配研究目标时运行。" },
            { "BUILDINGDESC.DLC1COSMICRESEARCHCENTER", "原版（太空探索）：将收集的数据库转换为轨道研究。自动化要求已存有数据且存在有效研究目标。" },
            { "BUILDINGDESC.NUCLEARRESEARCHCENTER", "原版：复制人研究放射性材料以获得核研究点。自动化仅在当前研究目标消耗该类研究点时进行。" },
            { "BUILDINGDESC.ORBITALRESEARCHCENTER", "原版：复制人处理从太空带回的轨道数据库。自动化在存有数据库且有匹配研究目标时自动处理。" },
            { "BUILDINGDESC.GENETICANALYSISSTATION", "原版：复制人分析植物种子或变异以解锁其信息。自动化仅在已装入有效样本时进行分析。" },
            { "BUILDINGDESC.MORBROVERMAKER", "原版：复制人用送达材料组装莫布机器人。自动化无需复制人即可运行其配方队列。" },
            { "BUILDINGDESC.MISSIONCONTROL", "原版：位于合规房间的复制人引导范围内的火箭，提供飞行速度加成。自动化仅在范围内确实存在火箭时提供同样的原版加成。" },
            { "BUILDINGDESC.MISSIONCONTROLCLUSTER", "原版（太空探索）：有人值守的指挥站，加速星团内的火箭。自动化只对该站自身报告在范围内的火箭施加原版加成。" },
            { "BUILDINGDESC.FARMSTATION", "原版：位于农场房间的复制人生产微量营养肥料用于照料作物。自动化按原版周期生产；另有选项允许在当前无作物需求时继续生产。" },
            { "BUILDINGDESC.SPICEGRINDER", "原版：复制人将送达材料研磨成食物香料。自动化在电力与材料充足时运行其配方队列。" },
            { "BUILDINGDESC.COOKINGSTATION", "原版：复制人用电力烹饪食物配方。自动化无需厨师即可完成队列中的配方。" },
            { "BUILDINGDESC.GOURMETCOOKINGSTATION", "原版：复制人使用天然气烹饪高级食物。自动化自行完成队列中的配方。" },
            { "BUILDINGDESC.MICROBEMUSHER", "原版：复制人制作糊糊棒等基础食物。自动化无需厨师即可完成队列配方。" },
            { "BUILDINGDESC.DEEPFRYER", "原版：复制人用油与电力油炸食物。自动化自行完成队列配方。" },
            { "BUILDINGDESC.MILKPRESS", "原版：复制人将植物压榨成奶与副产物。自动化在电力与植物充足时运行配方队列。" },
            { "BUILDINGDESC.SMOKER", "原版：燃烧木材熏制鱼类，成品需复制人取出。自动化运行配方并一次性释放成品，并有防止重复释放的保护。" },
            { "BUILDINGDESC.ROCKCRUSHER", "原版：复制人将岩石与金属矿粉碎为精炼产物。自动化无需复制人即可运行配方队列。" },
            { "BUILDINGDESC.METALREFINERY", "原版：复制人使用液体冷却剂精炼金属矿，冷却剂吸收热量。自动化在矿石与冷却剂充足时运行配方队列。" },
            { "BUILDINGDESC.GLASSFORGE", "原版：复制人将沙子熔炼为熔融玻璃。自动化自行运行配方队列。" },
            { "BUILDINGDESC.SUPERMATERIALREFINERY", "原版：复制人制作超导材料等高级材料。自动化无需复制人即可运行配方队列。" },
            { "BUILDINGDESC.SUITFABRICATOR", "原版：复制人用送达材料制造与修理外骨骼服。自动化自行运行配方队列。" },
            { "BUILDINGDESC.CLOTHINGFABRICATOR", "原版：复制人将芦苇纤维织成衣物。自动化无需复制人即可运行配方队列。" },
            { "BUILDINGDESC.CLOTHINGALTERATIONSTATION", "原版：复制人将衣物改造为其他服装。自动化自行运行配方队列。" },
            { "BUILDINGDESC.CRAFTINGTABLE", "原版：具有艺术技能的复制人雕刻装饰雕塑。自动化代为完成制作工作，品质仍遵循原版规则。" },
            { "BUILDINGDESC.ADVANCEDCRAFTINGTABLE", "原版：复制人组装自动化与电子元件。自动化无需复制人即可运行配方队列。" },
            { "BUILDINGDESC.SLUDGEPRESS", "原版：复制人将污染泥浆压滤为水与泥土。自动化自行运行配方队列。" },
            { "BUILDINGDESC.DIAMONDPRESS", "原版：复制人将精炼碳压制成钻石。自动化无需复制人即可运行配方队列。" },
            { "BUILDINGDESC.CHEMICALREFINERY", "原版：复制人生产以乙醇为基础的化学产物。自动化在电力与原料充足时运行配方队列。" },
            { "BUILDINGDESC.MISSILEFABRICATOR", "原版：复制人用送达材料组装爆破弹药。自动化无需复制人即可运行配方队列。" },
            { "BUILDINGDESC.DATAMINER", "原版：复制人从存储的数据库中提取研究数据。自动化在存有数据时自行提取。" },
            { "BUILDINGDESC.OILREFINERY", "原版：复制人将 10 kg/s 原油精炼为 5 kg/s 石油与少量天然气。自动化无需复制人即可运行；比例选项可恢复旧版 100% 转化。" },
            { "BUILDINGDESC.OILWELLCAP", "原版：注水抽取原油，积聚的气压需复制人手动释放。自动化在达到阈值时自动释放压力。" },
            { "BUILDINGDESC.DESALINATOR", "原版：将盐水转化为水，残留的盐需复制人搬出。自动化仅释放堆积的盐产物。" },
            { "BUILDINGDESC.MILKFATSEPARATOR", "原版：将奶分离为脂肪与水，固体产物需复制人清空。自动化仅释放已完成的固体产物。" },
            { "BUILDINGDESC.GEOTUNER", "原版：科学类复制人对目标地热喷口进行调谐，在限定时间内提升其产量。自动化对已指派的喷口执行同样的调谐周期，并遵守原版冷却。" },
            { "BUILDINGDESC.VALVE", "原版：修改流量后需要复制人前往手动调节阀门，新数值才会生效。自动化立即应用新的流量并取消待处理的阀门任务。" },
            { "BUILDINGDESC.RESEARCHCENTER", "原版：由复制人在研究站进行研究，将研究材料转化为研究点数。自动化在存在对应课题与研究材料时自行运转。" },
            { "BUILDINGDESC.RANCHSTATION", "原版：牧场主梳理小动物，赋予“已梳理”效果以提升繁殖。自动化沿用原版牧场状态机，小动物会自行入站，双方动画正常播放。" },
            { "BUILDINGDESC.SHEARINGSTATION", "原版：牧场主修剪毛毛虫获得芦苇纤维或塑料。自动化驱动原版修剪周期，包含小动物动画与冷却。" },
            { "BUILDINGDESC.MILKINGSTATION", "原版：牧场主为小动物挤奶以产出液体。自动化执行原版挤奶周期，保留小动物动画与冷却。" },
            { "BUILDINGDESC.UNDERWATERRANCHSTATION", "原版（水生包）：在水下站点梳理游泳类小动物。自动化使用与陆地版本相同的原版牧场状态机。" },
            { "BUILDINGDESC.UNDERWATERSHEARINGSTATION", "原版（水生包）：修剪水生小动物以获得纤维产物。自动化驱动与陆地版本相同的原版修剪周期。" },
            { "BUILDINGDESC.UNDERWATERMILKINGSTATION", "原版（水生包）：为水生小动物挤奶以产出液体。自动化执行与陆地版本相同的原版挤奶周期。" },
            { "BUILDINGDESC.LIQUIDBOTTLER", "原版：将管道输入的液体装瓶供手动搬运。自动化在装满后自动释放瓶装液体，供自动清扫器搬运。" },
            { "BUILDINGDESC.GASBOTTLER", "原版：将管道输入的气体充罐供手动搬运。自动化在充满后自动释放罐装气体，供自动清扫器搬运。" },
            { "BUILDINGDESC.LIQUIDPUMPINGSTATION", "原版：复制人操作压水泵从下方水池抽取瓶装水。自动化在存有可用液体时自动生成瓶装液体并置于平台上，供自动清扫器搬运。" }
        };

        /// <summary>Per building help texts (ChineseTraditional).</summary>
        internal static readonly Dictionary<string, string> DescriptionsChineseTraditional = new Dictionary<string, string>
        {
            { "BUILDINGDESC.FABRICATEDWOODMAKER", "原版：由複製人把植物纖維與樹脂壓製成膠合板。自動化在電力與兩種原料齊備時自行執行配方佇列。" },
            { "BUILDINGDESC.POWERCONTROLSTATION", "原版：需要具備電力技工專長的複製人在發電站房間內以精煉金屬製作微型晶片。自動化無需複製人即可製作；房間與技能需求可於下方單獨忽略。" },
            { "BUILDINGDESC.MANUALGENERATOR", "原版：複製人踩踏發電輪，輸出 400 W 電力並消耗體力。自動化僅在所連電路仍需要電力時自行轉動，電路上所有電池充滿後停止。" },
            { "BUILDINGDESC.TELESCOPE", "原版：複製人觀測已選定的太空目標以完成解析。自動化僅在已指派有效可解析目標時執行，無目標時絕不驅動望遠鏡。" },
            { "BUILDINGDESC.CLUSTERTELESCOPE", "原版（太空探索）：複製人在地表解析星圖目標。自動化只解析當前已指派的目標，未選擇目標時保持空閒。" },
            { "BUILDINGDESC.CLUSTERTELESCOPEENCLOSED", "原版（太空探索）：無需直接暴露天空的封閉版本。自動化行為與開放式望遠鏡一致，同樣要求已指派目標。" },
            { "BUILDINGDESC.MANUALHIGHENERGYPARTICLESPAWNER", "原版：複製人搖動手柄發射輻射粒子並消耗體力。自動化在粒子快取未滿時自行搖動，並播放原版工作動畫。" },
            { "BUILDINGDESC.RESETSKILLSSTATION", "原版：複製人在此重置並返還已學技能點。自動化僅代為完成操作工作，技能重置本身仍需玩家指派複製人。" },
            { "BUILDINGDESC.APOTHECARY", "原版：複製人用送達的材料製作藥物。自動化在電力與材料充足時自行執行配方佇列。" },
            { "BUILDINGDESC.ADVANCEDAPOTHECARY", "原版：用送達材料製作高階藥物與輻射治療物。自動化無需複製人即可執行其配方佇列。" },
            { "BUILDINGDESC.SUSHIBAR", "原版：複製人用送達的食材製作生魚料理。自動化自行執行配方佇列。" },
            { "BUILDINGDESC.ICEKETTLE", "原版：燃燒木材將送入的冰融化為水，成品需複製人搬出。自動化只釋放成品液體倉，絕不傾倒木材或冰的輸入倉。" },
            { "BUILDINGDESC.CAMPFIRE", "原版：複製人添柴燃燒木材以加熱周圍環境。自動化在存有燃料且建築啟用時自行添柴。" },
            { "BUILDINGDESC.ICECOOLEDFAN", "原版：複製人操作風扇，消耗冰塊為區域降溫。自動化在存有冰塊時自行操作。" },
            { "BUILDINGDESC.COMPOST", "原版：複製人定期翻堆，將汙染泥土轉化為泥土。自動化在批次就緒時按原版週期翻堆，並播放原版動畫。" },
            { "BUILDINGDESC.FOODDEHYDRATOR", "原版：燃燒木材脫水食物，成品脫水包需複製人清空。自動化同時執行配方佇列並自動釋放輸出倉中的成品。" },
            { "BUILDINGDESC.ADVANCEDRESEARCHCENTER", "原版：複製人在此研究高階科技，消耗水與電力。自動化僅在當前研究目標確實需要此站點研究點時才進行研究。" },
            { "BUILDINGDESC.COSMICRESEARCHCENTER", "原版：複製人把收集到的太空資料轉換為軌道研究點。自動化僅在存在資料且有匹配研究目標時執行。" },
            { "BUILDINGDESC.DLC1COSMICRESEARCHCENTER", "原版（太空探索）：將收集的資料庫轉換為軌道研究。自動化要求已存有資料且存在有效研究目標。" },
            { "BUILDINGDESC.NUCLEARRESEARCHCENTER", "原版：複製人研究放射性材料以獲得核研究點。自動化僅在當前研究目標消耗該類研究點時進行。" },
            { "BUILDINGDESC.ORBITALRESEARCHCENTER", "原版：複製人處理從太空帶回的軌道資料庫。自動化在存有資料庫且有匹配研究目標時自動處理。" },
            { "BUILDINGDESC.GENETICANALYSISSTATION", "原版：複製人分析植物種子或變異以解鎖其資訊。自動化僅在已裝入有效樣本時進行分析。" },
            { "BUILDINGDESC.MORBROVERMAKER", "原版：複製人用送達材料組裝莫布機器人。自動化無需複製人即可執行其配方佇列。" },
            { "BUILDINGDESC.MISSIONCONTROL", "原版：位於合規房間的複製人引導範圍內的火箭，提供飛行速度加成。自動化僅在範圍內確實存在火箭時提供同樣的原版加成。" },
            { "BUILDINGDESC.MISSIONCONTROLCLUSTER", "原版（太空探索）：有人值守的指揮站，加速星團內的火箭。自動化只對該站自身報告在範圍內的火箭施加原版加成。" },
            { "BUILDINGDESC.FARMSTATION", "原版：位於農場房間的複製人生產微量營養肥料用於照料作物。自動化按原版週期生產；另有選項允許在當前無作物需求時繼續生產。" },
            { "BUILDINGDESC.SPICEGRINDER", "原版：複製人將送達材料研磨成食物香料。自動化在電力與材料充足時執行其配方佇列。" },
            { "BUILDINGDESC.COOKINGSTATION", "原版：複製人用電力烹飪食物配方。自動化無需廚師即可完成佇列中的配方。" },
            { "BUILDINGDESC.GOURMETCOOKINGSTATION", "原版：複製人使用天然氣烹飪高階food。自動化自行完成佇列中的配方。" },
            { "BUILDINGDESC.MICROBEMUSHER", "原版：複製人制作糊糊棒等基礎食物。自動化無需廚師即可完成佇列配方。" },
            { "BUILDINGDESC.DEEPFRYER", "原版：複製人用油與電力油炸食物。自動化自行完成佇列配方。" },
            { "BUILDINGDESC.MILKPRESS", "原版：複製人將植物壓榨成奶與副產物。自動化在電力與植物充足時執行配方佇列。" },
            { "BUILDINGDESC.SMOKER", "原版：燃燒木材熏製魚類，成品需複製人取出。自動化執行配方並一次性釋放成品，並有防止重複釋放的保護。" },
            { "BUILDINGDESC.ROCKCRUSHER", "原版：複製人將岩石與金屬礦粉碎為精煉產物。自動化無需複製人即可執行配方佇列。" },
            { "BUILDINGDESC.METALREFINERY", "原版：複製人使用液體冷卻劑精煉金屬礦，冷卻劑吸收熱量。自動化在礦石與冷卻劑充足時執行配方佇列。" },
            { "BUILDINGDESC.GLASSFORGE", "原版：複製人將沙子熔鍊為熔融玻璃。自動化自行執行配方佇列。" },
            { "BUILDINGDESC.SUPERMATERIALREFINERY", "原版：複製人制作超導材料等高階材料。自動化無需複製人即可執行配方佇列。" },
            { "BUILDINGDESC.SUITFABRICATOR", "原版：複製人用送達材料製造與修理外骨骼服。自動化自行執行配方佇列。" },
            { "BUILDINGDESC.CLOTHINGFABRICATOR", "原版：複製人將蘆葦纖維織成衣物。自動化無需複製人即可執行配方佇列。" },
            { "BUILDINGDESC.CLOTHINGALTERATIONSTATION", "原版：複製人將衣物改造為其他服裝。自動化自行執行配方佇列。" },
            { "BUILDINGDESC.CRAFTINGTABLE", "原版：具有藝術技能的複製人雕刻裝飾雕塑。自動化代為完成製作工作，品質仍遵循原版規則。" },
            { "BUILDINGDESC.ADVANCEDCRAFTINGTABLE", "原版：複製人組裝自動化與電子元件。自動化無需複製人即可執行配方佇列。" },
            { "BUILDINGDESC.SLUDGEPRESS", "原版：複製人將汙染泥漿壓濾為水與泥土。自動化自行執行配方佇列。" },
            { "BUILDINGDESC.DIAMONDPRESS", "原版：複製人將精煉碳壓制成鑽石。自動化無需複製人即可執行配方佇列。" },
            { "BUILDINGDESC.CHEMICALREFINERY", "原版：複製人生產以乙醇為基礎的化學產物。自動化在電力與原料充足時執行配方佇列。" },
            { "BUILDINGDESC.MISSILEFABRICATOR", "原版：複製人用送達材料組裝爆破彈藥。自動化無需複製人即可執行配方佇列。" },
            { "BUILDINGDESC.DATAMINER", "原版：複製人從儲存的資料庫中提取研究資料。自動化在存有資料時自行提取。" },
            { "BUILDINGDESC.OILREFINERY", "原版：複製人將 10 kg/s 原油精煉為 5 kg/s 石油與少量天然氣。自動化無需複製人即可執行；比例選項可恢復舊版 100% 轉化。" },
            { "BUILDINGDESC.OILWELLCAP", "原版：注水抽取原油，積聚的氣壓需複製人手動釋放。自動化在達到閾值時自動釋放壓力。" },
            { "BUILDINGDESC.DESALINATOR", "原版：將鹽水轉化為水，殘留的鹽需複製人搬出。自動化僅釋放堆積的鹽產物。" },
            { "BUILDINGDESC.MILKFATSEPARATOR", "原版：將奶分離為脂肪與水，固體產物需複製人清空。自動化僅釋放已完成的固體產物。" },
            { "BUILDINGDESC.GEOTUNER", "原版：科學類複製人對目標地熱噴口進行調諧，在限定時間內提升其產量。自動化對已指派的噴口執行同樣的調諧週期，並遵守原版冷卻。" },
            { "BUILDINGDESC.VALVE", "原版：修改流量後需要複製人前往手動調節閥門，新數值才會生效。自動化立即套用新的流量並取消待處理的閥門任務。" },
            { "BUILDINGDESC.RESEARCHCENTER", "原版：由複製人在研究站進行研究，將研究材料轉化為研究點數。自動化在存在對應課題與研究材料時自行運轉。" },
            { "BUILDINGDESC.RANCHSTATION", "原版：牧場主梳理小動物，賦予“已梳理”效果以提升繁殖。自動化沿用原版牧場狀態機，小動物會自行入站，雙方動畫正常播放。" },
            { "BUILDINGDESC.SHEARINGSTATION", "原版：牧場主修剪毛毛蟲獲得蘆葦纖維或塑膠。自動化驅動原版修剪週期，包含小動物動畫與冷卻。" },
            { "BUILDINGDESC.MILKINGSTATION", "原版：牧場主為小動物擠奶以產出液體。自動化執行原版擠奶週期，保留小動物動畫與冷卻。" },
            { "BUILDINGDESC.UNDERWATERRANCHSTATION", "原版（水生包）：在水下站點梳理游泳類小動物。自動化使用與陸地版本相同的原版牧場狀態機。" },
            { "BUILDINGDESC.UNDERWATERSHEARINGSTATION", "原版（水生包）：修剪水生小動物以獲得纖維產物。自動化驅動與陸地版本相同的原版修剪週期。" },
            { "BUILDINGDESC.UNDERWATERMILKINGSTATION", "原版（水生包）：為水生小動物擠奶以產出液體。自動化執行與陸地版本相同的原版擠奶週期。" },
            { "BUILDINGDESC.LIQUIDBOTTLER", "原版：將管道輸入的液體裝瓶供手動搬運。自動化在裝滿後自動釋放瓶裝液體，供自動清掃器搬運。" },
            { "BUILDINGDESC.GASBOTTLER", "原版：將管道輸入的氣體充罐供手動搬運。自動化在充滿後自動釋放罐裝氣體，供自動清掃器搬運。" },
            { "BUILDINGDESC.LIQUIDPUMPINGSTATION", "原版：複製人操作壓水泵從下方水池抽取瓶裝水。自動化在存有可用液體時自動生成瓶裝液體並置於平台上，供自動清掃器搬運。" }
        };

        /// <summary>Per building help texts (Korean).</summary>
        internal static readonly Dictionary<string, string> DescriptionsKorean = new Dictionary<string, string>
        {
            { "BUILDINGDESC.FABRICATEDWOODMAKER", "기본: 복제체가 식물 섬유와 수지를 눌러 합판을 만듭니다. 자동화는 전력과 두 재료가 있으면 스스로 제작 대기열을 진행합니다." },
            { "BUILDINGDESC.POWERCONTROLSTATION", "기본: 전력 정비 특성을 가진 복제체가 발전소 방에서 정제 금속으로 마이크로칩을 만듭니다. 자동화는 복제체 없이 제작하며, 방과 기술 조건은 아래에서 따로 무시할 수 있습니다." },
            { "BUILDINGDESC.MANUALGENERATOR", "바닐라: 복제인이 발전 바퀴를 돌려 400W를 생산합니다. 자동화는 연결된 회로에 전력이 필요할 때만 스스로 돌리고, 모든 배터리가 가득 차면 멈춥니다." },
            { "BUILDINGDESC.TELESCOPE", "바닐라: 복제인이 선택한 우주 목표를 분석해 밝혀냅니다. 자동화는 유효한 분석 대상이 지정된 경우에만 작동하며 대상이 없으면 건드리지 않습니다." },
            { "BUILDINGDESC.CLUSTERTELESCOPE", "바닐라(Spaced Out!): 복제인이 지표에서 성도 목표를 분석합니다. 자동화는 지정된 목표만 분석하고 선택이 없으면 대기합니다." },
            { "BUILDINGDESC.CLUSTERTELESCOPEENCLOSED", "바닐라(Spaced Out!): 하늘 노출이 필요 없는 밀폐형입니다. 자동화는 일반 망원경과 동일하며 지정된 대상이 필요합니다." },
            { "BUILDINGDESC.MANUALHIGHENERGYPARTICLESPAWNER", "바닐라: 복제인이 손잡이를 돌려 라드볼트를 방출합니다. 자동화는 버퍼에 여유가 있을 때 바닐라 작업 애니메이션과 함께 돌립니다." },
            { "BUILDINGDESC.RESETSKILLSSTATION", "바닐라: 복제인이 습득한 기술 점수를 환원합니다. 자동화는 조작 작업만 대신하며 초기화 자체는 복제인 지정이 필요합니다." },
            { "BUILDINGDESC.APOTHECARY", "바닐라: 복제인이 배달된 재료로 약을 만듭니다. 자동화는 전력과 재료가 있으면 제작 대기열을 스스로 처리합니다." },
            { "BUILDINGDESC.ADVANCEDAPOTHECARY", "바닐라: 배달된 재료로 고급 약과 방사선 치료제를 만듭니다. 자동화는 복제인 없이 제작 대기열을 처리합니다." },
            { "BUILDINGDESC.SUSHIBAR", "바닐라: 복제인이 배달된 식재료로 생선 요리를 만듭니다. 자동화가 제작 대기열을 스스로 처리합니다." },
            { "BUILDINGDESC.ICEKETTLE", "바닐라: 목재를 태워 얼음을 물로 녹이며 완성된 액체는 복제인이 옮깁니다. 자동화는 완성 액체 보관만 배출하고 목재나 얼음 투입물은 건드리지 않습니다." },
            { "BUILDINGDESC.CAMPFIRE", "바닐라: 복제인이 장작을 넣어 주변을 데웁니다. 자동화는 연료가 있고 건물이 활성일 때 스스로 넣습니다." },
            { "BUILDINGDESC.ICECOOLEDFAN", "바닐라: 복제인이 선풍기를 조작해 얼음을 소모하며 냉각합니다. 자동화는 얼음이 있으면 스스로 조작합니다." },
            { "BUILDINGDESC.COMPOST", "바닐라: 복제인이 주기적으로 퇴비를 뒤집어 오염된 흙을 흙으로 바꿉니다. 자동화는 배치가 준비되면 바닐라 주기로 뒤집고 애니메이션도 재생합니다." },
            { "BUILDINGDESC.FOODDEHYDRATOR", "바닐라: 목재를 태워 식품을 건조하고 완성품은 복제인이 비웁니다. 자동화는 제작 대기열 처리와 출력 보관의 완성품 배출을 모두 수행합니다." },
            { "BUILDINGDESC.ADVANCEDRESEARCHCENTER", "바닐라: 복제인이 물과 전력을 소모해 고급 연구를 수행합니다. 자동화는 현재 연구 목표가 이 시설의 연구 점수를 필요로 할 때만 작동합니다." },
            { "BUILDINGDESC.COSMICRESEARCHCENTER", "바닐라: 복제인이 수집한 우주 데이터를 궤도 연구 점수로 변환합니다. 자동화는 데이터와 해당 연구 목표가 있을 때만 작동합니다." },
            { "BUILDINGDESC.DLC1COSMICRESEARCHCENTER", "바닐라(Spaced Out!): 수집한 데이터 뱅크를 궤도 연구로 변환합니다. 자동화에는 데이터와 유효한 연구 목표가 필요합니다." },
            { "BUILDINGDESC.NUCLEARRESEARCHCENTER", "바닐라: 복제인이 방사성 물질을 연구해 핵 연구 점수를 얻습니다. 자동화는 연구 목표가 해당 점수를 소모할 때만 작동합니다." },
            { "BUILDINGDESC.ORBITALRESEARCHCENTER", "바닐라: 복제인이 우주에서 가져온 궤도 데이터 뱅크를 처리합니다. 자동화는 데이터 뱅크와 대응 연구 목표가 있을 때 처리합니다." },
            { "BUILDINGDESC.GENETICANALYSISSTATION", "바닐라: 복제인이 씨앗이나 돌연변이를 분석해 정보를 해금합니다. 자동화는 유효한 표본이 있을 때만 분석합니다." },
            { "BUILDINGDESC.MORBROVERMAKER", "바닐라: 복제인이 배달된 재료로 모브 로버를 조립합니다. 자동화는 복제인 없이 제작 대기열을 처리합니다." },
            { "BUILDINGDESC.MISSIONCONTROL", "바닐라: 규정 방의 복제인이 범위 내 로켓을 유도해 비행 속도 보너스를 줍니다. 자동화는 실제 로켓이 범위에 있을 때만 동일한 보너스를 부여합니다." },
            { "BUILDINGDESC.MISSIONCONTROLCLUSTER", "바닐라(Spaced Out!): 유인 임무 통제소가 성단 내 로켓을 가속합니다. 자동화는 시설이 범위 내로 보고한 로켓에만 보너스를 적용합니다." },
            { "BUILDINGDESC.FARMSTATION", "바닐라: 농장 방의 복제인이 식물 관리를 위한 미량 영양 비료를 생산합니다. 자동화는 바닐라 주기로 생산하며, 옵션으로 수요가 없어도 계속 생산할 수 있습니다." },
            { "BUILDINGDESC.SPICEGRINDER", "바닐라: 복제인이 배달된 재료를 갈아 향신료를 만듭니다. 자동화는 전력과 재료가 있을 때 대기열을 처리합니다." },
            { "BUILDINGDESC.COOKINGSTATION", "바닐라: 복제인이 전기로 요리를 만듭니다. 자동화는 요리사 없이 대기열 레시피를 처리합니다." },
            { "BUILDINGDESC.GOURMETCOOKINGSTATION", "바닐라: 복제인이 천연가스로 고급 요리를 만듭니다. 자동화가 대기열 레시피를 스스로 처리합니다." },
            { "BUILDINGDESC.MICROBEMUSHER", "바닐라: 복제인이 머시 바 등 기본 식량을 만듭니다. 자동화는 요리사 없이 레시피를 처리합니다." },
            { "BUILDINGDESC.DEEPFRYER", "바닐라: 복제인이 기름과 전력으로 음식을 튀깁니다. 자동화가 대기열 레시피를 처리합니다." },
            { "BUILDINGDESC.MILKPRESS", "바닐라: 복제인이 식물을 압착해 우유와 부산물을 만듭니다. 자동화는 전력과 식물이 있을 때 대기열을 처리합니다." },
            { "BUILDINGDESC.SMOKER", "바닐라: 목재를 태워 생선을 훈제하고 완성품은 복제인이 꺼냅니다. 자동화는 레시피를 처리하고 중복 배출을 막으며 완성품을 한 번만 배출합니다." },
            { "BUILDINGDESC.ROCKCRUSHER", "바닐라: 복제인이 암석과 광석을 분쇄해 정제물을 만듭니다. 자동화는 복제인 없이 대기열을 처리합니다." },
            { "BUILDINGDESC.METALREFINERY", "바닐라: 복제인이 액체 냉각재로 광석을 정제하며 열은 냉각재가 흡수합니다. 자동화는 광석과 냉각재가 있을 때 처리합니다." },
            { "BUILDINGDESC.GLASSFORGE", "바닐라: 복제인이 모래를 녹여 용융 유리를 만듭니다. 자동화가 대기열을 스스로 처리합니다." },
            { "BUILDINGDESC.SUPERMATERIALREFINERY", "바닐라: 복제인이 서미움 등 초소재를 제작합니다. 자동화는 복제인 없이 대기열을 처리합니다." },
            { "BUILDINGDESC.SUITFABRICATOR", "바닐라: 복제인이 배달 재료로 외골격을 제작·수리합니다. 자동화가 대기열을 처리합니다." },
            { "BUILDINGDESC.CLOTHINGFABRICATOR", "바닐라: 복제인이 갈대 섬유로 옷을 짭니다. 자동화는 복제인 없이 대기열을 처리합니다." },
            { "BUILDINGDESC.CLOTHINGALTERATIONSTATION", "바닐라: 복제인이 의류를 다른 옷으로 개조합니다. 자동화가 대기열을 처리합니다." },
            { "BUILDINGDESC.CRAFTINGTABLE", "바닐라: 예술 기술의 복제인이 장식 조각을 만듭니다. 자동화는 제작 작업을 대신하며 품질은 바닐라 규칙을 따릅니다." },
            { "BUILDINGDESC.ADVANCEDCRAFTINGTABLE", "바닐라: 복제인이 자동화·전자 부품을 조립합니다. 자동화는 복제인 없이 대기열을 처리합니다." },
            { "BUILDINGDESC.SLUDGEPRESS", "바닐라: 복제인이 오염된 진흙을 짜서 물과 흙으로 만듭니다. 자동화가 대기열을 처리합니다." },
            { "BUILDINGDESC.DIAMONDPRESS", "바닐라: 복제인이 정제 탄소를 다이아몬드로 압축합니다. 자동화는 복제인 없이 처리합니다." },
            { "BUILDINGDESC.CHEMICALREFINERY", "바닐라: 복제인이 에탄올 기반 화학 제품을 생산합니다. 자동화는 전력과 재료가 있을 때 처리합니다." },
            { "BUILDINGDESC.MISSILEFABRICATOR", "바닐라: 복제인이 배달 재료로 탄약을 조립합니다. 자동화는 복제인 없이 대기열을 처리합니다." },
            { "BUILDINGDESC.DATAMINER", "바닐라: 복제인이 보관된 데이터 뱅크에서 연구 데이터를 추출합니다. 자동화는 데이터가 있으면 추출합니다." },
            { "BUILDINGDESC.OILREFINERY", "바닐라: 복제인이 원유 10kg/s를 석유 5kg/s와 천연가스로 정제합니다. 자동화는 복제인 없이 가동되며 비율 옵션으로 구버전 100% 변환을 복원할 수 있습니다." },
            { "BUILDINGDESC.OILWELLCAP", "바닐라: 물을 주입해 원유를 뽑아내며 쌓인 가스 압력은 복제인이 빼야 합니다. 자동화는 임계치에 도달하면 자동으로 압력을 방출합니다." },
            { "BUILDINGDESC.DESALINATOR", "바닐라: 소금물을 물로 바꾸고 남은 소금은 복제인이 옮깁니다. 자동화는 쌓인 소금 산출물만 배출합니다." },
            { "BUILDINGDESC.MILKFATSEPARATOR", "바닐라: 우유를 지방과 물로 분리하며 고체 산물은 복제인이 비웁니다. 자동화는 완성된 고체 산물만 배출합니다." },
            { "BUILDINGDESC.GEOTUNER", "바닐라: 과학 복제인이 지정한 간헐천을 조율해 일정 시간 산출량을 높입니다. 자동화는 지정된 간헐천에 동일한 조율 주기를 수행하며 쿨다운을 지킵니다." },
            { "BUILDINGDESC.VALVE", "기본: 유량을 변경하면 복제인이 밸브를 조작해야 새 값이 적용됩니다. 자동화는 새 유량을 즉시 적용하고 대기 중인 밸브 작업을 취소합니다." },
            { "BUILDINGDESC.RESEARCHCENTER", "기본: 복제인이 연구소에서 연구하여 연구 자원을 연구 점수로 바꿉니다. 자동화는 해당 연구 과제와 자원이 있을 때 스스로 작동합니다." },
            { "BUILDINGDESC.RANCHSTATION", "바닐라: 목장 담당자가 크리터를 손질해 번식을 높이는 효과를 줍니다. 자동화는 바닐라 목장 상태 기계를 따르므로 크리터가 들어오고 양쪽 애니메이션이 재생됩니다." },
            { "BUILDINGDESC.SHEARINGSTATION", "바닐라: 목장 담당자가 드레코의 털을 깎아 섬유나 플라스틱을 얻습니다. 자동화는 크리터 애니메이션과 쿨다운을 포함한 바닐라 주기를 실행합니다." },
            { "BUILDINGDESC.MILKINGSTATION", "바닐라: 목장 담당자가 크리터에서 액체 산물을 짜냅니다. 자동화는 애니메이션과 쿨다운을 유지한 채 바닐라 주기를 실행합니다." },
            { "BUILDINGDESC.UNDERWATERRANCHSTATION", "바닐라(수생 팩): 수중 시설에서 헤엄치는 크리터를 손질합니다. 자동화는 지상판과 동일한 상태 기계를 사용합니다." },
            { "BUILDINGDESC.UNDERWATERSHEARINGSTATION", "바닐라(수생 팩): 수생 크리터의 털을 깎아 섬유 산물을 얻습니다. 자동화는 지상판과 동일한 주기를 실행합니다." },
            { "BUILDINGDESC.UNDERWATERMILKINGSTATION", "바닐라(수생 팩): 수생 크리터에서 액체 산물을 짜냅니다. 자동화는 지상판과 동일한 주기를 실행합니다." },
            { "BUILDINGDESC.LIQUIDBOTTLER", "바닐라: 파이프의 액체를 병에 담아 수동 운반합니다. 자동화는 가득 차면 자동으로 병을 배출하여 자동 정리기가 운반할 수 있게 합니다." },
            { "BUILDINGDESC.GASBOTTLER", "바닐라: 파이프의 기체를 캔에 담아 수동 운반합니다. 자동화는 가득 차면 자동으로 캔을 배출하여 자동 정리기가 운반할 수 있게 합니다." },
            { "BUILDINGDESC.LIQUIDPUMPINGSTATION", "바닐라: 복제인이 물 펌프를 조작해 병에 담습니다. 자동화는 액체가 있을 때 자동으로 병을 플랫폼에 배출하여 자동 정리기가 운반할 수 있게 합니다." }
        };

        /// <summary>Per building help texts (Japanese).</summary>
        internal static readonly Dictionary<string, string> DescriptionsJapanese = new Dictionary<string, string>
        {
            { "BUILDINGDESC.FABRICATEDWOODMAKER", "バニラ：複製人間が植物繊維と樹脂を圧縮して合板を作ります。自動化は電力と両方の材料がある間、レシピキューを自動で進めます。" },
            { "BUILDINGDESC.POWERCONTROLSTATION", "バニラ：電気整備の特性を持つ複製人間が、発電所の部屋で精錬金属からマイクロチップを作ります。自動化は複製人間なしで製作し、部屋条件とスキル条件は下で個別に無視できます。" },
            { "BUILDINGDESC.MANUALGENERATOR", "バニラ：複製人がホイールを回して400Wを発電します。自動化は接続回路が電力を必要とする間だけ自動で回し、全電池が満充電になると停止します。" },
            { "BUILDINGDESC.TELESCOPE", "バニラ：複製人が選択した宇宙目標を分析して解明します。自動化は有効な分析対象が設定されている場合のみ動作し、対象が無い時は一切操作しません。" },
            { "BUILDINGDESC.CLUSTERTELESCOPE", "バニラ（Spaced Out!）：複製人が地表から星図の目標を分析します。自動化は指定済みの目標のみを分析し、未選択時は待機します。" },
            { "BUILDINGDESC.CLUSTERTELESCOPEENCLOSED", "バニラ（Spaced Out!）：直接空に露出する必要のない密閉型です。自動化は通常の望遠鏡と同じ挙動で、対象の指定が必要です。" },
            { "BUILDINGDESC.MANUALHIGHENERGYPARTICLESPAWNER", "バニラ：複製人がハンドルを回してラドボルトを射出します。自動化はバッファに空きがある間、バニラの作業アニメーションを再生しながら回します。" },
            { "BUILDINGDESC.RESETSKILLSSTATION", "バニラ：複製人がスキルポイントを払い戻します。自動化は操作作業のみを代行し、リセット自体は複製人の指定が必要です。" },
            { "BUILDINGDESC.APOTHECARY", "バニラ：複製人が搬入された材料から薬を作ります。自動化は電力と材料がある間、レシピキューを自動で処理します。" },
            { "BUILDINGDESC.ADVANCEDAPOTHECARY", "バニラ：搬入材料から高度な薬や放射線治療薬を作ります。自動化は複製人なしでレシピキューを処理します。" },
            { "BUILDINGDESC.SUSHIBAR", "バニラ：複製人が搬入食材から生魚料理を作ります。自動化はレシピキューを自動で処理します。" },
            { "BUILDINGDESC.ICEKETTLE", "バニラ：木材を燃やして氷を水に溶かし、完成した液体は複製人が運び出します。自動化は完成液体の保管のみを排出し、木材や氷の投入分には触れません。" },
            { "BUILDINGDESC.CAMPFIRE", "バニラ：複製人が薪をくべて周囲を加熱します。自動化は燃料があり建物が有効な間、自動でくべます。" },
            { "BUILDINGDESC.ICECOOLEDFAN", "バニラ：複製人がファンを操作し、氷を消費して周囲を冷却します。自動化は氷がある間、自動で操作します。" },
            { "BUILDINGDESC.COMPOST", "バニラ：複製人が定期的に堆肥を切り返し、汚染土を土に変えます。自動化はバッチが揃うとバニラ周期で切り返し、アニメーションも再生します。" },
            { "BUILDINGDESC.FOODDEHYDRATOR", "バニラ：木材を燃やして食品を乾燥させ、完成品は複製人が取り出します。自動化はレシピ処理と出力保管からの完成品排出の両方を行います。" },
            { "BUILDINGDESC.ADVANCEDRESEARCHCENTER", "バニラ：複製人が水と電力を消費して高度な研究を行います。自動化は現在の研究対象がこの施設の研究ポイントを必要とする場合のみ動作します。" },
            { "BUILDINGDESC.COSMICRESEARCHCENTER", "バニラ：複製人が収集した宇宙データを軌道研究ポイントに変換します。自動化はデータと対応する研究対象がある場合のみ動作します。" },
            { "BUILDINGDESC.DLC1COSMICRESEARCHCENTER", "バニラ（Spaced Out!）：収集したデータバンクを軌道研究に変換します。自動化にはデータと有効な研究対象が必要です。" },
            { "BUILDINGDESC.NUCLEARRESEARCHCENTER", "バニラ：複製人が放射性物質を研究し、核研究ポイントを獲得します。自動化は研究対象がそのポイントを消費する場合のみ動作します。" },
            { "BUILDINGDESC.ORBITALRESEARCHCENTER", "バニラ：複製人が宇宙から持ち帰った軌道データバンクを処理します。自動化はデータバンクと対応する研究対象がある間、自動処理します。" },
            { "BUILDINGDESC.GENETICANALYSISSTATION", "バニラ：複製人が種子や変異を分析して情報を解放します。自動化は有効なサンプルが装填されている場合のみ分析します。" },
            { "BUILDINGDESC.MORBROVERMAKER", "バニラ：複製人が搬入材料からモーブローバーを組み立てます。自動化は複製人なしでレシピキューを処理します。" },
            { "BUILDINGDESC.MISSIONCONTROL", "バニラ：規定の部屋にいる複製人が範囲内のロケットを誘導し、飛行速度ボーナスを与えます。自動化は実際にロケットが範囲内にある場合のみ同じボーナスを与えます。" },
            { "BUILDINGDESC.MISSIONCONTROLCLUSTER", "バニラ（Spaced Out!）：有人のミッションコントロールがクラスター内のロケットを加速します。自動化は施設自身が範囲内と報告したロケットにのみボーナスを適用します。" },
            { "BUILDINGDESC.FARMSTATION", "バニラ：農場部屋の複製人が植物の世話に使う微量栄養肥料を生産します。自動化はバニラ周期で生産し、追加オプションで需要が無くても生産を続けられます。" },
            { "BUILDINGDESC.SPICEGRINDER", "バニラ：複製人が搬入材料を挽いて香辛料を作ります。自動化は電力と材料がある間、レシピキューを処理します。" },
            { "BUILDINGDESC.COOKINGSTATION", "バニラ：複製人が電力で料理を作ります。自動化は料理人なしでキューのレシピを処理します。" },
            { "BUILDINGDESC.GOURMETCOOKINGSTATION", "バニラ：複製人が天然ガスで高級料理を作ります。自動化はキューのレシピを自動処理します。" },
            { "BUILDINGDESC.MICROBEMUSHER", "バニラ：複製人がマッシュバーなどの基本食料を作ります。自動化は料理人なしでレシピを処理します。" },
            { "BUILDINGDESC.DEEPFRYER", "バニラ：複製人が油と電力で食品を揚げます。自動化はキューのレシピを自動処理します。" },
            { "BUILDINGDESC.MILKPRESS", "バニラ：複製人が植物を搾ってミルクと副産物を作ります。自動化は電力と植物がある間、レシピを処理します。" },
            { "BUILDINGDESC.SMOKER", "バニラ：木材を燃やして魚を燻製にし、完成品は複製人が取り出します。自動化はレシピを処理し、二重排出を防ぎつつ完成品を一度だけ排出します。" },
            { "BUILDINGDESC.ROCKCRUSHER", "バニラ：複製人が岩石や鉱石を粉砕して精製物にします。自動化は複製人なしでレシピを処理します。" },
            { "BUILDINGDESC.METALREFINERY", "バニラ：複製人が液体冷却材を用いて鉱石を精錬し、熱は冷却材が吸収します。自動化は鉱石と冷却材がある間、レシピを処理します。" },
            { "BUILDINGDESC.GLASSFORGE", "バニラ：複製人が砂を溶かして溶融ガラスを作ります。自動化はレシピキューを自動処理します。" },
            { "BUILDINGDESC.SUPERMATERIALREFINERY", "バニラ：複製人がサーミウム等の超素材を作ります。自動化は複製人なしでレシピを処理します。" },
            { "BUILDINGDESC.SUITFABRICATOR", "バニラ：複製人が搬入材料でエクソスーツを製造・修理します。自動化はレシピキューを自動処理します。" },
            { "BUILDINGDESC.CLOTHINGFABRICATOR", "バニラ：複製人がリード繊維を衣類に織ります。自動化は複製人なしでレシピを処理します。" },
            { "BUILDINGDESC.CLOTHINGALTERATIONSTATION", "バニラ：複製人が衣類を別の服に作り替えます。自動化はレシピキューを自動処理します。" },
            { "BUILDINGDESC.CRAFTINGTABLE", "バニラ：芸術スキルの複製人が装飾彫刻を彫ります。自動化は製作作業を代行し、品質はバニラ規則に従います。" },
            { "BUILDINGDESC.ADVANCEDCRAFTINGTABLE", "バニラ：複製人が自動化・電子部品を組み立てます。自動化は複製人なしでレシピを処理します。" },
            { "BUILDINGDESC.SLUDGEPRESS", "バニラ：複製人が汚泥を絞って水と土にします。自動化はレシピキューを自動処理します。" },
            { "BUILDINGDESC.DIAMONDPRESS", "バニラ：複製人が精製炭素をダイヤモンドに圧縮します。自動化は複製人なしでレシピを処理します。" },
            { "BUILDINGDESC.CHEMICALREFINERY", "バニラ：複製人がエタノール系の化学製品を製造します。自動化は電力と材料がある間、レシピを処理します。" },
            { "BUILDINGDESC.MISSILEFABRICATOR", "バニラ：複製人が搬入材料から弾薬を組み立てます。自動化は複製人なしでレシピを処理します。" },
            { "BUILDINGDESC.DATAMINER", "バニラ：複製人が保管データバンクから研究データを抽出します。自動化はデータがある間、自動で抽出します。" },
            { "BUILDINGDESC.OILREFINERY", "バニラ：複製人が原油10kg/sを石油5kg/sと天然ガスに精製します。自動化は複製人なしで稼働し、比率オプションで旧版100%変換に戻せます。" },
            { "BUILDINGDESC.OILWELLCAP", "バニラ：水を注入して原油を汲み上げ、溜まったガス圧は複製人が抜きます。自動化は閾値に達すると自動で減圧します。" },
            { "BUILDINGDESC.DESALINATOR", "バニラ：塩水を水に変え、残った塩は複製人が運び出します。自動化は蓄積した塩の産出のみを排出します。" },
            { "BUILDINGDESC.MILKFATSEPARATOR", "バニラ：ミルクを脂肪と水に分離し、固形産物は複製人が取り出します。自動化は完成した固形産物のみを排出します。" },
            { "BUILDINGDESC.GEOTUNER", "バニラ：科学系の複製人が対象の間欠泉を調整し、一定時間産出量を高めます。自動化は指定された間欠泉に同じ調整サイクルを行い、バニラのクールダウンを守ります。" },
            { "BUILDINGDESC.VALVE", "通常：流量を変更しても、複製人がバルブを操作するまで新しい値は適用されません。自動化は新しい流量を即座に適用し、保留中のバルブ作業を取り消します。" },
            { "BUILDINGDESC.RESEARCHCENTER", "通常：複製人が研究ステーションで研究し、研究資源を研究ポイントに変換します。自動化は対応する研究課題と資源がある間、自動で稼働します。" },
            { "BUILDINGDESC.RANCHSTATION", "バニラ：飼育員がクリッターを手入れし、繁殖を高める効果を与えます。自動化はバニラの牧場ステートマシンに従い、クリッターが入場して双方のアニメーションが再生されます。" },
            { "BUILDINGDESC.SHEARINGSTATION", "バニラ：飼育員がドレッコを刈って繊維やプラスチックを得ます。自動化はクリッターのアニメーションとクールダウンを含むバニラの毛刈りサイクルを実行します。" },
            { "BUILDINGDESC.MILKINGSTATION", "バニラ：飼育員がクリッターから液体産物を搾ります。自動化はアニメーションとクールダウンを保ったままバニラの搾乳サイクルを実行します。" },
            { "BUILDINGDESC.UNDERWATERRANCHSTATION", "バニラ（水生パック）：水中の施設で泳ぐクリッターを手入れします。自動化は陸上版と同じステートマシンを使用します。" },
            { "BUILDINGDESC.UNDERWATERSHEARINGSTATION", "バニラ（水生パック）：水生クリッターを刈って繊維産物を得ます。自動化は陸上版と同じ毛刈りサイクルを実行します。" },
            { "BUILDINGDESC.UNDERWATERMILKINGSTATION", "バニラ（水生パック）：水生クリッターから液体産物を搾ります。自動化は陸上版と同じ搾乳サイクルを実行します。" },
            { "BUILDINGDESC.LIQUIDBOTTLER", "バニラ：配管からの液体をボトルに詰めて手動運搬します。自動化は満タンになると自動でボトルを排出し、自動掃除機が運搬できるようにします。" },
            { "BUILDINGDESC.GASBOTTLER", "バニラ：配管からの気体をキャニスターに詰めて手動運搬します。自動化は満タンになると自動でキャニスターを排出し、自動掃除機が運搬できるようにします。" },
            { "BUILDINGDESC.LIQUIDPUMPINGSTATION", "バニラ：複製人がポンプを操作して液体を汲み出します。自動化は液体がある間、自動でボトルをプラットフォームに排出し、自動掃除機が運搬できるようにします。" }
        };

        /// <summary>
        /// Japanese building names.
        ///
        /// Oxygen Not Included ships no official Japanese localization, so
        /// these names cannot be copied from a vanilla string file the way
        /// the English, Simplified Chinese, Traditional Chinese and Korean
        /// names are (see <see cref="VanillaBuildingNames"/>). They mirror the
        /// wording used by the community Japanese translation; when the game
        /// itself runs in Japanese the live game string wins over this table
        /// (see <see cref="BuildingNameBinder"/>).
        /// </summary>
        internal static readonly Dictionary<string, string> NamesJapanese = new Dictionary<string, string>
        {
            { "MANUALGENERATOR", "手動発電機" },
            { "FABRICATEDWOODMAKER", "合板プレス" },
            { "POWERCONTROLSTATION", "電力管理ステーション" },
            { "TELESCOPE", "望遠鏡" },
            { "CLUSTERTELESCOPE", "望遠鏡" },
            { "CLUSTERTELESCOPEENCLOSED", "密閉望遠鏡" },
            { "MANUALHIGHENERGYPARTICLESPAWNER", "手動ラドボルト発生器" },
            { "RESETSKILLSSTATION", "スキルスクラバー" },
            { "APOTHECARY", "調剤台" },
            { "ADVANCEDAPOTHECARY", "原子力調剤台" },
            { "SUSHIBAR", "寿司バー" },
            { "ICEKETTLE", "氷液化装置" },
            { "CAMPFIRE", "木材ヒーター" },
            { "ICECOOLEDFAN", "アイスファン" },
            { "COMPOST", "コンポスト" },
            { "FOODDEHYDRATOR", "食品乾燥機" },
            { "RESEARCHCENTER", "研究端末" },
            { "ADVANCEDRESEARCHCENTER", "スーパーコンピュータ" },
            { "COSMICRESEARCHCENTER", "バーチャルプラネタリウム" },
            { "DLC1COSMICRESEARCHCENTER", "バーチャルプラネタリウム" },
            { "NUCLEARRESEARCHCENTER", "材料研究ターミナル" },
            { "ORBITALRESEARCHCENTER", "軌道データ収集研究所" },
            { "GENETICANALYSISSTATION", "植物分析装置" },
            { "MORBROVERMAKER", "バイオボットビルダー" },
            { "MISSIONCONTROL", "ミッションコントロールステーション" },
            { "MISSIONCONTROLCLUSTER", "ミッションコントロールステーション" },
            { "FARMSTATION", "農業ステーション" },
            { "SPICEGRINDER", "スパイスグラインダー" },
            { "COOKINGSTATION", "電気グリル" },
            { "GOURMETCOOKINGSTATION", "ガスレンジ" },
            { "MICROBEMUSHER", "微生物マッシャー" },
            { "DEEPFRYER", "ディープフライヤー" },
            { "MILKPRESS", "植物粉砕機" },
            { "SMOKER", "燻製器" },
            { "ROCKCRUSHER", "岩石粉砕機" },
            { "METALREFINERY", "金属精錬器" },
            { "GLASSFORGE", "ガラス炉" },
            { "SUPERMATERIALREFINERY", "分子鍛造炉" },
            { "SUITFABRICATOR", "エクソスーツ製造機" },
            { "CLOTHINGFABRICATOR", "織機" },
            { "CLOTHINGALTERATIONSTATION", "衣類リメイク台" },
            { "CRAFTINGTABLE", "製作ステーション" },
            { "ADVANCEDCRAFTINGTABLE", "はんだ付けステーション" },
            { "SLUDGEPRESS", "スラッジプレス" },
            { "DIAMONDPRESS", "ダイヤモンドプレス" },
            { "CHEMICALREFINERY", "乳化器" },
            { "MISSILEFABRICATOR", "ブラストショット製造機" },
            { "DATAMINER", "データマイナー" },
            { "OILREFINERY", "石油精製器" },
            { "OILWELLCAP", "オイルウェル" },
            { "DESALINATOR", "脱塩装置" },
            { "MILKFATSEPARATOR", "グリーナー" },
            { "GEOTUNER", "ジオチューナー" },
            { "RANCHSTATION", "グルーミングステーション" },
            { "SHEARINGSTATION", "毛刈りステーション" },
            { "MILKINGSTATION", "搾乳ステーション" },
            { "UNDERWATERRANCHSTATION", "水生グルーミングステーション" },
            { "UNDERWATERSHEARINGSTATION", "水生毛刈りステーション" },
            { "UNDERWATERMILKINGSTATION", "水生搾乳ステーション" },
            { "LIQUIDBOTTLER", "液体ボトル充填機" },
            { "GASBOTTLER", "気体キャニスター充填機" },
            { "LIQUIDPUMPINGSTATION", "手動液体ポンプ" }
        };

        /// <summary>Returns the description table of the requested language.</summary>
        /// <param name="language">Resolved (never <see cref="UiLanguage.Auto"/>) language.</param>
        internal static Dictionary<string, string> DescriptionsFor(UiLanguage language)
        {
            switch (language)
            {
                case UiLanguage.ChineseSimplified:
                    return DescriptionsChineseSimplified;
                case UiLanguage.ChineseTraditional:
                    return DescriptionsChineseTraditional;
                case UiLanguage.Korean:
                    return DescriptionsKorean;
                case UiLanguage.Japanese:
                    return DescriptionsJapanese;
                default:
                    return DescriptionsEnglish;
            }
        }

        /// <summary>
        /// Returns the building name table of the requested language, or
        /// <c>null</c> when the game supplied names should be used.
        /// </summary>
        /// <param name="language">Resolved options language.</param>
        internal static Dictionary<string, string> NamesFor(UiLanguage language)
        {
            if (language == UiLanguage.Japanese)
            {
                return NamesJapanese;
            }

            // English, both Chinese variants and Korean are served from the
            // generated vanilla table so every label matches the wording of
            // the game itself.
            return VanillaBuildingNames.For(language);
        }
    }
}
