// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AutoMachineRebuilt.UI.Components
{
    public class FMultiSelectDropdown : KMonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public class FDropDownEntry
        {
            public string Title;
            public string Description = "";
            public Action<bool> OnToggled;
            public bool Enabled = true;
            public FToggle Toggle;

            public FDropDownEntry(string title, Action<bool> onToggled, bool enabled = true, string tooltip = "")
            {
                Title = title;
                OnToggled = onToggled;
                Enabled = enabled;
                Description = tooltip;
            }
        }

        public class FDropDownButtonEntry : FDropDownEntry
        {
            public FButton Button;

            public FDropDownButtonEntry(string title, Action<bool> onToggled, string tooltip = "")
                : base(title, onToggled, true, tooltip)
            {
            }
        }

        public System.Action RefreshUI;

        private GameObject DropDownContent;
        private FToggle entryPrefab;
        private FButton buttonEntryPrefab;
        private Image backgroundImage;

        public Color Inactive = UIUtils.rgb(62f, 67f, 87f);
        public Color OnHover = UIUtils.rgb(88f, 95f, 122f);

        public List<FDropDownEntry> DropDownEntries;

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            backgroundImage = GetComponent<Image>();
            if (backgroundImage != null)
            {
                backgroundImage.color = Inactive;
            }

            Transform tContent = transform.Find("DropDownContent");
            if (tContent != null)
            {
                DropDownContent = tContent.gameObject;
                Transform tItem = tContent.Find("Item");
                if (tItem != null)
                {
                    entryPrefab = tItem.GetComponent<FToggle>() ?? tItem.gameObject.AddComponent<FToggle>();
                    entryPrefab.gameObject.SetActive(false);
                }

                Transform tButtonItem = tContent.Find("ButtonItem");
                if (tButtonItem != null)
                {
                    buttonEntryPrefab = tButtonItem.GetComponent<FButton>() ?? tButtonItem.gameObject.AddComponent<FButton>();
                    buttonEntryPrefab.gameObject.SetActive(false);
                }
            }

            InitializeDropDown();
        }

        public void InitializeDropDown()
        {
            if (DropDownEntries == null || DropDownContent == null)
            {
                return;
            }

            DropDownContent.SetActive(true);
            foreach (FDropDownEntry dropDownEntry in DropDownEntries)
            {
                if (dropDownEntry is FDropDownButtonEntry entry && buttonEntryPrefab != null)
                {
                    InitializeButton(entry);
                }
                else if (entryPrefab != null)
                {
                    InitializeToggle(dropDownEntry);
                }
            }
            DropDownContent.SetActive(false);
        }

        private void InitializeButton(FDropDownButtonEntry entry)
        {
            FButton fButton = global::Util.KInstantiateUI<FButton>(buttonEntryPrefab.gameObject, DropDownContent, true);
            fButton.OnClick += delegate
            {
                entry.OnToggled?.Invoke(true);
                if (RefreshUI != null)
                {
                    RefreshUI.Invoke();
                }
            };

            LocText lt = fButton.GetComponentInChildren<LocText>();
            if (lt != null)
            {
                lt.text = entry.Title;
            }

            if (!string.IsNullOrEmpty(entry.Description))
            {
                UIUtils.AddSimpleTooltipToObject(fButton.transform, entry.Description);
            }
            entry.Button = fButton;
        }

        private void InitializeToggle(FDropDownEntry entry)
        {
            FToggle fToggle = global::Util.KInstantiateUI<FToggle>(entryPrefab.gameObject, DropDownContent, true);
            fToggle.SetCheckmark("Background/Checkmark");
            fToggle.SetOnFromCode(entry.Enabled);
            fToggle.OnClick += delegate(bool on)
            {
                entry.OnToggled?.Invoke(on);
                if (RefreshUI != null)
                {
                    RefreshUI.Invoke();
                }
            };

            LocText lt = fToggle.GetComponentInChildren<LocText>();
            if (lt != null)
            {
                lt.text = entry.Title;
            }

            if (!string.IsNullOrEmpty(entry.Description))
            {
                UIUtils.AddSimpleTooltipToObject(fToggle.transform, entry.Description);
            }
            entry.Toggle = fToggle;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = OnHover;
            }
            if (DropDownContent != null)
            {
                DropDownContent.SetActive(true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = Inactive;
            }
            if (DropDownContent != null)
            {
                DropDownContent.SetActive(false);
            }
        }
    }
}
