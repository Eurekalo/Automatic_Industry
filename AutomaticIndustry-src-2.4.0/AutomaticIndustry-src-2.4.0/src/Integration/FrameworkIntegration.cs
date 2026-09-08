// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System;
using System.Reflection;
using AutoMachineRebuilt.Config;
using AutoMachineRebuilt.Localization;
using AutoMachineRebuilt.Patches;
using AutoMachineRebuilt.Util;
using PeterHan.PLib.Options;

namespace AutoMachineRebuilt.Integration
{
    /// <summary>
    /// Self-contained integration module on the Automatic Industry side.
    /// Safely bridges metadata, options, and live configuration change notifications
    /// to ONIModFramework when present, with zero hardcoding required in ModMenu.
    /// </summary>
    public static class FrameworkIntegration
    {
        public static void Register()
        {
            try
            {
                // Detect ONIModFramework via reflection to prevent strict compile-time dependency
                Type bootstrapType = Type.GetType("ONIModFramework.API.Core.FrameworkBootstrap, ModMenu") ??
                                     FindTypeAcrossAssemblies("ONIModFramework.API.Core.FrameworkBootstrap");

                if (bootstrapType == null)
                {
                    Log.Info("ONIModFramework not detected. Operating in standalone PLib mode.");
                    return;
                }

                // Initialize Framework
                MethodInfo initMethod = bootstrapType.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static);
                initMethod?.Invoke(null, null);

                // Get Context
                PropertyInfo contextProp = bootstrapType.GetProperty("Context", BindingFlags.Public | BindingFlags.Static);
                object context = contextProp?.GetValue(null, null);
                if (context == null) return;

                // 1. Register Metadata
                RegisterModMetadata(context);

                // 2. Subscribe to ConfigChanged events
                SubscribeToConfigEvents(context);

                Log.Info("Successfully registered Automatic Industry with ONIModFramework.");
            }
            catch (Exception ex)
            {
                Log.Warn("Optional ONIModFramework integration skipped: " + ex.Message);
            }
        }

        private static void RegisterModMetadata(object context)
        {
            try
            {
                PropertyInfo modsProp = context.GetType().GetProperty("Mods");
                object modRegistry = modsProp?.GetValue(context, null);
                if (modRegistry == null) return;

                Type metadataType = Type.GetType("ONIModFramework.API.Metadata.ModMetadata, ModMenu") ??
                                    FindTypeAcrossAssemblies("ONIModFramework.API.Metadata.ModMetadata");
                if (metadataType == null) return;

                object metaObj = Activator.CreateInstance(metadataType);
                SetProperty(metaObj, "StaticId", "AutomaticIndustry");
                SetProperty(metaObj, "DisplayName", "Automatic Industry");
                SetProperty(metaObj, "Version", "2.4.35");
                SetProperty(metaObj, "Description", "Automates industrial and utility buildings in Oxygen Not Included.");
                SetProperty(metaObj, "Author", "AutoMachine Rebuilt contributors");
                SetProperty(metaObj, "IsEnabled", true);
                SetProperty(metaObj, "Capabilities", new string[] { "Configuration", "ConfigScreen", "Automation" });

                MethodInfo regMethod = modRegistry.GetType().GetMethod("Register", new Type[] { metadataType });
                regMethod?.Invoke(modRegistry, new object[] { metaObj });
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to register mod metadata: " + ex.Message);
            }
        }

        private static void SubscribeToConfigEvents(object context)
        {
            try
            {
                PropertyInfo eventsProp = context.GetType().GetProperty("Events");
                object eventsObj = eventsProp?.GetValue(context, null);
                if (eventsObj == null) return;

                EventInfo configEvent = eventsObj.GetType().GetEvent("ConfigChanged");
                if (configEvent != null)
                {
                    Action<string, object> handler = OnFrameworkConfigChanged;
                    configEvent.AddEventHandler(eventsObj, handler);
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to subscribe to framework config events: " + ex.Message);
            }
        }

        private static void OnFrameworkConfigChanged(string staticId, object newConfig)
        {
            if (string.IsNullOrEmpty(staticId)) return;

            if (staticId.Equals("AutomaticIndustry", StringComparison.OrdinalIgnoreCase))
            {
                Log.Info("Automatic Industry received live config update from framework.");
                try
                {
                    HideWorldStatusIconsPatch.ApplyIconVisibility();
                    OptionTextBinder.Apply();
                }
                catch (Exception ex)
                {
                    Log.Warn("Error applying live config update: " + ex.Message);
                }
            }
        }

        private static Type FindTypeAcrossAssemblies(string typeFullName)
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.IsDynamic) continue;
                try
                {
                    Type t = asm.GetType(typeFullName);
                    if (t != null) return t;
                }
                catch { }
            }
            return null;
        }

        private static void SetProperty(object target, string propName, object value)
        {
            try
            {
                PropertyInfo pi = target?.GetType().GetProperty(propName);
                pi?.SetValue(target, value, null);
            }
            catch { }
        }
    }
}
