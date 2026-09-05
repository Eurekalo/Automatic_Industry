// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System;
using UnityEngine;
using AutoMachineRebuilt.Config;

namespace AutoMachineRebuilt.Util
{
    /// <summary>
    /// Central logging helper. Every message is prefixed so that players and
    /// mod maintainers can identify this mod in the Player.log file.
    ///
    /// Provides zero-allocation, fast boolean gated logging methods to completely
    /// eliminate performance overhead and log spam during normal gameplay.
    /// </summary>
    internal static class Log
    {
        private const string Prefix = "[AutoMachineRebuilt] ";

        /// <summary>Writes an informational message (used sparingly for startup/mod events).</summary>
        internal static void Info(string message)
        {
            Debug.Log(Prefix + message);
        }

        /// <summary>Writes a warning message.</summary>
        internal static void Warn(string message)
        {
            Debug.LogWarning(Prefix + message);
        }

        /// <summary>
        /// Writes a diagnostic message only when diagnostic logging is enabled.
        /// </summary>
        internal static void Diagnostic(string message)
        {
            if (AutoMachineOptions.Instance != null && AutoMachineOptions.Instance.EnableDiagnosticLogging)
            {
                Debug.Log(Prefix + "[Diagnostic] " + message);
            }
        }

        /// <summary>
        /// Writes a building automation event only when building automation logging is enabled.
        /// </summary>
        internal static void BuildingEvent(string optionKey, string message)
        {
            if (AutoMachineOptions.Instance != null && AutoMachineOptions.Instance.LogBuildingAutomation)
            {
                Debug.Log(Prefix + "[" + optionKey + "] " + message);
            }
        }

        /// <summary>
        /// Writes a detailed log for complex or fragile state machine buildings (e.g. Geotuner, Ranch, Refinery).
        /// </summary>
        internal static void Problematic(string optionKey, string message)
        {
            if (AutoMachineOptions.Instance != null && AutoMachineOptions.Instance.LogProblematicBuildings)
            {
                Debug.Log(Prefix + "[State/Issue] [" + optionKey + "] " + message);
            }
        }

        /// <summary>
        /// Writes a diagnostic message only while verbose logging is enabled.
        /// </summary>
        internal static void Verbose(string message)
        {
            if (AutoMachineOptions.Instance != null && AutoMachineOptions.Instance.VerboseLogging)
            {
                Debug.Log(Prefix + message);
            }
        }

        /// <summary>
        /// Writes an error message. Exceptions are never rethrown: a broken
        /// automation feature must not be able to crash the base game.
        /// </summary>
        internal static void Error(string message, Exception e = null)
        {
            Debug.LogError(Prefix + message + (e == null ? string.Empty : "\n" + e));
        }
    }
}
