// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.
// Original mod concept: "AutoMachine" (Steam Workshop id 2992024030).

using System.Collections.Generic;
using AutoMachineRebuilt.Automation;
using AutoMachineRebuilt.Config;
using AutoMachineRebuilt.Util;

namespace AutoMachineRebuilt.Localization
{
    /// <summary>
    /// Self check of the localization layer.
    ///
    /// The mod ships five languages and three text tables (option texts,
    /// per building descriptions, building names). A missing or a misfiled
    /// row used to surface as an option row written in the wrong language, or
    /// as a raw string key on screen. The audit compares every table against
    /// the English table, which is the single source of truth for the key set
    /// and the base text of every bilingual label, and verifies that each
    /// translated value is actually written in the script of its language.
    ///
    /// The audit never throws and never changes a string; it only reports, so
    /// a packaging mistake is visible in Player.log instead of on screen.
    /// </summary>
    internal static class LocalizationAudit
    {
        /// <summary>Languages the mod ships, English excluded.</summary>
        private static readonly UiLanguage[] Translated =
        {
            UiLanguage.ChineseSimplified,
            UiLanguage.ChineseTraditional,
            UiLanguage.Korean,
            UiLanguage.Japanese
        };

        /// <summary>Runs every check and reports the result once.</summary>
        internal static void Run()
        {
            SafeInvoke.Try("Auditing the localization tables", RunCore);
        }

        /// <summary>Performs the audit.</summary>
        private static void RunCore()
        {
            int problems = 0;

            foreach (UiLanguage language in Translated)
            {
                problems += CompareKeys("option texts", language,
                                        Translations.English, Translations.For(language));
                problems += CompareKeys("building descriptions", language,
                                        BuildingDescriptions.DescriptionsEnglish,
                                        BuildingDescriptions.DescriptionsFor(language));
                problems += CheckScript("option texts", language, Translations.For(language));
                problems += CheckScript("building descriptions", language,
                                        BuildingDescriptions.DescriptionsFor(language));
                problems += CheckNames(language);
            }

            problems += CheckRegistryCoverage();

            if (problems == 0)
            {
                Log.Verbose("Localization audit passed: every language is complete and consistent.");
            }
            else
            {
                Log.Warn("Localization audit found " + problems +
                         " issue(s); the affected rows fall back to English.");
            }
        }

        /// <summary>
        /// Reports rows that the English table defines and a translation does
        /// not, and rows a translation defines on its own.
        /// </summary>
        /// <param name="table">Human readable table name.</param>
        /// <param name="language">Translated language.</param>
        /// <param name="english">English reference table.</param>
        /// <param name="translated">Table of the translated language.</param>
        private static int CompareKeys(string table, UiLanguage language,
                                       Dictionary<string, string> english,
                                       Dictionary<string, string> translated)
        {
            if (english == null || translated == null)
            {
                return 0;
            }

            int problems = 0;

            foreach (KeyValuePair<string, string> row in english)
            {
                string value;
                if (!translated.TryGetValue(row.Key, out value) || string.IsNullOrEmpty(value))
                {
                    problems++;
                    Log.Warn("Missing " + language + " " + table + " entry: " + row.Key);
                }
            }

            foreach (KeyValuePair<string, string> row in translated)
            {
                if (!english.ContainsKey(row.Key))
                {
                    problems++;
                    Log.Warn("Unknown " + language + " " + table + " entry: " + row.Key);
                }
            }

            return problems;
        }

        /// <summary>
        /// Reports translated values that contain characters of a different
        /// language, which is how a copy and paste mistake between the
        /// Korean, Japanese and Chinese tables shows up.
        /// </summary>
        /// <param name="table">Human readable table name.</param>
        /// <param name="language">Translated language.</param>
        /// <param name="values">Table of the translated language.</param>
        private static int CheckScript(string table, UiLanguage language,
                                       Dictionary<string, string> values)
        {
            if (values == null)
            {
                return 0;
            }

            int problems = 0;

            foreach (KeyValuePair<string, string> row in values)
            {
                if (IsForeignScript(language, row.Value))
                {
                    problems++;
                    Log.Warn("The " + language + " " + table + " entry " + row.Key +
                             " contains characters of another language: " + row.Value);
                }
            }

            return problems;
        }

        /// <summary>Verifies that every language can name every building.</summary>
        /// <param name="language">Translated language.</param>
        private static int CheckNames(UiLanguage language)
        {
            Dictionary<string, string> names = BuildingDescriptions.NamesFor(language);
            if (names == null)
            {
                // The language is resolved from the running game instead of a
                // table; nothing to audit.
                return 0;
            }

            int problems = 0;

            foreach (KeyValuePair<string, string> row in VanillaBuildingNames.English)
            {
                string value;
                if (!names.TryGetValue(row.Key, out value) || string.IsNullOrEmpty(value))
                {
                    problems++;
                    Log.Warn("Missing " + language + " building name: " + row.Key);
                }
                else if (IsForeignScript(language, value))
                {
                    problems++;
                    Log.Warn("The " + language + " name of " + row.Key +
                             " contains characters of another language: " + value);
                }
            }

            return problems;
        }

        /// <summary>
        /// Verifies that every automated building has an English name, a
        /// description and therefore a readable option row.
        /// </summary>
        private static int CheckRegistryCoverage()
        {
            int problems = 0;

            foreach (AutomationEntry entry in AutomationRegistry.Entries)
            {
                if (!VanillaBuildingNames.English.ContainsKey(entry.OptionKey))
                {
                    problems++;
                    Log.Warn("Automated building without a vanilla name: " + entry.OptionKey);
                }

                if (!BuildingDescriptions.DescriptionsEnglish.ContainsKey(
                        BuildingDescriptions.DescriptionKeyPrefix + entry.OptionKey))
                {
                    problems++;
                    Log.Warn("Automated building without a description: " + entry.OptionKey);
                }
            }

            return problems;
        }

        /// <summary>
        /// Whether a value uses a writing system that cannot appear in the
        /// given language. Latin letters, digits and punctuation are ignored
        /// because every language uses them for units and numbers.
        /// </summary>
        /// <param name="language">Language the value belongs to.</param>
        /// <param name="value">Translated value.</param>
        private static bool IsForeignScript(UiLanguage language, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            bool kana = false;
            bool hangul = false;

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if ((c >= '\u3040' && c <= '\u30ff') || (c >= '\u31f0' && c <= '\u31ff'))
                {
                    kana = true;
                }
                else if ((c >= '\uac00' && c <= '\ud7af') || (c >= '\u1100' && c <= '\u11ff'))
                {
                    hangul = true;
                }
            }

            switch (language)
            {
                case UiLanguage.ChineseSimplified:
                case UiLanguage.ChineseTraditional:
                    return kana || hangul;
                case UiLanguage.Korean:
                    return kana;
                case UiLanguage.Japanese:
                    return hangul;
                default:
                    return false;
            }
        }
    }
}
