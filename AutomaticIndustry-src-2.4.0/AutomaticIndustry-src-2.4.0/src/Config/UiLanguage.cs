// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using PeterHan.PLib.Options;

namespace AutoMachineRebuilt.Config
{
    /// <summary>
    /// Language used by the mod's own option labels. Building names always
    /// come from the running game so they never drift from vanilla wording.
    /// The display labels are written in their own language on purpose, which
    /// keeps them readable no matter which language is currently active.
    /// </summary>
    public enum UiLanguage
    {
        /// <summary>Follow the language the game is running in.</summary>
        [Option("Auto")]
        Auto,

        /// <summary>Force English.</summary>
        [Option("English")]
        English,

        /// <summary>Force Simplified Chinese.</summary>
        [Option("简体中文")]
        ChineseSimplified,

        /// <summary>Force Traditional Chinese.</summary>
        [Option("繁體中文")]
        ChineseTraditional,

        /// <summary>Force Korean.</summary>
        [Option("한국어")]
        Korean,

        /// <summary>Force Japanese.</summary>
        [Option("日本語")]
        Japanese
    }
}
