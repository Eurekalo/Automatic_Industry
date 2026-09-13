// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using AutoMachineRebuilt.UI.TMP;
using AutoMachineRebuilt.Util;
using UnityEngine;

namespace AutoMachineRebuilt.UI
{
    public static class BuildingEditorAssets
    {
        public static GameObject BuildingEditorWindowPrefab;
        private static AssetBundle loadedBundle;

        public static void LoadAssets()
        {
            if (BuildingEditorWindowPrefab != null) return;

            try
            {
                // Check if our own bundle is already loaded in Unity memory
                AssetBundle existing = null;
                try
                {
                    existing = AssetBundle.GetAllLoadedAssetBundles().FirstOrDefault(b => b != null && (b.name == "autoind_aio" || b.name == "ai_building_editor"));
                }
                catch { }

                loadedBundle = existing;

                if (loadedBundle == null)
                {
                    string assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string assetsDir = Path.Combine(assemblyDir, "assets");

                    string platformSubdir = "windows";
                    switch (Application.platform)
                    {
                        case RuntimePlatform.LinuxPlayer:
                            platformSubdir = "linux";
                            break;
                        case RuntimePlatform.OSXPlayer:
                            platformSubdir = "mac";
                            break;
                        default:
                            platformSubdir = "windows";
                            break;
                    }

                    string bundlePath = Path.Combine(Path.Combine(assetsDir, platformSubdir), "ai_building_editor");
                    if (!File.Exists(bundlePath))
                    {
                        string fallbackPath = Path.Combine(assetsDir, "ai_building_editor");
                        if (File.Exists(fallbackPath))
                        {
                            bundlePath = fallbackPath;
                        }
                    }

                    if (File.Exists(bundlePath))
                    {
                        loadedBundle = AssetBundle.LoadFromFile(bundlePath);
                    }
                    else
                    {
                        Log.Warn("Automatic Industry BuildingEditor AssetBundle not found at: " + bundlePath);
                        return;
                    }
                }

                if (loadedBundle != null)
                {
                    BuildingEditorWindowPrefab = loadedBundle.LoadAsset<GameObject>("Assets/AI_/BuildingEditor.prefab") ??
                                                 loadedBundle.LoadAsset<GameObject>("assets/ai_/buildingeditor.prefab") ??
                                                 loadedBundle.LoadAsset<GameObject>("BuildingEditor.prefab");

                    if (BuildingEditorWindowPrefab != null)
                    {
                        TMPConverter converter = new TMPConverter();
                        converter.ReplaceAllText(BuildingEditorWindowPrefab);
                        Log.Info("Successfully loaded and initialized Automatic Industry BuildingEditorWindowPrefab from AssetBundle.");
                    }
                    else
                    {
                        Log.Warn("BuildingEditor.prefab not found inside Automatic Industry AssetBundle.");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Exception loading Automatic Industry BuildingEditor AssetBundle: " + ex.Message);
            }
        }
    }
}
