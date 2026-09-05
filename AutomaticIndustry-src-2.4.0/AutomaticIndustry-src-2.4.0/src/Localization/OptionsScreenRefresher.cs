// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System;
using AutoMachineRebuilt.Config;
using AutoMachineRebuilt.Util;
using PeterHan.PLib.Options;

namespace AutoMachineRebuilt.Localization
{
    /// <summary>
    /// Rebuilds the mod options dialog after a language change.
    ///
    /// PLib builds every option row once, when the dialog is created, and
    /// resolves the labels at that moment. Re-binding the string table alone
    /// therefore has no visible effect on a dialog that is already on screen.
    /// Reopening the dialog is the only supported way to show the new labels
    /// immediately; it is a no-op outside of the mods screen.
    /// </summary>
    internal static class OptionsScreenRefresher
    {
        /// <summary>Guards against reopening the dialog recursively.</summary>
        private static bool reopening;

        /// <summary>
        /// Closes the confirmed dialog and shows a freshly built one that
        /// uses the newly selected language.
        /// </summary>
        internal static void Reopen()
        {
            if (reopening)
            {
                return;
            }

            SafeInvoke.Try("Reopening the options dialog", delegate
            {
                reopening = true;
                try
                {
                    POptions.ShowDialog(typeof(AutoMachineOptions), OnClosed);
                }
                finally
                {
                    reopening = false;
                }
            });
        }

        /// <summary>
        /// Applies the settings the player confirms in the rebuilt dialog.
        /// PLib only raises <see cref="IOptions.OnOptionsChanged"/> for the
        /// dialog it opened itself, so the mod does it here.
        /// </summary>
        /// <param name="result">Options instance returned by the dialog.</param>
        private static void OnClosed(object result)
        {
            AutoMachineOptions options = result as AutoMachineOptions;
            if (options == null)
            {
                return;
            }

            options.OnOptionsChanged();
        }
    }
}
