// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AutoMachineRebuilt.UI.Components
{
    public class FButton : KMonoBehaviour, IPointerUpHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public bool PlayClickSound = true;
        private bool interactable = true;
        private Material material;

#pragma warning disable CS0649
        [MyCmpGet]
        private Image image;

        [MyCmpGet]
        private Button button;
#pragma warning restore CS0649

        [SerializeField]
        public Color disabledColor = new Color(0.78f, 0.78f, 0.78f);

        [SerializeField]
        public Color normalColor = new Color(0.243f, 0.263f, 0.341f);

        [SerializeField]
        public Color hoverColor = new Color(0.345f, 0.373f, 0.702f);

        public bool allowRightClick;

        public event System.Action OnClick;
        public event System.Action OnRightClick;
        public event System.Action OnPointerEnterAction;
        public event System.Action OnPointerExitAction;

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            if (button != null && button.image != null)
            {
                image = button.image;
            }
            if (image != null)
            {
                material = image.material;
            }
            interactable = true;
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            if (button != null)
            {
                button.navigation = GetNoNavigation();
            }
        }

        private Navigation GetNoNavigation()
        {
            Navigation result = default(Navigation);
            result.mode = Navigation.Mode.None;
            result.wrapAround = false;
            return result;
        }

        public void SetInteractable(bool interactable)
        {
            if (this == null || (button == null && image == null) || interactable == this.interactable)
            {
                return;
            }
            this.interactable = interactable;
            if (button == null)
            {
                if (image != null)
                {
                    image.color = interactable ? normalColor : disabledColor;
                }
            }
            else
            {
                button.interactable = interactable;
            }
        }

        public void ClearOnClick()
        {
            OnClick = null;
            OnRightClick = null;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!interactable || !KInputManager.isFocused)
            {
                return;
            }
            KInputManager.SetUserActive();
            if (!eventData.dragging)
            {
                if (button != null)
                {
                    button.OnDeselect(null);
                }
                if (OnRightClick != null && eventData.button == PointerEventData.InputButton.Right)
                {
                    OnRightClick.Invoke();
                }
                else if (OnClick != null && (eventData.button == PointerEventData.InputButton.Left || allowRightClick))
                {
                    OnClick.Invoke();
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnPointerEnterAction?.Invoke();
            if (interactable && KInputManager.isFocused)
            {
                if (button == null && image != null)
                {
                    image.color = hoverColor;
                }
                KInputManager.SetUserActive();
                KMonoBehaviour.PlaySound(UISoundHelper.MouseOver);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnPointerExitAction?.Invoke();
            if (button == null && image != null)
            {
                image.color = normalColor;
            }
            else if (button != null)
            {
                button.OnDeselect(null);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (interactable && KInputManager.isFocused)
            {
                KInputManager.SetUserActive();
                if (PlayClickSound && ((OnClick != null && (eventData.button == PointerEventData.InputButton.Left || allowRightClick)) ||
                                       (OnRightClick != null && eventData.button == PointerEventData.InputButton.Right)))
                {
                    KMonoBehaviour.PlaySound(UISoundHelper.ClickOpen);
                }
            }
        }
    }
}
