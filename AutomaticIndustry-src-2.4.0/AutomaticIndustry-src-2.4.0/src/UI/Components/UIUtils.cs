// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using UnityEngine;

namespace AutoMachineRebuilt.UI.Components
{
    public static class UIUtils
    {
        public static Color rgb(float r, float g, float b)
        {
            return new Color(r / 255f, g / 255f, b / 255f);
        }

        public static Color rgba(float r, float g, float b, double a)
        {
            return new Color(r / 255f, g / 255f, b / 255f, (float)a);
        }

        public static Color Lighten(Color color, float percentage)
        {
            float h, s, v;
            Color.RGBToHSV(color, out h, out s, out v);
            v = Mathf.Clamp01(v + percentage / 100f);
            return Color.HSVToRGB(h, s, v);
        }

        public static ToolTip AddSimpleTooltipToObject(Transform t, string tooltip, bool alignCenter = true, float wrapWidth = 0f, bool onBottom = true)
        {
            if (t == null) return null;
            return AddSimpleTooltipToObject(t.gameObject, tooltip, alignCenter, wrapWidth, onBottom);
        }

        public static ToolTip AddSimpleTooltipToObject(GameObject go, string tooltip, bool alignCenter = true, float wrapWidth = 0f, bool onBottom = true)
        {
            if (go == null) return null;

            ToolTip tt = go.GetComponent<ToolTip>() ?? go.AddComponent<ToolTip>();
            tt.UseFixedStringKey = false;
            tt.enabled = true;
            tt.tooltipPivot = alignCenter ? new Vector2(0.5f, onBottom ? 1f : 0f) : new Vector2(1f, onBottom ? 1f : 0f);
            tt.tooltipPositionOffset = onBottom ? new Vector2(0f, -20f) : new Vector2(0f, 20f);
            tt.parentPositionAnchor = new Vector2(0.5f, 0.5f);
            if (wrapWidth > 0f)
            {
                tt.WrapWidth = wrapWidth;
                tt.SizingSetting = ToolTip.ToolTipSizeSetting.MaxWidthWrapContent;
            }

            if (ToolTipScreen.Instance != null)
            {
                ToolTipScreen.Instance.SetToolTip(tt);
            }

            tt.SetSimpleTooltip(tooltip);
            return tt;
        }
    }
}
