// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using AutoMachineRebuilt.UI.Components;
using AutoMachineRebuilt.UI.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoMachineRebuilt.UI
{
    public class BuildingConfigUIEntry : KMonoBehaviour
    {
        public BuildingConfigItem TargetItem;

        private Image displayImage;
        private LocText label;
        private FToggle enabledCheckbox;
        private Image checkboxBg;
        private FButton selectButton;
        private GameObject gear;
        private Image selectionHighlight;
        private bool init;

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            UpdateUI();
        }

        private void InitUi()
        {
            if (init) return;
            init = true;

            Transform tImg = transform.Find("DisplayImageContainer/DisplayImage");
            displayImage = tImg != null ? tImg.GetComponent<Image>() : null;

            Transform tLbl = transform.Find("Label");
            label = tLbl != null ? tLbl.GetComponent<LocText>() : null;

            Transform tGear = transform.Find("Gear");
            gear = tGear != null ? tGear.gameObject : null;
            if (gear != null)
            {
                UIUtils.AddSimpleTooltipToObject(gear, "Contains advanced conditional settings");
            }

            Transform tChk = transform.Find("Checkbox");
            if (tChk != null)
            {
                enabledCheckbox = tChk.GetComponent<FToggle>() ?? tChk.gameObject.AddComponent<FToggle>();
                checkboxBg = tChk.GetComponent<Image>();
                enabledCheckbox.SetCheckmark("Checkmark");
                enabledCheckbox.OnClick += delegate(bool on)
                {
                    if (TargetItem != null)
                    {
                        TargetItem.SetEnabled(on);
                        BuildingConfigEditorScreen.Instance?.OnBuildingToggled(TargetItem, on);
                    }
                };
            }

            selectButton = GetComponent<FButton>() ?? gameObject.AddComponent<FButton>();
            selectButton.OnClick += delegate
            {
                if (TargetItem != null)
                {
                    BuildingConfigEditorScreen.Instance?.SelectBuilding(TargetItem);
                }
            };

            selectionHighlight = GetComponent<Image>();
        }

        public void SetSelected(bool selected)
        {
            if (selectionHighlight != null)
            {
                selectionHighlight.color = selected ? UIUtils.rgb(45f, 52f, 70f) : UIUtils.rgb(30f, 35f, 45f);
            }
        }

        public void UpdateItem(BuildingConfigItem newItem)
        {
            TargetItem = newItem;
            UpdateUI();
        }

        public void UpdateUI()
        {
            if (TargetItem == null) return;
            if (!init)
            {
                InitUi();
            }

            if (label != null)
            {
                label.SetText(TargetItem.GetDisplayName());
            }

            if (gear != null)
            {
                gear.SetActive(TargetItem.HasSubOptions);
            }

            if (enabledCheckbox != null)
            {
                enabledCheckbox.SetOnFromCode(TargetItem.IsEnabled);
                enabledCheckbox.SetInteractable(true);
            }

            if (displayImage != null)
            {
                try
                {
                    Tuple<Sprite, Color> uisprite = BuildingConfigItem.GetBuildingSprite(TargetItem.Id);
                    if (uisprite != null && uisprite.first != null)
                    {
                        displayImage.sprite = uisprite.first;
                        displayImage.color = uisprite.second;
                    }
                }
                catch
                {
                }
            }
        }
    }
}
