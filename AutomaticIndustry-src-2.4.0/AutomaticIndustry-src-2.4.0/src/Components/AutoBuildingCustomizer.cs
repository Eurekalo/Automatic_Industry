// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System;
using AutoMachineRebuilt.Automation;
using AutoMachineRebuilt.Config;
using AutoMachineRebuilt.Integration.Multiplayer;
using AutoMachineRebuilt.Localization;
using AutoMachineRebuilt.Util;
using KSerialization;
using UnityEngine;

namespace AutoMachineRebuilt.Components
{
    /// <summary>
    /// Component attached to automated building prefabs enabling per-building customization.
    /// Manages player UserMenu toggle buttons, duplicant wrench tweaking chores,
    /// CopySettings integration, Status Items, and instant debug/zero-dupe fallback paths.
    /// </summary>
    [SerializationConfig(MemberSerialization.OptIn)]
    [AddComponentMenu("KMonoBehaviour/scripts/AutoBuildingCustomizer")]
    public class AutoBuildingCustomizer : KMonoBehaviour
    {
        [Serialize]
        private bool isAutomated;

        [Serialize]
        private bool hasUserOverride;

        [Serialize]
        private bool pendingToggle;

        private Chore toggleChore;
        private AutomationToggleWorkable workable;
        private string prefabId;
        private KSelectable selectable;

        private static StatusItem statusItemAutomated;
        private static StatusItem statusItemManual;
        private static StatusItem statusItemPending;

        private static readonly EventSystem.IntraObjectHandler<AutoBuildingCustomizer> OnRefreshUserMenuDelegate =
            new EventSystem.IntraObjectHandler<AutoBuildingCustomizer>(delegate (AutoBuildingCustomizer cmp, object data)
            {
                cmp.OnRefreshUserMenu(data);
            });

        private static readonly EventSystem.IntraObjectHandler<AutoBuildingCustomizer> OnCopySettingsDelegate =
            new EventSystem.IntraObjectHandler<AutoBuildingCustomizer>(delegate (AutoBuildingCustomizer cmp, object data)
            {
                cmp.OnCopySettings(data);
            });

        public bool IsAutomated => isAutomated;
        public bool HasUserOverride => hasUserOverride;
        public bool PendingToggle => pendingToggle;

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            workable = gameObject.AddOrGet<AutomationToggleWorkable>();
            selectable = GetComponent<KSelectable>();

            var kpid = GetComponent<KPrefabID>();
            if (kpid != null)
            {
                prefabId = kpid.PrefabTag.Name;
            }

            InitStatusItems();
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            if (workable == null) workable = gameObject.AddOrGet<AutomationToggleWorkable>();
            if (selectable == null) selectable = GetComponent<KSelectable>();

            if (string.IsNullOrEmpty(prefabId))
            {
                var kpid = GetComponent<KPrefabID>();
                if (kpid != null) prefabId = kpid.PrefabTag.Name;
            }

            // Smooth migration for existing colonies: if building hasn't been individually
            // customized by player yet, inherit from colony registry or global option default.
            if (!hasUserOverride)
            {
                if (ColonyAutomationMasterRegistry.Instance != null &&
                    ColonyAutomationMasterRegistry.Instance.TryGetOverride(Grid.PosToCell(gameObject), out bool savedState))
                {
                    isAutomated = savedState;
                    hasUserOverride = true;
                }
                else
                {
                    isAutomated = AutoMachineOptions.IsEnabledFor(prefabId);
                }
            }

            Subscribe(493375141, OnRefreshUserMenuDelegate); // GameHashes.RefreshUserMenu
            Subscribe(-905833192, OnCopySettingsDelegate);   // GameHashes.CopySettings

            if (pendingToggle)
            {
                CreateChore();
            }

            RefreshStatusItems();
        }

        /// <summary>
        /// Initializes custom status items with localized strings.
        /// </summary>
        private static void InitStatusItems()
        {
            if (statusItemAutomated != null) return;

            statusItemAutomated = new StatusItem(
                "AutoMachine_AutomationActive",
                "BUILDING",
                "status_item_operating",
                StatusItem.IconType.Info,
                NotificationType.Neutral,
                false,
                OverlayModes.None.ID
            );
            statusItemAutomated.SetResolveStringCallback((str, data) => STRINGS.AUTOMACHINEREBUILT.UI.STATUSITEMS.AUTOMATION_ENABLED);

            statusItemManual = new StatusItem(
                "AutoMachine_AutomationManual",
                "BUILDING",
                string.Empty,
                StatusItem.IconType.Info,
                NotificationType.Neutral,
                false,
                OverlayModes.None.ID
            );
            statusItemManual.SetResolveStringCallback((str, data) => STRINGS.AUTOMACHINEREBUILT.UI.STATUSITEMS.AUTOMATION_MANUAL);

            statusItemPending = new StatusItem(
                "AutoMachine_AutomationPending",
                "BUILDING",
                "status_item_pending_work",
                StatusItem.IconType.Exclamation,
                NotificationType.Neutral,
                false,
                OverlayModes.None.ID
            );
            statusItemPending.SetResolveStringCallback((str, data) => STRINGS.AUTOMACHINEREBUILT.UI.STATUSITEMS.AUTOMATION_PENDING);
        }

        /// <summary>
        /// Checks whether automation is active on this building for the specified prefab ID.
        /// </summary>
        public bool IsAutomatedFor(string targetPrefabId)
        {
            var options = AutoMachineOptions.Instance;
            if (options == null || !options.EnableAllAutomation)
            {
                // Master switch is off
                if (options != null && !AutoMachineOptions.IsEnabledFor(targetPrefabId))
                {
                    return false;
                }
            }

            if (options != null && options.EnablePerBuildingCustomization && hasUserOverride)
            {
                return isAutomated;
            }

            return AutoMachineOptions.IsEnabledFor(targetPrefabId);
        }

        /// <summary>
        /// Retrieves the localized manual vs automated explanation for this specific building.
        /// </summary>
        public static string GetBuildingExplanation(string prefabId, UiLanguage lang)
        {
            if (string.IsNullOrEmpty(prefabId)) return null;

            string normKey = prefabId.ToUpperInvariant();
            if (normKey == "LIQUIDVALVE" || normKey == "GASVALVE")
            {
                normKey = "VALVE";
            }

            var table = BuildingDescriptions.DescriptionsFor(lang);
            string fullKey = BuildingDescriptions.DescriptionKeyPrefix + normKey;
            if (table != null && table.TryGetValue(fullKey, out string desc) && !string.IsNullOrEmpty(desc))
            {
                return desc;
            }

            if (BuildingDescriptions.DescriptionsEnglish.TryGetValue(fullKey, out string engDesc) && !string.IsNullOrEmpty(engDesc))
            {
                return engDesc;
            }

            return null;
        }

        private static string GetFormattedTooltip(string baseTooltip, string explanation, string tag)
        {
            if (!string.IsNullOrEmpty(explanation))
            {
                return baseTooltip + "\n\n" + explanation + "\n\n" + tag;
            }
            return baseTooltip + "\n\n" + tag;
        }

        private void OnRefreshUserMenu(object data)
        {
            var options = AutoMachineOptions.Instance;
            if (options == null || !options.EnablePerBuildingCustomization)
            {
                return;
            }

            UiLanguage lang = OptionTextBinder.Resolve(options.OptionsLanguage);
            var tr = Translations.For(lang);

            string explanation = GetBuildingExplanation(prefabId, lang);
            string modSourceTag = tr.ContainsKey("UI.USERMENUACTIONS.MOD_SOURCE_TAG")
                ? tr["UI.USERMENUACTIONS.MOD_SOURCE_TAG"]
                : "<color=#4BC5FF><b>[Mod: Automatic Industry]</b></color>";

            if (pendingToggle)
            {
                // Cancel pending upgrade chore
                string cancelName = tr.ContainsKey("UI.USERMENUACTIONS.CANCEL_TOGGLE.NAME")
                    ? tr["UI.USERMENUACTIONS.CANCEL_TOGGLE.NAME"]
                    : "Cancel Upgrade";
                string cancelTooltipRaw = tr.ContainsKey("UI.USERMENUACTIONS.CANCEL_TOGGLE.TOOLTIP")
                    ? tr["UI.USERMENUACTIONS.CANCEL_TOGGLE.TOOLTIP"]
                    : "Cancel the pending automation upgrade task.";
                string cancelTooltip = GetFormattedTooltip(cancelTooltipRaw, explanation, modSourceTag);

                KIconButtonMenu.ButtonInfo cancelBtn = new KIconButtonMenu.ButtonInfo(
                    "action_cancel",
                    cancelName,
                    OnUserCancelToggle,
                    Action.NumActions,
                    null,
                    null,
                    null,
                    cancelTooltip
                );
                Game.Instance.userMenu.AddButton(gameObject, cancelBtn);
            }
            else
            {
                if (isAutomated)
                {
                    // Revert to manual
                    string disableName = tr.ContainsKey("UI.USERMENUACTIONS.DISABLE_AUTOMATION.NAME")
                        ? tr["UI.USERMENUACTIONS.DISABLE_AUTOMATION.NAME"]
                        : "Revert to Manual";
                    string disableTooltipRaw = tr.ContainsKey("UI.USERMENUACTIONS.DISABLE_AUTOMATION.TOOLTIP")
                        ? tr["UI.USERMENUACTIONS.DISABLE_AUTOMATION.TOOLTIP"]
                        : "Revert this machine back to vanilla manual duplicant operation.";
                    string manualTooltip = GetFormattedTooltip(disableTooltipRaw, explanation, modSourceTag);

                    KIconButtonMenu.ButtonInfo manualBtn = new KIconButtonMenu.ButtonInfo(
                        "action_repair",
                        disableName,
                        OnUserRequestToggle,
                        Action.NumActions,
                        null,
                        null,
                        null,
                        manualTooltip
                    );
                    Game.Instance.userMenu.AddButton(gameObject, manualBtn);
                }
                else
                {
                    // Enable automation
                    string enableName = tr.ContainsKey("UI.USERMENUACTIONS.ENABLE_AUTOMATION.NAME")
                        ? tr["UI.USERMENUACTIONS.ENABLE_AUTOMATION.NAME"]
                        : "Enable Automation";
                    string enableTooltipRaw = tr.ContainsKey("UI.USERMENUACTIONS.ENABLE_AUTOMATION.TOOLTIP")
                        ? tr["UI.USERMENUACTIONS.ENABLE_AUTOMATION.TOOLTIP"]
                        : "Upgrade this machine to operate unattended without duplicants.";
                    string autoTooltip = GetFormattedTooltip(enableTooltipRaw, explanation, modSourceTag);

                    KIconButtonMenu.ButtonInfo autoBtn = new KIconButtonMenu.ButtonInfo(
                        "action_power",
                        enableName,
                        OnUserRequestToggle,
                        Action.NumActions,
                        null,
                        null,
                        null,
                        autoTooltip
                    );
                    Game.Instance.userMenu.AddButton(gameObject, autoBtn);
                }

                if (hasUserOverride)
                {
                    // Revert to global default
                    string resetName = tr.ContainsKey("UI.USERMENUACTIONS.RESET_OVERRIDE.NAME")
                        ? tr["UI.USERMENUACTIONS.RESET_OVERRIDE.NAME"]
                        : "Follow Global Setting";
                    string resetTooltipRaw = tr.ContainsKey("UI.USERMENUACTIONS.RESET_OVERRIDE.TOOLTIP")
                        ? tr["UI.USERMENUACTIONS.RESET_OVERRIDE.TOOLTIP"]
                        : "Clear individual customization on this machine and sync with global mod settings.";
                    string resetTooltip = GetFormattedTooltip(resetTooltipRaw, explanation, modSourceTag);

                    KIconButtonMenu.ButtonInfo resetBtn = new KIconButtonMenu.ButtonInfo(
                        "action_sync",
                        resetName,
                        OnUserResetToGlobal,
                        Action.NumActions,
                        null,
                        null,
                        null,
                        resetTooltip
                    );
                    Game.Instance.userMenu.AddButton(gameObject, resetBtn);
                }
            }
        }

        private void OnUserRequestToggle()
        {
            bool shiftPressed = UnityEngine.Input.GetKey(KeyCode.LeftShift) || UnityEngine.Input.GetKey(KeyCode.RightShift) ||
                                UnityEngine.Input.GetKey(KeyCode.LeftControl) || UnityEngine.Input.GetKey(KeyCode.RightControl);

            RequestToggle(shiftPressed);
        }

        private void OnUserCancelToggle()
        {
            CancelPendingToggle();
        }

        private void OnUserResetToGlobal()
        {
            ResetToGlobalDefault();
        }

        /// <summary>
        /// Checks if there are any living Duplicants present on the specific world/asteroid or rocket interior of this building.
        /// Handles DLC multi-planetoid colonies and in-flight Clustercraft rocket cabins.
        /// </summary>
        public static bool HasLiveMinionsInWorld(GameObject go)
        {
            if (go == null) return false;
            int worldId = go.GetMyWorldId();

            if (global::Components.LiveMinionIdentities != null)
            {
                for (int i = 0; i < global::Components.LiveMinionIdentities.Count; i++)
                {
                    MinionIdentity minion = global::Components.LiveMinionIdentities[i];
                    if (minion != null && minion.GetMyWorldId() == worldId)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Requests toggling the automation state of this building.
        /// Handles Instant / Debug / Zero-dupe / Multi-world fast-paths vs scheduling duplicant chore.
        /// </summary>
        public void RequestToggle(bool forceInstant = false)
        {
            var options = AutoMachineOptions.Instance;
            bool hasMinionsInWorld = HasLiveMinionsInWorld(gameObject);

            bool instantMode = forceInstant ||
                               (options != null && options.ToggleMode == AutomationToggleMode.Instant) ||
                               DebugHandler.InstantBuildMode ||
                               Game.Instance.SandboxModeActive ||
                               !hasMinionsInWorld;

            if (instantMode)
            {
                CompleteToggle();
                try
                {
                    PopFXManager.Instance.SpawnFX(
                        PopFXManager.Instance.sprite_Building,
                        isAutomated ? "Automated" : "Manual",
                        transform,
                        Vector3.zero
                    );
                }
                catch
                {
                }
                return;
            }

            pendingToggle = true;
            CreateChore();
            RefreshStatusItems();
            Game.Instance.userMenu.Refresh(gameObject);
        }

        private void CreateChore()
        {
            if (toggleChore != null)
            {
                toggleChore.Cancel("Recreating toggle chore");
                toggleChore = null;
            }

            if (workable == null)
            {
                workable = gameObject.AddOrGet<AutomationToggleWorkable>();
            }

            toggleChore = new WorkChore<AutomationToggleWorkable>(
                Db.Get().ChoreTypes.Toggle,
                workable,
                null,
                true,
                null,
                null,
                null,
                true,
                null,
                false,
                true,
                null,
                false,
                true,
                true,
                PriorityScreen.PriorityClass.basic,
                5,
                false,
                true
            );
        }

        private void CancelChore()
        {
            if (toggleChore != null)
            {
                toggleChore.Cancel("Cancelled by user or completion");
                toggleChore = null;
            }
        }

        private bool isApplyingRemoteSync;

        public void CancelPendingToggle()
        {
            pendingToggle = false;
            CancelChore();
            RefreshStatusItems();
            if (Game.Instance != null && Game.Instance.userMenu != null)
            {
                Game.Instance.userMenu.Refresh(gameObject);
            }

            if (!isApplyingRemoteSync)
            {
                MultiplayerManager.SendBuildingSync(Grid.PosToCell(gameObject), prefabId, 1, 0);
            }
        }

        public void CompleteToggle()
        {
            isAutomated = !isAutomated;
            hasUserOverride = true;
            pendingToggle = false;
            CancelChore();

            if (ColonyAutomationMasterRegistry.Instance != null)
            {
                int cell = Grid.PosToCell(gameObject);
                ColonyAutomationMasterRegistry.Instance.RecordOverride(cell, isAutomated);
            }

            SyncBuildingState();

            if (!isApplyingRemoteSync)
            {
                MultiplayerManager.SendBuildingSync(Grid.PosToCell(gameObject), prefabId, 0, isAutomated ? 1 : 2);
            }
        }

        public void ResetToGlobalDefault()
        {
            hasUserOverride = false;
            pendingToggle = false;
            CancelChore();

            if (ColonyAutomationMasterRegistry.Instance != null)
            {
                int cell = Grid.PosToCell(gameObject);
                ColonyAutomationMasterRegistry.Instance.RemoveOverride(cell);
            }

            isAutomated = AutoMachineOptions.IsEnabledFor(prefabId);

            SyncBuildingState();

            if (!isApplyingRemoteSync)
            {
                MultiplayerManager.SendBuildingSync(Grid.PosToCell(gameObject), prefabId, 2, 0);
            }
        }

        /// <summary>
        /// Applies remote building customization updates received across the ONI Together multiplayer network.
        /// </summary>
        public void ApplyRemoteSync(byte actionType, int targetOverrideState, ulong senderId)
        {
            isApplyingRemoteSync = true;
            try
            {
                switch (actionType)
                {
                    case 0: // Set override directly
                        this.isAutomated = (targetOverrideState == 1);
                        this.hasUserOverride = true;
                        this.pendingToggle = false;
                        CancelChore();

                        if (ColonyAutomationMasterRegistry.Instance != null)
                        {
                            int cell = Grid.PosToCell(gameObject);
                            ColonyAutomationMasterRegistry.Instance.RecordOverride(cell, this.isAutomated);
                        }

                        SyncBuildingState();
                        break;

                    case 1: // Cancel pending toggle chore
                        this.pendingToggle = false;
                        CancelChore();
                        RefreshStatusItems();
                        if (Game.Instance != null && Game.Instance.userMenu != null)
                        {
                            Game.Instance.userMenu.Refresh(gameObject);
                        }
                        break;

                    case 2: // Reset to global
                        this.hasUserOverride = false;
                        this.pendingToggle = false;
                        CancelChore();

                        if (ColonyAutomationMasterRegistry.Instance != null)
                        {
                            int cell = Grid.PosToCell(gameObject);
                            ColonyAutomationMasterRegistry.Instance.RemoveOverride(cell);
                        }

                        this.isAutomated = AutoMachineOptions.IsEnabledFor(prefabId);
                        SyncBuildingState();
                        break;
                }
            }
            finally
            {
                isApplyingRemoteSync = false;
            }
        }

        private void OnCopySettings(object data)
        {
            GameObject sourceGo = data as GameObject;
            if (sourceGo == null) return;

            var sourceCustomizer = sourceGo.GetComponent<AutoBuildingCustomizer>();
            if (sourceCustomizer == null) return;

            this.isAutomated = sourceCustomizer.isAutomated;
            this.hasUserOverride = sourceCustomizer.hasUserOverride;

            if (this.pendingToggle)
            {
                CancelPendingToggle();
            }

            SyncBuildingState();

            if (!isApplyingRemoteSync)
            {
                int stateCode = this.hasUserOverride ? (this.isAutomated ? 1 : 2) : 0;
                byte actionCode = this.hasUserOverride ? (byte)0 : (byte)2;
                MultiplayerManager.SendBuildingSync(Grid.PosToCell(gameObject), prefabId, actionCode, stateCode);
            }
        }

        /// <summary>
        /// Immediately synchronizes all machine controllers, chore states, and duplicant operation flags.
        /// </summary>
        public void SyncBuildingState()
        {
            if (isAutomated)
            {
                ChoreSuppression.CancelOperateChores(gameObject);

                ComplexFabricator fabricator = GetComponent<ComplexFabricator>();
                if (fabricator != null && fabricator.GetComponent<ComplexFabricatorWorkable>() != null)
                {
                    fabricator.duplicantOperated = false;
                    BuildingComplete building = GetComponent<BuildingComplete>();
                    if (building != null) building.isManuallyOperated = false;

                    var cancelChore = HarmonyLib.AccessTools.Method(typeof(ComplexFabricator), "CancelChore");
                    if (cancelChore != null)
                    {
                        SafeInvoke.Try("AutoBuildingCustomizer CancelChore", delegate
                        {
                            cancelChore.Invoke(fabricator, null);
                        });
                    }

                    fabricator.Trigger((int)GameHashes.FabricatorOrdersUpdated, fabricator);
                }
            }
            else
            {
                // Revert to manual operation
                ComplexFabricator fabricator = GetComponent<ComplexFabricator>();
                if (fabricator != null && fabricator.GetComponent<ComplexFabricatorWorkable>() != null)
                {
                    fabricator.duplicantOperated = true;
                    BuildingComplete building = GetComponent<BuildingComplete>();
                    if (building != null) building.isManuallyOperated = true;

                    var updateChore = HarmonyLib.AccessTools.Method(typeof(ComplexFabricator), "UpdateChore");
                    if (updateChore != null)
                    {
                        SafeInvoke.Try("AutoBuildingCustomizer UpdateChore", delegate
                        {
                            updateChore.Invoke(fabricator, null);
                        });
                    }

                    fabricator.SetQueueDirty();
                    fabricator.Trigger((int)GameHashes.FabricatorOrdersUpdated, fabricator);
                }

                AutoFabricatorController fabController = GetComponent<AutoFabricatorController>();
                if (fabController != null)
                {
                    fabController.StopAutomation();
                }

                AutoOilRefinery refinery = GetComponent<AutoOilRefinery>();
                if (refinery != null)
                {
                    refinery.StopAutomation();
                }

                AutoWorkControllerBase workController = GetComponent<AutoWorkControllerBase>();
                if (workController != null && !(workController is AutoStorageReleaseController))
                {
                    workController.StopAutomation();
                }
            }

            // For machines like Desalinator whose conversion is driven by internal state machines,
            // trigger storage change event and wake state machine if input mass is already present.
            Desalinator desalinator = GetComponent<Desalinator>();
            if (desalinator != null)
            {
                Storage storage = desalinator.GetComponent<Storage>();
                if (storage != null)
                {
                    desalinator.Trigger((int)GameHashes.OnStorageChange, storage);
                }

                Desalinator.StatesInstance smi = desalinator.GetSMI<Desalinator.StatesInstance>();
                if (smi != null && smi.IsRunning() && smi.GetCurrentState() == smi.sm.on.waiting && HasConvertableMass(gameObject))
                {
                    smi.GoTo(smi.sm.on.working_pre);
                }
            }

            // Compost: Refresh state machine so dupe flip chores are immediately created/cancelled
            Compost compost = GetComponent<Compost>();
            if (compost != null && compost.smi != null && compost.smi.IsRunning())
            {
                compost.smi.StopSM("customizer toggle");
                compost.smi.StartSM();
            }

            // IceCooledFan: Refresh state machine so use chore is re-evaluated immediately
            IceCooledFan fan = GetComponent<IceCooledFan>();
            if (fan != null && fan.smi != null && fan.smi.IsRunning())
            {
                fan.smi.StopSM("customizer toggle");
                fan.smi.StartSM();
            }

            // MilkFatSeparator (Gleaner): If automated and full, trigger immediate emptying
            MilkSeparator.Instance milkSmi = gameObject.GetSMI<MilkSeparator.Instance>();
            if (milkSmi != null && milkSmi.IsRunning() && isAutomated && MilkSeparator.RequiresEmptying(milkSmi))
            {
                milkSmi.GoTo(milkSmi.sm.operational.emptyComplete);
            }

            // Food Smoker: If automated and requires emptying, empty immediately
            FoodSmoker.StatesInstance smokerSmi = gameObject.GetSMI<FoodSmoker.StatesInstance>();
            if (smokerSmi != null && smokerSmi.IsRunning() && isAutomated && smokerSmi.RequiresEmptying())
            {
                smokerSmi.OnEmptyComplete(null);
            }

            // GeoTuner: If reverted to manual, restore delivery capacities immediately
            if (!isAutomated)
            {
                AutoGeoTuner geoTuner = GetComponent<AutoGeoTuner>();
                if (geoTuner != null)
                {
                    geoTuner.StopAutomation();
                }
            }

            RefreshStatusItems();
            if (Game.Instance != null && Game.Instance.userMenu != null)
            {
                Game.Instance.userMenu.Refresh(gameObject);
            }
        }

        private static bool HasConvertableMass(GameObject go)
        {
            if (go == null) return false;
            ElementConverter[] converters = go.GetComponents<ElementConverter>();
            if (converters == null) return false;
            for (int i = 0; i < converters.Length; i++)
            {
                if (converters[i] != null && converters[i].HasEnoughMassToStartConverting())
                {
                    return true;
                }
            }
            return false;
        }

        public void RefreshStatusItems()
        {
            if (selectable == null) selectable = GetComponent<KSelectable>();
            if (selectable == null) return;

            var options = AutoMachineOptions.Instance;
            if (options == null || !options.EnablePerBuildingCustomization)
            {
                selectable.RemoveStatusItem(statusItemAutomated);
                selectable.RemoveStatusItem(statusItemManual);
                selectable.RemoveStatusItem(statusItemPending);
                return;
            }

            if (pendingToggle)
            {
                selectable.RemoveStatusItem(statusItemAutomated);
                selectable.RemoveStatusItem(statusItemManual);
                selectable.AddStatusItem(statusItemPending, this);
            }
            else
            {
                selectable.RemoveStatusItem(statusItemPending);
                if (isAutomated)
                {
                    selectable.RemoveStatusItem(statusItemManual);
                    selectable.AddStatusItem(statusItemAutomated, this);
                }
                else
                {
                    selectable.RemoveStatusItem(statusItemAutomated);
                    selectable.AddStatusItem(statusItemManual, this);
                }
            }
        }

        protected override void OnCleanUp()
        {
            CancelChore();
            Unsubscribe(493375141, OnRefreshUserMenuDelegate);
            Unsubscribe(-905833192, OnCopySettingsDelegate);
            base.OnCleanUp();
        }
    }
}
