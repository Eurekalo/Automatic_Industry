// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Reflection;
using AutoMachineRebuilt.Config;
using AutoMachineRebuilt.Util;

namespace AutoMachineRebuilt.Localization
{
    /// <summary>
    /// Writes every mod owned option text into the game string table.
    ///
    /// PLib resolves option titles and tooltips through
    /// <c>Strings.TryGet</c>. Without this binder the raw keys
    /// (<c>STRINGS.AUTOMACHINEREBUILT....</c>) end up on screen, which also
    /// blows up the row width and pushes the checkboxes out of the dialog.
    ///
    /// Building names are copied from the running game so they always match
    /// vanilla wording; the mod only supplies its own categories, options,
    /// tooltips and enum labels.
    /// </summary>
    internal static class OptionTextBinder
    {
        /// <summary>Prefix shared by every mod string key.</summary>
        private const string KeyPrefix = "STRINGS.AUTOMACHINEREBUILT.";

        /// <summary>Maximum length of a title before it is trimmed.</summary>
        private const int MaxTitleLength = 64;

        /// <summary>
        /// Applies all option texts using the configured language. Safe to
        /// call repeatedly; every call simply overwrites the entries.
        /// </summary>
        internal static void Apply()
        {
            SafeInvoke.Try("Binding option texts", ApplyCore);
        }

        /// <summary>Performs the actual string binding.</summary>
        private static void ApplyCore()
        {
            AutoMachineOptions options = AutoMachineOptions.Instance ?? new AutoMachineOptions();
            UiLanguage language = Resolve(options.OptionsLanguage);
            bool bilingual = options.BilingualLabels && language != UiLanguage.English;

            Dictionary<string, string> table = Translations.For(language);

            foreach (KeyValuePair<string, string> entry in Translations.English)
            {
                string translated;
                if (!table.TryGetValue(entry.Key, out translated) || string.IsNullOrEmpty(translated))
                {
                    translated = entry.Value;
                }

                bool isTooltip = entry.Key.StartsWith("TOOLTIP.", StringComparison.Ordinal);
                string text = Combine(entry.Value, translated, bilingual, isTooltip);
                Strings.Add(KeyPrefix + entry.Key, text);
            }

            ApplyBuildingDescriptions(language, bilingual);
            BuildingNameBinder.Apply(bilingual, language);

            // Remember what the labels were built with, so the options dialog
            // is only rebuilt when the player really changed the language or
            // the bilingual toggle.
            AutoMachineOptions.NoteAppliedLabels(options.OptionsLanguage, options.BilingualLabels);
        }

        /// <summary>
        /// Writes the per building help texts. Every automated building has
        /// its own description of the vanilla mechanic and of what the
        /// automation adds, instead of the generic tooltip the option rows
        /// used to share.
        /// </summary>
        /// <param name="language">Resolved options language.</param>
        /// <param name="bilingual">Whether English should be shown as well.</param>
        private static void ApplyBuildingDescriptions(UiLanguage language, bool bilingual)
        {
            Dictionary<string, string> table = BuildingDescriptions.DescriptionsFor(language);

            foreach (KeyValuePair<string, string> entry in BuildingDescriptions.DescriptionsEnglish)
            {
                string translated;
                if (!table.TryGetValue(entry.Key, out translated) || string.IsNullOrEmpty(translated))
                {
                    translated = entry.Value;
                }

                Strings.Add(KeyPrefix + entry.Key,
                    Combine(entry.Value, translated, bilingual, true));
            }
        }

        /// <summary>
        /// Merges the English source text and its translation. Titles use a
        /// compact "English / translation" form, tooltips are stacked on two
        /// lines so long help texts stay readable.
        /// </summary>
        /// <param name="english">English source text.</param>
        /// <param name="translated">Localized text.</param>
        /// <param name="bilingual">Whether both texts should be shown.</param>
        /// <param name="isTooltip">Whether the text is a tooltip.</param>
        internal static string Combine(string english, string translated, bool bilingual, bool isTooltip)
        {
            if (!bilingual || string.IsNullOrEmpty(english) ||
                string.Equals(english, translated, StringComparison.Ordinal))
            {
                return isTooltip ? translated : Trim(translated);
            }

            if (isTooltip)
            {
                return translated + "\n" + english;
            }

            return Trim(english + " / " + translated);
        }

        /// <summary>Keeps titles short so option rows cannot overflow.</summary>
        /// <param name="text">Title candidate.</param>
        private static string Trim(string text)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= MaxTitleLength)
            {
                return text;
            }

            return text.Substring(0, MaxTitleLength - 1) + "…";
        }

        /// <summary>
        /// Turns <see cref="UiLanguage.Auto"/> into a concrete language by
        /// inspecting the locale the game is running in. Any failure falls
        /// back to English.
        /// </summary>
        /// <param name="configured">Language chosen by the player.</param>
        internal static UiLanguage Resolve(UiLanguage configured)
        {
            if (configured != UiLanguage.Auto)
            {
                return configured;
            }

            string code = GetGameLocaleCode();
            if (string.IsNullOrEmpty(code))
            {
                return UiLanguage.English;
            }

            code = code.Replace('_', '-').ToLowerInvariant();

            if (code.StartsWith("zh-tw", StringComparison.Ordinal) ||
                code.StartsWith("zh-hant", StringComparison.Ordinal) ||
                code.StartsWith("zh-hk", StringComparison.Ordinal))
            {
                return UiLanguage.ChineseTraditional;
            }

            if (code.StartsWith("zh", StringComparison.Ordinal))
            {
                return UiLanguage.ChineseSimplified;
            }

            if (code.StartsWith("ko", StringComparison.Ordinal))
            {
                return UiLanguage.Korean;
            }

            if (code.StartsWith("ja", StringComparison.Ordinal))
            {
                return UiLanguage.Japanese;
            }

            return UiLanguage.English;
        }

        /// <summary>
        /// Whether the game itself currently runs in a language, which makes
        /// its own string table the authoritative source for vanilla wording.
        /// </summary>
        /// <param name="language">Language to compare the game locale with.</param>
        internal static bool GameRunsIn(UiLanguage language)
        {
            return Resolve(UiLanguage.Auto) == language;
        }

        /// <summary>
        /// Reads the active locale code by checking KPlayerPrefs, Localization.sLocale,
        /// and inspecting active string table entries.
        /// </summary>
        private static string GetGameLocaleCode()
        {
            try
            {
                // 1. Check KPlayerPrefs first (where ONI stores user's chosen language code)
                string prefCode = KPlayerPrefs.GetString(global::Localization.SELECTED_LANGUAGE_CODE_KEY, null);
                if (!string.IsNullOrEmpty(prefCode))
                {
                    return prefCode;
                }

                // 2. Check Localization.sLocale private static field
                FieldInfo sLocaleField = typeof(global::Localization).GetField("sLocale", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
                if (sLocaleField != null)
                {
                    object locale = sLocaleField.GetValue(null);
                    if (locale != null)
                    {
                        FieldInfo codeField = locale.GetType().GetField("mCode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (codeField != null)
                        {
                            string c = codeField.GetValue(locale) as string;
                            if (!string.IsNullOrEmpty(c)) return c;
                        }
                    }
                }

                // 3. Fallback: inspect actual vanilla string translations in game string table
                StringEntry entry;
                if (Strings.TryGet("STRINGS.BUILDINGS.PREFABS.DESALINATOR.NAME", out entry) && !string.IsNullOrEmpty(entry.String))
                {
                    string name = entry.String;
                    if (name.Contains("脱盐")) return "zh";
                    if (name.Contains("脫鹽")) return "zh-tw";
                    if (name.Contains("탈염")) return "ko";
                    if (name.Contains("脱塩")) return "ja";
                }

                if (Strings.TryGet("STRINGS.BUILDINGS.PREFABS.RESEARCHCENTER.NAME", out entry) && !string.IsNullOrEmpty(entry.String))
                {
                    string name = entry.String;
                    if (name.Contains("研究站") || name.Contains("研究")) return "zh";
                    if (name.Contains("연구")) return "ko";
                }
            }
            catch (Exception e)
            {
                Log.Verbose("Could not read the game locale: " + e.Message);
            }
            return null;
        }
    }
}
