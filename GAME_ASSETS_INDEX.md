# 🌌 Oxygen Not Included (缺氧) 游戏本体资源与模组开发索引表
> **Game Install Root**: `E:\SteamLibrary\steamapps\common\OxygenNotIncluded`  
> **Build Target**: Unity 6000 (Unity 6 / 2022+ LTS), .NET Framework 4.8 / CLR 4.0, MonoBleedingEdge

本索引旨在为 Agent 及模组开发者提供游戏本体中所有核心程序集、物理化学数据、本地化字符串、世界生成规则、动画音效与预制体模板的定位与调用指南。

---

## 目录
1. [C# 核心程序集与引用库 (Managed Assemblies)](#1-c-核心程序集与引用库-managed-assemblies)
2. [本地化语言包与字符串字典 (Strings & Localization)](#2-本地化语言包与字符串字典-strings--localization)
3. [元素物理化学数据库 (Elements Database)](#3-元素物理化学数据库-elements-database)
4. [世界生成与星图配置 (Worldgen & DLC Clusters)](#4-世界生成与星图配置-worldgen--dlc-clusters)
5. [预制体蓝图与遗迹结构 (Templates & POIs)](#5-预制体蓝图与遗迹结构-templates--pois)
6. [百科全书与图鉴数据库 (Codex Database)](#6-百科全书与图鉴数据库-codex-database)
7. [原生底层库与调试符号 (Native Plugins & SimDLL PDB)](#7-原生底层库与调试符号-native-plugins--simdll-pdb)
8. [FMOD 音效与音频事件库 (Audio Banks)](#8-fmod-音效与音频事件库-audio-banks)

---

## 1. C# 核心程序集与引用库 (Managed Assemblies)
**路径**: `E:\SteamLibrary\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed\`

| 程序集文件名 | 大小 | 说明与开发用途 | 核心类与命名空间 |
| :--- | :--- | :--- | :--- |
| **`Assembly-CSharp.dll`** | ~14.07 MB | **游戏核心主程序集**（包含所有建筑、生物、任务、管道、电力、逻辑门、UI 屏幕、状态机等核心实现） | `ComplexFabricator`, `OilRefinery`, `SolidTransferArm`, `BuildingDef`, `Harvestable`, `Db`, `GameHashes`, `Chore`, `Workable`, `KSelectable`, `SaveGame`, `ElementLoader` |
| **`Assembly-CSharp-firstpass.dll`** | ~2.77 MB | **Klei 底层通用框架**（跨游戏复用基础结构、事件分发、动画控制器、网格索引、模组载入器） | `KMonoBehaviour`, `StateMachine<,,>`, `EventSystem`, `Grid`, `SimMessages`, `Assets`, `KBatchedAnimController`, `LocString`, `KMod.UserMod2` |
| **`0Harmony.dll`** | ~2.46 MB | 官方内置的 **Harmony v2.x 运行时补丁库** | `HarmonyLib.Harmony`, `HarmonyPatch`, `Transpiler`, `Prefix`, `Postfix` |
| **`UnityEngine.CoreModule.dll`** | ~1.98 MB | Unity 引擎核心组件与对象基类 | `GameObject`, `Component`, `Transform`, `Vector3`, `Mathf`, `Time`, `Object` |
| **`UnityEngine.UI.dll` / `UIElementsModule.dll`** | ~2.68 MB | Unity UI 与 UIElements 图形界面系统 | `Button`, `Image`, `Text`, `Canvas`, `RectTransform` |
| **`UnityEngine.InputLegacyModule.dll`** | ~45 KB | 传统 Unity 输入管理系统（处理快捷键判定） | `UnityEngine.Input`, `KeyCode` |
| **`UnityEngine.TextMeshPro.dll`** | ~446 KB | 富文本渲染与字体排版组件 | `TextMeshProUGUI`, `TMP_FontAsset` |
| **`Newtonsoft.Json.dll`** | ~465 KB | JSON 序列化/反序列化库 | `JsonConvert`, `JObject`, `JsonSerializer` |
| **`VYaml.dll`** | ~134 KB | 高性能 YAML 解析库（用于世界生成与元素定义解析） | `YamlSerializer` |
| **`ImGui.NET.dll` / `ImGui.dll`** | ~247 KB | 游戏内 Debug 浮动调试窗口库 | `ImGuiNET.ImGui` |
| **`com.rlabrecque.steamworks.net.dll`** | ~400 KB | Steamworks API 接口库（创意工坊/成就） | `SteamUGC`, `SteamUser` |

---

## 2. 本地化语言包与字符串字典 (Strings & Localization)
**路径**: `E:\SteamLibrary\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\StreamingAssets\strings\`

| 文件名 | 大小 | 说明与开发用途 |
| :--- | :--- | :--- |
| **`strings_template.pot`** | ~4.23 MB | **全游戏权威 POT 模板母表**。包含游戏本体及所有 DLC 的全部字符串键名（如 `STRINGS.BUILDINGS.PREFABS...`, `STRINGS.UI...`, `STRINGS.CREATURES...`），用于校验键名与提取官方英文字符串。 |
| **`strings_preinstalled_zh_klei.po`** | ~5.63 MB | **官方简体中文完整 PO 字典**。包含官方所有建筑、物品、状态、科技的权威中文译名。 |
| **`strings_preinstalled_ko_klei.po`** | ~7.07 MB | **官方韩语完整 PO 字典**。 |
| **`strings_preinstalled_ru_klei.po`** | ~8.12 MB | **官方俄语完整 PO 字典**。 |
| **`polib.py` / `update_from_pot.py`** | ~66 KB | Klei 官方提供的 Python 翻译提取与 PO 合并处理工具。 |

---

## 3. 元素物理化学数据库 (Elements Database)
**路径**: `E:\SteamLibrary\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\StreamingAssets\elements\`

| 文件名 | 格式 | 说明与开发用途 |
| :--- | :--- | :--- |
| **`solid.yaml`** | YAML | **全部固体元素定义**：比热容、导热系数、熔点与相变产物、硬度、辐射吸收、初始温度、材质 Tag（如 `Metal`, `RefinedMetal`, `BuildableAny`）。 |
| **`liquid.yaml`** | YAML | **全部液体元素定义**：比热容、导热系数、凝固点/沸点、粘度、光穿透率、辐射吸收率。 |
| **`gas.yaml`** | YAML | **全部气体元素定义**：比热容、摩尔质量、液化点、温室效应、辐射吸收率。 |
| **`special.yaml`** | YAML | **特殊元素定义**：真空（`Vacuum`）、不可破坏中子质（`Unobtanium`）等。 |

---

## 4. 世界生成与星图配置 (Worldgen & DLC Clusters)
**基础路径**: `E:\SteamLibrary\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\StreamingAssets\worldgen\`  
**DLC 拓展路径**: `E:\SteamLibrary\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\StreamingAssets\dlc\<dlc_id>\worldgen\`

* `dlc/expansion1/` (眼冒金星 / Spaced Out!)
* `dlc/dlc2/` (冰霜行星 / Frosty Planet Pack)
* `dlc/dlc3/` ~ `dlc/dlc5/` (未来及最新 DLC 扩展包)

| 子目录 / 文件 | 说明与开发用途 |
| :--- | :--- |
| **`worlds/`** | 各个小行星星球（Planetoid）的地表尺寸、生物群系比例与世界规则。 |
| **`clusters/`** | 多星球星系网络拓扑配置（起始星球、邻近小行星、外围深空星体分布）。 |
| **`biomes/` & `subworlds/`** | 群系地形分布（温度区间、气压基准、生成元素权重分布图）。 |
| **`storytraits/`** | 故事特质（遗迹故事线建筑生成规则，如冷冻舱、化石挖掘、地热喷口、Demolior）。 |
| **`mobs.yaml` / `rooms.yaml`** | 野生动物生成概率表及房间类型判定规则（如农场、电站、实验室的最小/最大格数与建筑需求）。 |

---

## 5. 预制体蓝图与遗迹结构 (Templates & POIs)
**基础路径**: `E:\SteamLibrary\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\StreamingAssets\templates\`  
**DLC 拓展路径**: `E:\SteamLibrary\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\StreamingAssets\dlc\<dlc_id>\templates\`

| 子目录 / 文件 | 说明与开发用途 |
| :--- | :--- |
| **`bases/`** | 初始打印胶囊基地模板。 |
| **`geysers/`** | 所有原版自然间歇泉、火山与喷孔结构蓝图。 |
| **`poi/` & `storytraits/`** | 脑波机、引力传送仪、反熵热中和器、神经重校仪、古老 Gravitas 遗迹结构。 |
| **`dev_tests/`** | 开发者调试专用测试蓝图模板。 |

---

## 6. 百科全书与图鉴数据库 (Codex Database)
**路径**: `E:\SteamLibrary\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\StreamingAssets\codex\`

| 子目录 | 说明与开发用途 |
| :--- | :--- |
| **`Buildings/`** | 所有建筑的图鉴词条、功能说明、输入/输出能耗与背景设定。 |
| **`Creatures/`** | 所有动物的饮食表、产卵周期、舒适温度与产物。 |
| **`Plants/`** | 作物与变异种子生长环境、光照/气压要求及收获周期。 |
| **`ElementTypes/`** | 元素周期表与分类图鉴。 |
| **`StoryTraits/` / `Journals/`** | 剧情日志、背景邮件与故事特质阶段指引。 |

---

## 7. 原生底层库与调试符号 (Native Plugins & SimDLL PDB)
**路径**: `E:\SteamLibrary\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Plugins\x86_64\`

| 文件名 | 大小 | 说明与开发用途 |
| :--- | :--- | :--- |
| **`SimDLL.dll`** | ~1.18 MB | C++ 原生流体与热力学模拟网格动态库（负责每 Tick 元素流动与换热）。 |
| **`SimDLL.pdb`** | ~17.67 MB | **完整 C++ 调试符号 PDB 文件**（在排查底层原生崩溃 Dump 时极其宝贵）。 |
| **`cimgui.dll`** | ~958 KB | 原生 Dear ImGui 图形接口封装库。 |
| **`fmodstudio.dll` / `resonanceaudio.dll`** | ~4.07 MB | FMOD 原生空间环绕音效引擎。 |

---

## 8. FMOD 音效与音频事件库 (Audio Banks)
**路径**: `E:\SteamLibrary\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\StreamingAssets\`

* `Buildings.bank` / `DLC*_Buildings.bank`：全部建筑运转、完成与报错音效。
* `Creatures.bank` / `DLC*_Creatures.bank`：动物叫声、移动与交互音效。
* `DuplicantActions.bank` / `DuplicantVoices.bank`：复制人扳手改装、工作、呼吸、咳嗽、音效。
* `Master Bank.strings.bank`：音频事件（FMOD Event String）全局命名路由表。

---

## 💡 模组开发速查与常用工作流

### 1. 反编译与类结构查询
* 将 `OxygenNotIncluded_Data/Managed/Assembly-CSharp.dll` 载入反编译器（如 ILSpy, dnSpy, JetBrains dotPeek）即可查看任何原版机制的内部实现代码。

### 2. 多语言译名快速检索
* 在 `OxygenNotIncluded_Data/StreamingAssets/strings/strings_template.pot` 或 `strings_preinstalled_zh_klei.po` 中搜索英文或中文名称，即可精准获得其对应的 `LocString` 常量键。

### 3. 模组工程引用路径配置
```xml
<PropertyGroup>
    <ONIManaged>E:\SteamLibrary\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed</ONIManaged>
</PropertyGroup>
<ItemGroup>
    <Reference Include="Assembly-CSharp" HintPath="$(ONIManaged)\Assembly-CSharp.dll" Private="false" />
    <Reference Include="Assembly-CSharp-firstpass" HintPath="$(ONIManaged)\Assembly-CSharp-firstpass.dll" Private="false" />
    <Reference Include="0Harmony" HintPath="$(ONIManaged)\0Harmony.dll" Private="false" />
    <Reference Include="UnityEngine.CoreModule" HintPath="$(ONIManaged)\UnityEngine.CoreModule.dll" Private="false" />
    <Reference Include="UnityEngine.InputLegacyModule" HintPath="$(ONIManaged)\UnityEngine.InputLegacyModule.dll" Private="false" />
</ItemGroup>
```
