// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using AutoMachineRebuilt.Config;
using AutoMachineRebuilt.Localization;
using AutoMachineRebuilt.UI.Components;
using AutoMachineRebuilt.UI.Model;
using AutoMachineRebuilt.Util;
using PeterHan.PLib.Options;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoMachineRebuilt.UI
{
    public class BuildingConfigEditorScreen : FScreen
    {
        public static BuildingConfigEditorScreen Instance;

        public FInputField2 FilterBar;
        public FButton ClearFilterButton;
        public FMultiSelectDropdown FilterDropDown;
        public GameObject OutlineEntryContainer;
        public GameObject OutlineEntryPrefab;

        public LocText SelectedEntryNameDisplay;
        public Image SelectedEntryPreviewImage;
        public LocText SelectedEntryDescriptionDisplay;
        public LocText SelectedEntryModOriginDisplay;

        private GameObject Details;
        private GameObject EnableBuildingContainer;
        private FToggle BuildingEnabledToggle;
        private FButton ResetSingleBuilding;
        private LocText ToggleAllButtonText;
        private FButton SaveButton;
        private LocText SaveButtonText;
        private FButton ToggleAllBuildingsButton;
        private LocText ToggleAllBuildingsButtonText;

        public BuildingConfigItem SelectedBuilding;

        private readonly Dictionary<string, BuildingConfigUIEntry> entryMap = new Dictionary<string, BuildingConfigUIEntry>(StringComparer.OrdinalIgnoreCase);
        private readonly List<GameObject> activeSubOptionRows = new List<GameObject>();
        private readonly HashSet<BuildingCategory> activeCategories = new HashSet<BuildingCategory>();
        private string currentSearchQuery = string.Empty;
        private bool initialized;

        private static readonly List<GameObject> hiddenBackgroundDialogs = new List<GameObject>();

        public static void ShowBuildingEditor(object source = null, BuildingCategory? initialCategory = null)
        {
            hiddenBackgroundDialogs.Clear();

            // Find and temporarily deactivate active options dialogs or ModMenu so they don't block the editor
            string[] backgroundDialogNames = new[] { "OptionsDialog", "ModOptions", "ModMenuDialog", "ModMenuScreen" };
            foreach (string dialogName in backgroundDialogNames)
            {
                try
                {
                    GameObject go = GameObject.Find(dialogName);
                    if (go != null && go.activeSelf)
                    {
                        go.SetActive(false);
                        hiddenBackgroundDialogs.Add(go);
                    }
                }
                catch { }
            }

            if (Instance == null)
            {
                // Safe UI parent resolution hierarchy:
                // FrontEndManager (Main Menu) -> ssOverlayCanvas (in-game HUD) -> PauseScreen -> globalCanvas
                GameObject parent = null;
                if (FrontEndManager.Instance != null && FrontEndManager.Instance.gameObject != null)
                {
                    parent = FrontEndManager.Instance.gameObject;
                }
                else if (GameScreenManager.Instance != null && GameScreenManager.Instance.ssOverlayCanvas != null)
                {
                    parent = GameScreenManager.Instance.ssOverlayCanvas;
                }
                else if (PauseScreen.Instance != null && PauseScreen.Instance.gameObject != null)
                {
                    parent = PauseScreen.Instance.gameObject;
                }
                else if (Global.Instance != null && Global.Instance.globalCanvas != null)
                {
                    parent = Global.Instance.globalCanvas;
                }

                if (parent == null)
                {
                    Log.Error("Unable to resolve suitable UI canvas parent for BuildingConfigEditorScreen");
                    return;
                }

                if (BuildingEditorAssets.BuildingEditorWindowPrefab == null)
                {
                    BuildingEditorAssets.LoadAssets();
                }

                if (BuildingEditorAssets.BuildingEditorWindowPrefab == null)
                {
                    Log.Warn("BuildingEditorWindowPrefab is null. Cannot open BuildingConfigEditorScreen.");
                    return;
                }

                GameObject windowGO = global::Util.KInstantiateUI(BuildingEditorAssets.BuildingEditorWindowPrefab, parent, true);
                if (windowGO == null) return;

                Instance = windowGO.GetComponent<BuildingConfigEditorScreen>() ?? windowGO.AddComponent<BuildingConfigEditorScreen>();
                Instance.Init();
                windowGO.name = "AI_BuildingConfigEditorScreen";
            }

            Instance.Show(true, initialCategory);
        }

        public override void Show(bool show = true)
        {
            Show(show, null);
        }

        public void Show(bool show, BuildingCategory? initialCategory)
        {
            base.Show(show);
            if (show)
            {
                transform.SetAsLastSibling();
                try
                {
                    Canvas canvas = GetComponent<Canvas>();
                    if (canvas != null)
                    {
                        canvas.overrideSorting = true;
                        canvas.sortingOrder = 350;
                    }
                }
                catch { }

                BuildingConfigRegistry.InvalidateCache();
                if (initialCategory.HasValue && initialCategory.Value != BuildingCategory.All)
                {
                    SetCategoryFilter(initialCategory.Value);
                }
                else
                {
                    ResetAllCategoryFilters();
                }

                UpdateLocalization();
                RefreshAllEntries();
                SelectFirstVisibleBuilding();
            }
            else
            {
                if (hiddenBackgroundDialogs.Count > 0)
                {
                    foreach (var dialog in hiddenBackgroundDialogs)
                    {
                        if (dialog != null)
                        {
                            try
                            {
                                KScreen kScreen = dialog.GetComponent<KScreen>();
                                if (kScreen != null)
                                {
                                    kScreen.Deactivate();
                                }
                                UnityEngine.Object.Destroy(dialog);
                            }
                            catch { }
                        }
                    }
                    hiddenBackgroundDialogs.Clear();
                    OptionsScreenRefresher.Reopen();
                }
            }
        }

        public void SetCategoryFilter(BuildingCategory category)
        {
            activeCategories.Clear();
            activeCategories.Add(category);

            if (FilterDropDown != null && FilterDropDown.DropDownEntries != null)
            {
                int index = 0;
                foreach (BuildingCategory cat in Enum.GetValues(typeof(BuildingCategory)))
                {
                    if (cat == BuildingCategory.All) continue;
                    if (index < FilterDropDown.DropDownEntries.Count)
                    {
                        var entry = FilterDropDown.DropDownEntries[index];
                        bool match = (cat == category);
                        entry.Enabled = match;
                        if (entry.Toggle != null)
                        {
                            entry.Toggle.SetOnFromCode(match);
                        }
                    }
                    index++;
                }
            }

            if (FilterBar != null)
            {
                FilterBar.Text = string.Empty;
                currentSearchQuery = string.Empty;
            }

            if (ToggleAllButtonText != null)
            {
                ToggleAllButtonText.SetText("Enable All");
            }

            ApplyFilters();
        }

        public void ResetAllCategoryFilters()
        {
            activeCategories.Clear();
            foreach (BuildingCategory cat in Enum.GetValues(typeof(BuildingCategory)))
            {
                if (cat == BuildingCategory.All) continue;
                activeCategories.Add(cat);
            }

            if (FilterDropDown != null && FilterDropDown.DropDownEntries != null)
            {
                foreach (var entry in FilterDropDown.DropDownEntries)
                {
                    entry.Enabled = true;
                    if (entry.Toggle != null)
                    {
                        entry.Toggle.SetOnFromCode(true);
                    }
                }
            }

            if (FilterBar != null)
            {
                FilterBar.Text = string.Empty;
                currentSearchQuery = string.Empty;
            }

            if (ToggleAllButtonText != null)
            {
                ToggleAllButtonText.SetText("Disable All");
            }

            ApplyFilters();
        }

        private void SelectFirstVisibleBuilding()
        {
            foreach (var kvp in entryMap)
            {
                if (kvp.Value != null && kvp.Value.gameObject.activeSelf && kvp.Value.TargetItem != null)
                {
                    SelectBuilding(kvp.Value.TargetItem);
                    return;
                }
            }
            if (SelectedBuilding != null)
            {
                RefreshDetails();
            }
            else
            {
                var all = BuildingConfigRegistry.GetAllItems();
                if (all.Count > 0) SelectBuilding(all[0]);
            }
        }

        public void Init()
        {
            if (initialized) return;
            initialized = true;

            // 1. Close, Reset All, Save, and Toggle All Buttons
            Transform tButtons = transform.Find("Buttons");
            Transform tClose = transform.Find("Buttons/CloseButton");
            if (tClose != null)
            {
                LocText closeText = tClose.GetComponentInChildren<LocText>();
                if (closeText != null) closeText.SetText(Translations.Get("BUILDINGEDITOR.RETURN"));

                FButton closeBtn = tClose.GetComponent<FButton>() ?? tClose.gameObject.AddComponent<FButton>();
                closeBtn.OnClick += delegate
                {
                    SaveAndClose();
                };
            }

            Transform tResetAll = transform.Find("Buttons/ResetButton");
            if (tResetAll != null)
            {
                LocText resetAllText = tResetAll.GetComponentInChildren<LocText>();
                if (resetAllText != null) resetAllText.SetText(Translations.Get("BUILDINGEDITOR.RESET_ALL"));

                FButton resetAllBtn = tResetAll.GetComponent<FButton>() ?? tResetAll.gameObject.AddComponent<FButton>();
                resetAllBtn.OnClick += delegate
                {
                    TryResetAll();
                };
            }

            if (tClose != null && tButtons != null)
            {
                // Create ToggleAllBuildingsButton (Enable All / Disable All)
                GameObject toggleAllGO = global::Util.KInstantiateUI(tClose.gameObject, tButtons.gameObject, true);
                toggleAllGO.name = "ToggleAllBuildingsButton";
                ToggleAllBuildingsButtonText = toggleAllGO.GetComponentInChildren<LocText>();
                ToggleAllBuildingsButton = toggleAllGO.GetComponent<FButton>() ?? toggleAllGO.AddComponent<FButton>();
                ToggleAllBuildingsButton.OnClick += delegate
                {
                    ToggleAllBuildings();
                };
                UIUtils.AddSimpleTooltipToObject(toggleAllGO, Translations.Get("BUILDINGEDITOR.BATCH_TOGGLE_TOOLTIP"));

                // Create SaveButton (Save / 保存)
                GameObject saveGO = global::Util.KInstantiateUI(tClose.gameObject, tButtons.gameObject, true);
                saveGO.name = "SaveButton";
                SaveButtonText = saveGO.GetComponentInChildren<LocText>();
                SaveButton = saveGO.GetComponent<FButton>() ?? saveGO.AddComponent<FButton>();
                SaveButton.OnClick += delegate
                {
                    SaveSettings();
                };
                UIUtils.AddSimpleTooltipToObject(saveGO, Translations.Get("BUILDINGEDITOR.SAVE_TOOLTIP"));

                // Visual layout order: [Toggle All] [Reset All] [Save] [Close]
                toggleAllGO.transform.SetSiblingIndex(0);
                if (tResetAll != null) tResetAll.SetSiblingIndex(1);
                saveGO.transform.SetSiblingIndex(2);
                tClose.SetSiblingIndex(3);
            }

            // 2. Outline (List) Entry Prefab & Container
            Transform tEntryPrefab = transform.Find("HorizontalLayout/ObjectList/ScrollArea/PresetEntryPrefab");
            if (tEntryPrefab != null)
            {
                OutlineEntryPrefab = tEntryPrefab.gameObject;
                OutlineEntryPrefab.AddOrGet<BuildingConfigUIEntry>();
                OutlineEntryPrefab.SetActive(false);
            }

            Transform tContainer = transform.Find("HorizontalLayout/ObjectList/ScrollArea/Content");
            if (tContainer != null)
            {
                OutlineEntryContainer = tContainer.gameObject;
            }

            // 3. Search Bar
            Transform tSearchInput = transform.Find("HorizontalLayout/ObjectList/SearchBar/Input");
            if (tSearchInput != null)
            {
                FilterBar = tSearchInput.GetComponent<FInputField2>() ?? tSearchInput.gameObject.AddComponent<FInputField2>();
                FilterBar.AddListener(OnSearchQueryChanged);
                FilterBar.Text = string.Empty;
            }

            Transform tDeleteBtn = transform.Find("HorizontalLayout/ObjectList/SearchBar/DeleteButton");
            if (tDeleteBtn != null)
            {
                ClearFilterButton = tDeleteBtn.GetComponent<FButton>() ?? tDeleteBtn.gameObject.AddComponent<FButton>();
                ClearFilterButton.OnClick += delegate
                {
                    if (FilterBar != null)
                    {
                        FilterBar.Text = string.Empty;
                    }
                };
            }

            // 4. Details Pane
            Transform tDetails = transform.Find("HorizontalLayout/ItemInfo");
            if (tDetails != null)
            {
                Details = tDetails.gameObject;
            }

            Transform tTitle = transform.Find("HorizontalLayout/ItemInfo/ScrollArea/Content/TitleWithIcon/Label");
            if (tTitle != null) SelectedEntryNameDisplay = tTitle.GetComponent<LocText>();

            Transform tIcon = transform.Find("HorizontalLayout/ItemInfo/ScrollArea/Content/TitleWithIcon/IconContainer/Icon");
            if (tIcon != null) SelectedEntryPreviewImage = tIcon.GetComponent<Image>();

            Transform tDesc = transform.Find("HorizontalLayout/ItemInfo/ScrollArea/Content/DescriptionContainer");
            if (tDesc != null) SelectedEntryDescriptionDisplay = tDesc.GetComponent<LocText>();

            Transform tModOrigin = transform.Find("HorizontalLayout/ItemInfo/ScrollArea/Content/ModFromContainer");
            if (tModOrigin != null) SelectedEntryModOriginDisplay = tModOrigin.GetComponent<LocText>();

            // Hide unused sliders
            Transform tWattage = transform.Find("HorizontalLayout/ItemInfo/ScrollArea/Content/WattageSettings");
            if (tWattage != null) tWattage.gameObject.SetActive(false);
            Transform tCapacity = transform.Find("HorizontalLayout/ItemInfo/ScrollArea/Content/CapacitySettings");
            if (tCapacity != null) tCapacity.gameObject.SetActive(false);
            Transform tRange = transform.Find("HorizontalLayout/ItemInfo/ScrollArea/Content/RangeSettings");
            if (tRange != null) tRange.gameObject.SetActive(false);

            // Master Enable Toggle
            Transform tEnable = transform.Find("HorizontalLayout/ItemInfo/ScrollArea/Content/EnableBuilding");
            if (tEnable != null)
            {
                EnableBuildingContainer = tEnable.gameObject;
                LocText enableText = tEnable.GetComponentInChildren<LocText>();
                if (enableText != null) enableText.SetText(Translations.Get("BUILDINGEDITOR.ENABLE_BUILDING"));

                Transform tEnableChk = tEnable.Find("Checkbox");
                if (tEnableChk != null)
                {
                    BuildingEnabledToggle = tEnableChk.GetComponent<FToggle>() ?? tEnableChk.gameObject.AddComponent<FToggle>();
                    BuildingEnabledToggle.SetCheckmark("Checkmark");
                    BuildingEnabledToggle.OnClick += delegate(bool on)
                    {
                        if (SelectedBuilding != null)
                        {
                            SelectedBuilding.SetEnabled(on);
                            OnBuildingToggled(SelectedBuilding, on);
                        }
                    };
                }
            }

            // Single Reset Button
            Transform tResetSingle = transform.Find("HorizontalLayout/ItemInfo/ScrollArea/Content/ResetButton");
            if (tResetSingle != null)
            {
                LocText resetSingleText = tResetSingle.GetComponentInChildren<LocText>();
                if (resetSingleText != null) resetSingleText.SetText(Translations.Get("BUILDINGEDITOR.RESET_CURRENT"));

                ResetSingleBuilding = tResetSingle.GetComponent<FButton>() ?? tResetSingle.gameObject.AddComponent<FButton>();
                ResetSingleBuilding.OnClick += ResetCurrentBuilding;
            }

            // 5. Category Filters Dropdown
            InitCategoryFilters();

            // Populate building rows
            CreateAllEntries();

            UpdateLocalization();
        }

        public void UpdateLocalization()
        {
            // 1. Window Title
            Transform tWinTitle = transform.Find("Title") ?? transform.Find("Header/Title") ?? transform.Find("TitleContainer/Title");
            if (tWinTitle == null)
            {
                foreach (LocText lt in GetComponentsInChildren<LocText>(true))
                {
                    if (lt != null && (lt.gameObject.name.Equals("Title", StringComparison.OrdinalIgnoreCase) ||
                                       (!string.IsNullOrEmpty(lt.key) && lt.key.IndexOf("TITLE", StringComparison.OrdinalIgnoreCase) >= 0)))
                    {
                        tWinTitle = lt.transform;
                        break;
                    }
                }
            }
            if (tWinTitle != null)
            {
                LocText titleLt = tWinTitle.GetComponent<LocText>();
                if (titleLt != null)
                {
                    titleLt.key = string.Empty;
                    titleLt.SetText(Translations.Get("BUILDINGEDITOR.TITLE"));
                }
            }

            // 2. Close & Reset Buttons
            Transform tClose = transform.Find("Buttons/CloseButton");
            if (tClose != null)
            {
                LocText closeText = tClose.GetComponentInChildren<LocText>();
                if (closeText != null)
                {
                    closeText.key = string.Empty;
                    closeText.SetText(Translations.Get("BUILDINGEDITOR.RETURN"));
                }
            }

            Transform tResetAll = transform.Find("Buttons/ResetButton");
            if (tResetAll != null)
            {
                LocText resetAllText = tResetAll.GetComponentInChildren<LocText>();
                if (resetAllText != null)
                {
                    resetAllText.key = string.Empty;
                    resetAllText.SetText(Translations.Get("BUILDINGEDITOR.RESET_ALL"));
                }
            }

            if (SaveButtonText != null)
            {
                SaveButtonText.key = string.Empty;
                SaveButtonText.SetText(Translations.Get("BUILDINGEDITOR.SAVE"));
            }

            UpdateBatchToggleButtonText();

            // 3. Search Bar Placeholder
            Transform tSearchInput = transform.Find("HorizontalLayout/ObjectList/SearchBar/Input");
            Transform tPlaceholder = transform.Find("HorizontalLayout/ObjectList/SearchBar/Input/Placeholder")
                ?? transform.Find("HorizontalLayout/ObjectList/SearchBar/Placeholder");
            if (tPlaceholder == null && tSearchInput != null)
            {
                tPlaceholder = tSearchInput.Find("Placeholder") ?? tSearchInput.Find("Text");
            }
            if (tPlaceholder != null)
            {
                LocText phLt = tPlaceholder.GetComponent<LocText>();
                if (phLt != null)
                {
                    phLt.key = string.Empty;
                    phLt.SetText(Translations.Get("BUILDINGEDITOR.SEARCH_PLACEHOLDER"));
                }
            }

            // 4. Filter Button Funnel Icon (Suppress text to prevent overlap)
            Transform tFilterBtn = transform.Find("HorizontalLayout/ObjectList/Filters/FilterButton");
            if (tFilterBtn != null)
            {
                LocText filterBtnText = tFilterBtn.GetComponentInChildren<LocText>();
                if (filterBtnText != null)
                {
                    filterBtnText.key = string.Empty;
                    filterBtnText.SetText(string.Empty);
                    filterBtnText.gameObject.SetActive(false);
                }
            }

            // 5. Enable & Single Reset Buttons
            if (EnableBuildingContainer != null)
            {
                LocText enableText = EnableBuildingContainer.GetComponentInChildren<LocText>();
                if (enableText != null)
                {
                    enableText.key = string.Empty;
                    enableText.SetText(Translations.Get("BUILDINGEDITOR.ENABLE_BUILDING"));
                }
            }

            if (ResetSingleBuilding != null)
            {
                LocText resetSingleText = ResetSingleBuilding.GetComponentInChildren<LocText>();
                if (resetSingleText != null)
                {
                    resetSingleText.key = string.Empty;
                    resetSingleText.SetText(Translations.Get("BUILDINGEDITOR.RESET_CURRENT"));
                }
            }

            // 6. Defensive wipe of any lingering STRINGS.UI.BUILDINGEDITOR keys
            foreach (LocText lt in GetComponentsInChildren<LocText>(true))
            {
                if (lt != null && !string.IsNullOrEmpty(lt.key) && lt.key.StartsWith("STRINGS.UI.BUILDINGEDITOR", StringComparison.OrdinalIgnoreCase))
                {
                    lt.key = string.Empty;
                }
            }
        }

        private void InitCategoryFilters()
        {
            Transform tFilterBtn = transform.Find("HorizontalLayout/ObjectList/Filters/FilterButton");
            if (tFilterBtn != null)
            {
                FilterDropDown = tFilterBtn.GetComponent<FMultiSelectDropdown>() ?? tFilterBtn.gameObject.AddComponent<FMultiSelectDropdown>();
                List<FMultiSelectDropdown.FDropDownEntry> list = new List<FMultiSelectDropdown.FDropDownEntry>();

                foreach (BuildingCategory cat in Enum.GetValues(typeof(BuildingCategory)))
                {
                    if (cat == BuildingCategory.All) continue;
                    activeCategories.Add(cat);
                    BuildingCategory currentCat = cat;
                    list.Add(new FMultiSelectDropdown.FDropDownEntry(
                        GetCategoryDisplayName(cat),
                        delegate(bool on)
                        {
                            if (on) activeCategories.Add(currentCat);
                            else activeCategories.Remove(currentCat);
                            ApplyFilters();
                        },
                        true
                    ));
                }

                FilterDropDown.DropDownEntries = list;
                FilterDropDown.InitializeDropDown();
            }

            Transform tToggleAll = transform.Find("HorizontalLayout/ObjectList/Filters/FilterButton/DropDownContent/ShowAllButton");
            if (tToggleAll != null)
            {
                ToggleAllButtonText = tToggleAll.GetComponentInChildren<LocText>();
                FButton btn = tToggleAll.GetComponent<FButton>() ?? tToggleAll.gameObject.AddComponent<FButton>();
                btn.OnClick += ToggleAllFilters;
            }
        }

        private void ToggleAllFilters()
        {
            bool anyDisabled = activeCategories.Count < Enum.GetValues(typeof(BuildingCategory)).Length - 1;
            foreach (BuildingCategory cat in Enum.GetValues(typeof(BuildingCategory)))
            {
                if (cat == BuildingCategory.All) continue;
                if (anyDisabled) activeCategories.Add(cat);
                else activeCategories.Remove(cat);
            }

            if (FilterDropDown != null && FilterDropDown.DropDownEntries != null)
            {
                foreach (var entry in FilterDropDown.DropDownEntries)
                {
                    entry.Enabled = anyDisabled;
                    if (entry.Toggle != null)
                    {
                        entry.Toggle.SetOnFromCode(anyDisabled);
                    }
                }
            }

            if (ToggleAllButtonText != null)
            {
                ToggleAllButtonText.SetText(anyDisabled ? "Disable All" : "Enable All");
            }

            ApplyFilters();
        }

        private string GetCategoryDisplayName(BuildingCategory cat)
        {
            UiLanguage lang = AutoMachineOptions.Instance != null ? AutoMachineOptions.Instance.OptionsLanguage : UiLanguage.Auto;
            UiLanguage resolved = OptionTextBinder.Resolve(lang);

            switch (cat)
            {
                case BuildingCategory.Fabricators:
                    switch (resolved)
                    {
                        case UiLanguage.ChineseSimplified: return "制造与加工";
                        case UiLanguage.ChineseTraditional: return "製造與加工";
                        case UiLanguage.Korean: return "제작 및 가공";
                        case UiLanguage.Japanese: return "製造・加工施設";
                        default: return "Fabricators & Cooking";
                    }
                case BuildingCategory.Refining:
                    switch (resolved)
                    {
                        case UiLanguage.ChineseSimplified: return "工业与精炼";
                        case UiLanguage.ChineseTraditional: return "工業與精煉";
                        case UiLanguage.Korean: return "산업 및 정제";
                        case UiLanguage.Japanese: return "工業・精錬施設";
                        default: return "Industry & Refining";
                    }
                case BuildingCategory.Special:
                    switch (resolved)
                    {
                        case UiLanguage.ChineseSimplified: return "特殊设施";
                        case UiLanguage.ChineseTraditional: return "特殊設施";
                        case UiLanguage.Korean: return "특수 시설";
                        case UiLanguage.Japanese: return "特殊施設";
                        default: return "Special Machinery";
                    }
                case BuildingCategory.Ranching:
                    switch (resolved)
                    {
                        case UiLanguage.ChineseSimplified: return "小动物牧养";
                        case UiLanguage.ChineseTraditional: return "小動物牧養";
                        case UiLanguage.Korean: return "크리터 목축";
                        case UiLanguage.Japanese: return "小動物飼育";
                        default: return "Critter Ranching";
                    }
                case BuildingCategory.Stations:
                    switch (resolved)
                    {
                        case UiLanguage.ChineseSimplified: return "工作站";
                        case UiLanguage.ChineseTraditional: return "工作站";
                        case UiLanguage.Korean: return "전문 작업대";
                        case UiLanguage.Japanese: return "ステーション";
                        default: return "Room Stations";
                    }
                case BuildingCategory.Research:
                    switch (resolved)
                    {
                        case UiLanguage.ChineseSimplified: return "科研与天文";
                        case UiLanguage.ChineseTraditional: return "科研與天文";
                        case UiLanguage.Korean: return "연구 및 천문";
                        case UiLanguage.Japanese: return "研究・天文施設";
                        default: return "Research & Astronomy";
                    }
                case BuildingCategory.Manual:
                    switch (resolved)
                    {
                        case UiLanguage.ChineseSimplified: return "手动与实用设施";
                        case UiLanguage.ChineseTraditional: return "手動與實用設施";
                        case UiLanguage.Korean: return "수동 및 실용 시설";
                        case UiLanguage.Japanese: return "手動・ユーティリティ";
                        default: return "Manual & Utility";
                    }
                default:
                    return cat.ToString();
            }
        }

        private void CreateAllEntries()
        {
            if (OutlineEntryContainer == null || OutlineEntryPrefab == null) return;

            foreach (var item in BuildingConfigRegistry.GetAllItems())
            {
                if (entryMap.ContainsKey(item.Id)) continue;

                GameObject go = global::Util.KInstantiateUI(OutlineEntryPrefab, OutlineEntryContainer, true);
                BuildingConfigUIEntry entry = go.GetComponent<BuildingConfigUIEntry>() ?? go.AddComponent<BuildingConfigUIEntry>();
                entry.UpdateItem(item);
                entryMap[item.Id] = entry;
            }

            ApplyFilters();
        }

        private void OnSearchQueryChanged(string query)
        {
            currentSearchQuery = query != null ? query.Trim().ToLowerInvariant() : string.Empty;
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            foreach (var kvp in entryMap)
            {
                BuildingConfigItem item = kvp.Value.TargetItem;
                if (item == null) continue;

                bool matchesCategory = activeCategories.Contains(item.Category);
                bool matchesSearch = string.IsNullOrEmpty(currentSearchQuery)
                    || item.Id.ToLowerInvariant().Contains(currentSearchQuery)
                    || item.GetDisplayName().ToLowerInvariant().Contains(currentSearchQuery);

                kvp.Value.gameObject.SetActive(matchesCategory && matchesSearch);
            }
        }

        public void SelectBuilding(BuildingConfigItem item)
        {
            SelectedBuilding = item;

            foreach (var kvp in entryMap)
            {
                kvp.Value.SetSelected(kvp.Value.TargetItem == item);
            }

            RefreshDetails();
        }

        public void OnBuildingToggled(BuildingConfigItem item, bool on)
        {
            if (entryMap.TryGetValue(item.Id, out var entry))
            {
                entry.UpdateUI();
            }

            if (SelectedBuilding == item)
            {
                if (BuildingEnabledToggle != null)
                {
                    BuildingEnabledToggle.SetOnFromCode(on);
                }
            }

            BuildingConfigItem.AutoSave();
            UpdateBatchToggleButtonText();
        }

        public void RefreshAllEntries()
        {
            var latestItems = BuildingConfigRegistry.GetAllItems(true);
            var itemMap = latestItems.ToDictionary(i => i.Id, StringComparer.OrdinalIgnoreCase);

            foreach (var kvp in entryMap)
            {
                if (itemMap.TryGetValue(kvp.Key, out var updatedItem))
                {
                    kvp.Value.UpdateItem(updatedItem);
                }
                else
                {
                    kvp.Value.UpdateUI();
                }
            }

            if (SelectedBuilding != null && itemMap.TryGetValue(SelectedBuilding.Id, out var updatedSelected))
            {
                SelectedBuilding = updatedSelected;
            }
        }

        private void RefreshDetails()
        {
            if (SelectedBuilding == null)
            {
                if (Details != null) Details.SetActive(false);
                return;
            }

            if (Details != null) Details.SetActive(true);

            if (SelectedEntryNameDisplay != null)
            {
                SelectedEntryNameDisplay.SetText(SelectedBuilding.GetDisplayName());
            }

            if (SelectedEntryDescriptionDisplay != null)
            {
                SelectedEntryDescriptionDisplay.SetText(SelectedBuilding.GetDescription());
            }

            if (SelectedEntryModOriginDisplay != null)
            {
                SelectedEntryModOriginDisplay.SetText("Automatic Industry  |  " + GetCategoryDisplayName(SelectedBuilding.Category));
            }

            if (SelectedEntryPreviewImage != null)
            {
                try
                {
                    Tuple<Sprite, Color> uisprite = BuildingConfigItem.GetBuildingSprite(SelectedBuilding.Id);
                    if (uisprite != null && uisprite.first != null)
                    {
                        SelectedEntryPreviewImage.sprite = uisprite.first;
                        SelectedEntryPreviewImage.color = uisprite.second;
                    }
                }
                catch
                {
                }
            }

            if (BuildingEnabledToggle != null)
            {
                BuildingEnabledToggle.SetOnFromCode(SelectedBuilding.IsEnabled);
            }

            // Populate sub-options
            BuildSubOptions();
        }

        private void BuildSubOptions()
        {
            // Clear existing sub-option rows
            foreach (GameObject row in activeSubOptionRows)
            {
                if (row != null) Destroy(row);
            }
            activeSubOptionRows.Clear();

            if (SelectedBuilding == null || !SelectedBuilding.HasSubOptions || EnableBuildingContainer == null)
            {
                return;
            }

            Transform contentParent = EnableBuildingContainer.transform.parent;
            int insertIndex = EnableBuildingContainer.transform.GetSiblingIndex() + 1;

            foreach (BuildingSubOption sub in SelectedBuilding.SubOptions)
            {
                GameObject row = global::Util.KInstantiateUI(EnableBuildingContainer, contentParent.gameObject, true);
                row.name = "SubOption_" + sub.Id;
                row.transform.SetSiblingIndex(insertIndex++);

                LocText label = row.GetComponentInChildren<LocText>();
                if (label != null)
                {
                    label.SetText(sub.Label);
#pragma warning disable CS0618
                    label.enableWordWrapping = true;
#pragma warning restore CS0618
                    RectTransform rt = label.rectTransform;
                    if (rt != null)
                    {
                        rt.offsetMax = new Vector2(-36f, rt.offsetMax.y);
                    }
                }

                if (!string.IsNullOrEmpty(sub.Tooltip))
                {
                    UIUtils.AddSimpleTooltipToObject(row, sub.Tooltip);
                }

                Transform tChk = row.transform.Find("Checkbox");
                if (tChk != null)
                {
                    FToggle toggle = tChk.GetComponent<FToggle>() ?? tChk.gameObject.AddComponent<FToggle>();
                    toggle.SetCheckmark("Checkmark");
                    toggle.SetOnFromCode(sub.IsEnabled);
                    BuildingSubOption currentSub = sub;
                    toggle.OnClick += delegate(bool on)
                    {
                        currentSub.SetEnabled(on);
                    };
                }

                activeSubOptionRows.Add(row);
            }
        }

        private void ResetCurrentBuilding()
        {
            if (SelectedBuilding != null)
            {
                SelectedBuilding.ResetToDefault();
                OnBuildingToggled(SelectedBuilding, SelectedBuilding.IsEnabled);
                RefreshDetails();
            }
        }

        private void TryResetAll()
        {
            GameObject parent = FrontEndManager.Instance?.gameObject
                ?? GameScreenManager.Instance?.ssOverlayCanvas
                ?? Global.Instance?.globalCanvas;

            ConfirmDialogScreen screen = ScreenPrefabs.Instance?.ConfirmDialogScreen != null
                ? KScreenManager.Instance.StartScreen(ScreenPrefabs.Instance.ConfirmDialogScreen.gameObject, parent) as ConfirmDialogScreen
                : null;

            if (screen != null)
            {
                string title = "Reset All Settings / 重置所有配置";
                string msg = "Are you sure you want to reset all building automation configurations to default?\n确定要将所有建筑的自动化配置恢复为默认吗？";
                screen.PopupConfirmDialog(msg, () =>
                {
                    BuildingConfigRegistry.ResetAllToDefault();
                    RefreshAllEntries();
                    RefreshDetails();
                }, null, null, null, title);
            }
            else
            {
                BuildingConfigRegistry.ResetAllToDefault();
                RefreshAllEntries();
                RefreshDetails();
            }
        }

        private void SaveAndClose()
        {
            try
            {
                POptions.WriteSettings(AutoMachineOptions.Instance);
                Log.Info("Saved building automation options successfully to config.json");
            }
            catch (Exception ex)
            {
                Log.Error("Failed to save options to config.json: ", ex);
            }

            Show(false);
        }

        public void SaveSettings()
        {
            try
            {
                BuildingConfigItem.AutoSave();
                if (SaveButtonText != null)
                {
                    SaveButtonText.SetText(Translations.Get("BUILDINGEDITOR.SAVED"));
                }
                Log.Info("Saved building automation options successfully to config.json");
            }
            catch (Exception ex)
            {
                Log.Error("Failed to save options to config.json: ", ex);
            }
        }

        public void ToggleAllBuildings()
        {
            bool allEnabled = BuildingConfigRegistry.AreAllEnabled();
            BuildingConfigRegistry.SetAllEnabled(!allEnabled);
            RefreshAllEntries();
            RefreshDetails();
            UpdateBatchToggleButtonText();
        }

        private void UpdateBatchToggleButtonText()
        {
            if (ToggleAllBuildingsButtonText != null)
            {
                bool allEnabled = BuildingConfigRegistry.AreAllEnabled();
                ToggleAllBuildingsButtonText.key = string.Empty;
                ToggleAllBuildingsButtonText.SetText(Translations.Get(allEnabled ? "BUILDINGEDITOR.DISABLE_ALL" : "BUILDINGEDITOR.ENABLE_ALL"));
            }
        }

        public override void OnKeyDown(KButtonEvent e)
        {
            if (e.TryConsume(Action.Escape))
            {
                SaveAndClose();
                return;
            }
            base.OnKeyDown(e);
        }
    }
}
