// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System;

namespace AutoMachineRebuilt.Util
{
    /// <summary>
    /// Wraps automation callbacks so a single failing building can never take
    /// down the simulation. Each failure is logged once per call site.
    /// </summary>
    internal static class SafeInvoke
    {
        /// <summary>Runs <paramref name="action"/> and swallows any exception.</summary>
        /// <param name="context">Human readable description used in the log.</param>
        /// <param name="action">Work to execute.</param>
        /// <returns><c>true</c> when the action completed without throwing.</returns>
        internal static bool Try(string context, System.Action action)
        {
            if (action == null)
            {
                return false;
            }

            try
            {
                action();
                return true;
            }
            catch (Exception e)
            {
                Log.Error("Automation step failed: " + context, e);
                return false;
            }
        }
    }
}
