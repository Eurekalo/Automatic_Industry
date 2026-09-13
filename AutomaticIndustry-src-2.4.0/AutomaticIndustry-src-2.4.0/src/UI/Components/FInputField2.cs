// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AutoMachineRebuilt.UI.Components
{
    public class FInputField2 : KMonoBehaviour
    {
        [MyCmpReq]
        public TMP_InputField inputField;

        [SerializeField]
        public string textPath = "Text";

        [SerializeField]
        public string placeHolderPath = "Placeholder";

        private bool initialized;
#pragma warning disable CS0414
        private bool dataTextUpdate;
#pragma warning restore CS0414

        public string Text
        {
            get => inputField != null ? inputField.text : string.Empty;
            set
            {
                EnsureInitialized();
                if (inputField != null)
                {
                    inputField.text = value;
                }
            }
        }

        public TMP_InputField.OnChangeEvent OnValueChanged => inputField != null ? inputField.onValueChanged : null;

        public void AddListener(UnityAction<string> action)
        {
            if (inputField != null && action != null)
            {
                inputField.onValueChanged.AddListener(action);
            }
        }

        private void EnsureInitialized()
        {
            if (!initialized && inputField != null)
            {
                if (inputField.textViewport != null)
                {
                    Transform tText = inputField.textViewport.transform.Find(textPath);
                    if (tText != null)
                    {
                        inputField.textComponent = tText.GetComponent<LocText>() ?? tText.gameObject.AddComponent<LocText>();
                    }

                    Transform tPlaceholder = inputField.textViewport.transform.Find(placeHolderPath);
                    if (tPlaceholder != null)
                    {
                        inputField.placeholder = tPlaceholder.GetComponent<LocText>() ?? tPlaceholder.gameObject.AddComponent<LocText>();
                    }
                }
                initialized = true;
            }
        }

        public void SetTextFromData(string newText, bool forceRefresh = false)
        {
            dataTextUpdate = true;
            Text = newText;
            if (forceRefresh && inputField != null)
            {
                inputField.ForceLabelUpdate();
            }
            dataTextUpdate = false;
        }
    }
}
