// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AutoMachineRebuilt.UI.Components
{
    public class FToggle : KMonoBehaviour, IPointerDownHandler, IPointerEnterHandler
    {
        [SerializeField]
        public Image mark;

        private bool _interactable = true;
        private bool on;

        public bool Interactable => _interactable;

        public bool On
        {
            get => on;
            set
            {
                on = value;
                if (mark != null && Interactable)
                {
                    mark.enabled = value;
                    OnChange?.Invoke(value);
                }
            }
        }

        public event System.Action<bool> OnClick;
        public event System.Action<bool> OnChange;

        public void SetInteractable(bool interactable)
        {
            _interactable = interactable;
            if (mark != null)
            {
                mark.color = _interactable ? Color.white : new Color(1f, 1f, 1f, 0.5f);
            }
        }

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            if (mark == null)
            {
                mark = gameObject.GetComponentInChildren<Image>();
            }
        }

        public void SetCheckmark(string path)
        {
            Transform t = transform.Find(path);
            if (t != null)
            {
                mark = t.GetComponent<Image>();
            }
        }

        public void Toggle()
        {
            On = !On;
        }

        public void SetOn(bool toggleOn)
        {
            On = toggleOn;
        }

        public void SetOnFromCode(bool setOn)
        {
            on = setOn;
            if (mark != null)
            {
                mark.enabled = on;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (KInputManager.isFocused && Interactable)
            {
                KInputManager.SetUserActive();
                KMonoBehaviour.PlaySound(UISoundHelper.Click);
                Toggle();
                OnClick?.Invoke(On);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Interactable && KInputManager.isFocused)
            {
                KInputManager.SetUserActive();
                KMonoBehaviour.PlaySound(UISoundHelper.MouseOver);
            }
        }
    }
}
