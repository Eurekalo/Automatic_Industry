// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoMachineRebuilt.UI.TMP
{
    public class TMPConverter
    {
        private static TMP_FontAsset NotoSans;
        private static TMP_FontAsset GrayStroke;

        public TMPConverter()
        {
            Initialize();
        }

        public void Initialize()
        {
            List<TMP_FontAsset> source = new List<TMP_FontAsset>(Resources.FindObjectsOfTypeAll<TMP_FontAsset>());
            NotoSans = source.FirstOrDefault(f => f != null && f.name == "NotoSans-Regular");
            GrayStroke = source.FirstOrDefault(f => f != null && f.name == "GRAYSTROKE REGULAR SDF");
        }

        public void ReplaceAllText(GameObject parent, bool realign = true)
        {
            if (parent == null) return;
            Text[] components = parent.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < components.Length; i++)
            {
                Text val = components[i];
                if (val == null || (val.gameObject != null && val.gameObject.name == "SettingsDialogData"))
                {
                    continue;
                }

                string text = val.text;
                GameObject go = val.gameObject;
                TMPSettings settings = ExtractTMPData(text, val);
                if (settings != null && go != null)
                {
                    LocText locText = go.AddComponent<LocText>();
                    TMP_FontAsset targetFont = (settings.Font != null && settings.Font.Contains("GRAYSTROKE")) ? GrayStroke : NotoSans;
                    if (targetFont != null)
                    {
                        locText.font = targetFont;
                    }
                    locText.fontStyle = settings.FontStyle;
                    locText.fontSize = settings.FontSize > 0 ? settings.FontSize : 14f;
                    locText.maxVisibleLines = settings.MaxVisibleLines;
                    locText.textWrappingMode = settings.EnableWordWrapping ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
                    locText.text = "";
                    locText.overflowMode = settings.Overflow;
                    if (settings.Color != null && settings.Color.Length >= 3)
                    {
                        locText.color = new Color(settings.Color[0], settings.Color[1], settings.Color[2]);
                    }
                    locText.fontSizeMin = settings.VariableFontSizeMinimum;
                    locText.fontSizeMax = settings.VariableFontSizeMaximum;
                    if (!string.IsNullOrEmpty(settings.Content))
                    {
                        string cleanedKey = settings.Content.Replace(" ", string.Empty);
                        if (!cleanedKey.StartsWith("STRINGS.UI.BUILDINGEDITOR", StringComparison.OrdinalIgnoreCase))
                        {
                            locText.key = cleanedKey;
                        }
                    }

                    if (realign)
                    {
                        TMPImportFix fix = go.AddComponent<TMPImportFix>();
                        fix.alignment = settings.Alignment;
                        fix.textOverflow = settings.Overflow;
                        fix.fontSizeMin = settings.VariableFontSizeMinimum;
                        fix.fontSizeMax = settings.VariableFontSizeMaximum;
                        fix.autoResize = settings.VariableFontSize;
                    }
                }
            }
        }

        private static bool IsValidJson(string data)
        {
            if (string.IsNullOrEmpty(data)) return false;
            string trimmed = data.Trim();
            return trimmed.StartsWith("{") && trimmed.EndsWith("}");
        }

        private static TMPSettings ExtractTMPData(string tmpData, Text text)
        {
            TMPSettings result = null;
            if (IsValidJson(tmpData))
            {
                try
                {
                    result = JsonConvert.DeserializeObject<TMPSettings>(tmpData);
                }
                catch (Exception)
                {
                }
                if (text != null)
                {
                    UnityEngine.Object.DestroyImmediate(text);
                }
            }
            return result;
        }
    }
}
