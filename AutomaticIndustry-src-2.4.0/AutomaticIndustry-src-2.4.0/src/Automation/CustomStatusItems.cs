// Copyright (c) 2026 Automatic Industry contributors. Licensed under the MIT License.
// Original mod concept: "AutoMachine" (Steam Workshop id 2992024030).

using System.Globalization;
using AutoMachineRebuilt.Components;
using UnityEngine;

namespace AutoMachineRebuilt.Automation
{
    /// <summary>
    /// Custom status items for buildings displaying operational countdowns using Geyser cycle formatting:
    /// 1. Compost: Next pitchfork flip countdown / flipping status (Cycles when > 100s, Seconds when <= 100s).
    /// 2. Oil Well: Time until pressure release threshold / venting status (Cycles when > 100s, Seconds when <= 100s).
    /// </summary>
    public static class CustomStatusItems
    {
        public static StatusItem CompostFlipCountdown;
        public static StatusItem OilWellPressureCountdown;

        public static void Register()
        {
            if (CompostFlipCountdown == null)
            {
                CompostFlipCountdown = new StatusItem(
                    id: "CompostFlipCountdown",
                    prefix: "BUILDING",
                    icon: "",
                    icon_type: StatusItem.IconType.Info,
                    notification_type: NotificationType.Neutral,
                    allow_multiples: false,
                    render_overlay: OverlayModes.None.ID,
                    showWorldIcon: false,
                    status_overlays: 129022,
                    resolve_string_callback: ResolveCompostString
                );
            }

            if (OilWellPressureCountdown == null)
            {
                OilWellPressureCountdown = new StatusItem(
                    id: "OilWellPressureCountdown",
                    prefix: "BUILDING",
                    icon: "",
                    icon_type: StatusItem.IconType.Info,
                    notification_type: NotificationType.Neutral,
                    allow_multiples: false,
                    render_overlay: OverlayModes.None.ID,
                    showWorldIcon: false,
                    status_overlays: 129022,
                    resolve_string_callback: ResolveOilWellString
                );
            }
        }

        /// <summary>
        /// Formats remaining duration into cycles (1 decimal place) if > 100 seconds,
        /// or integer seconds if <= 100 seconds, matching vanilla Geyser cycle time behavior.
        /// </summary>
        public static string FormatTimeOrCycles(float remSeconds, string code)
        {
            bool isZh = code.StartsWith("zh") || code == "zh-CN" || code == "zh-TW" || code == "zh-HK";
            bool isZhTw = code == "zh-TW" || code == "zh-HK";
            bool isKo = code.StartsWith("ko");
            bool isJa = code.StartsWith("ja");

            if (remSeconds > 100f)
            {
                float cycles = remSeconds / 600f;
                string cycleStr = cycles.ToString("0.0", CultureInfo.InvariantCulture);

                if (isZhTw) return $"{cycleStr} 週期";
                if (isZh) return $"{cycleStr} 周期";
                if (isKo) return $"{cycleStr} 주기";
                if (isJa) return $"{cycleStr} サイクル";
                return $"{cycleStr} cycles";
            }
            else
            {
                int seconds = Mathf.Max(0, Mathf.CeilToInt(remSeconds));

                if (isZhTw) return $"{seconds}秒";
                if (isZh) return $"{seconds}秒";
                if (isKo) return $"{seconds}초";
                if (isJa) return $"{seconds}秒";
                return $"{seconds}s";
            }
        }

        private static string ResolveCompostString(string str, object data)
        {
            if (data is CompostAutomationComponent compostComp)
            {
                string code = global::Localization.GetLocale()?.Code ?? "en";
                bool isZh = code.StartsWith("zh") || code == "zh-CN" || code == "zh-TW" || code == "zh-HK";
                bool isZhTw = code == "zh-TW" || code == "zh-HK";
                bool isKo = code.StartsWith("ko");
                bool isJa = code.StartsWith("ja");

                if (compostComp.IsFlipping)
                {
                    if (isZhTw) return "正在翻土...";
                    if (isZh) return "正在翻土...";
                    if (isKo) return "뒤집는 중...";
                    if (isJa) return "切り返し中...";
                    return "Flipping in progress...";
                }
                else
                {
                    float remTime = compostComp.GetRemainingFlipTime();
                    string timeFormatted = FormatTimeOrCycles(remTime, code);

                    if (isZhTw) return $"距離下次翻土: {timeFormatted}";
                    if (isZh) return $"距离下次翻土: {timeFormatted}";
                    if (isKo) return $"다음 뒤집기까지: {timeFormatted}";
                    if (isJa) return $"次の切り返しまで: {timeFormatted}";
                    return $"Next flip in: {timeFormatted}";
                }
            }
            return str;
        }

        private static string ResolveOilWellString(string str, object data)
        {
            if (data is AutoOilWellCap wellCapComp)
            {
                string code = global::Localization.GetLocale()?.Code ?? "en";
                bool isZh = code.StartsWith("zh") || code == "zh-CN" || code == "zh-TW" || code == "zh-HK";
                bool isZhTw = code == "zh-TW" || code == "zh-HK";
                bool isKo = code.StartsWith("ko");
                bool isJa = code.StartsWith("ja");

                if (wellCapComp.IsVenting)
                {
                    if (isZhTw) return "正在釋放積壓氣體...";
                    if (isZh) return "正在释放积压气体...";
                    if (isKo) return "압력 방출 중...";
                    if (isJa) return "圧力解放中...";
                    return "Venting pressure...";
                }
                else
                {
                    float remTime = wellCapComp.GetRemainingPressureTime();
                    string timeFormatted = FormatTimeOrCycles(remTime, code);

                    if (isZhTw) return $"距離需要壓力釋放: {timeFormatted}";
                    if (isZh) return $"距离需要压力释放: {timeFormatted}";
                    if (isKo) return $"압력 방출까지: {timeFormatted}";
                    if (isJa) return $"圧力解放まで: {timeFormatted}";
                    return $"Pressure release in: {timeFormatted}";
                }
            }
            return str;
        }
    }
}
