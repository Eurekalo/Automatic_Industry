// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System.Reflection;
using AutoMachineRebuilt.Config;
using HarmonyLib;

namespace AutoMachineRebuilt.Patches
{
    /// <summary>
    /// Optionally hides world-space floating status icons beneath buildings
    /// for Skill and Room requirements, while keeping the full details in the
    /// selection side screen and tooltips.
    /// </summary>
    [HarmonyPatch(typeof(Db), "Initialize")]
    public static class HideWorldStatusIconsPatch
    {
        private static readonly FieldInfo ShowWorldIconField =
            typeof(StatusItem).GetField("showShowWorldIcon", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix()
        {
            ApplyIconVisibility();
        }

        public static void ApplyIconVisibility()
        {
            var db = Db.Get();
            if (db == null || db.BuildingStatusItems == null)
            {
                return;
            }

            bool hideSkill = AutoMachineOptions.Instance.HideWorldIconSkillRequirement;
            SetStatusItemWorldIcon(db.BuildingStatusItems.ColonyLacksRequiredSkillPerk, !hideSkill);
            SetStatusItemWorldIcon(db.BuildingStatusItems.ClusterColonyLacksRequiredSkillPerk, !hideSkill);
            SetStatusItemWorldIcon(db.BuildingStatusItems.ColonyLacksDupeWithMultiSkillPerk, !hideSkill);

            bool hideRoom = AutoMachineOptions.Instance.HideWorldIconRoomRequirement;
            SetStatusItemWorldIcon(db.BuildingStatusItems.NotInRequiredRoom, !hideRoom);
        }

        public static void SetStatusItemWorldIcon(StatusItem item, bool show)
        {
            if (item == null)
            {
                return;
            }

            item.showInHoverCardOnly = !show;
            if (ShowWorldIconField != null)
            {
                ShowWorldIconField.SetValue(item, show);
            }
        }
    }

    [HarmonyPatch(typeof(StatusItemGroup), "AddStatusItem")]
    public static class StatusItemGroup_AddStatusItem_WorldIconPatch
    {
        public static void Prefix(StatusItem item)
        {
            if (item == null)
            {
                return;
            }

            var bsi = Db.Get()?.BuildingStatusItems;
            if (bsi == null)
            {
                return;
            }

            if (AutoMachineOptions.Instance.HideWorldIconSkillRequirement &&
                (item == bsi.ColonyLacksRequiredSkillPerk ||
                 item == bsi.ClusterColonyLacksRequiredSkillPerk ||
                 item == bsi.ColonyLacksDupeWithMultiSkillPerk))
            {
                HideWorldStatusIconsPatch.SetStatusItemWorldIcon(item, false);
            }
            else if (AutoMachineOptions.Instance.HideWorldIconRoomRequirement &&
                     item == bsi.NotInRequiredRoom)
            {
                HideWorldStatusIconsPatch.SetStatusItemWorldIcon(item, false);
            }
        }
    }
}
