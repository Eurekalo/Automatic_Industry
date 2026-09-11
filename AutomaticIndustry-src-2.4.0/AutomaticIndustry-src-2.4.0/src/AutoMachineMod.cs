// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.
// Original mod concept: "AutoMachine" (Steam Workshop id 2992024030).

using AutoMachineRebuilt.Config;
using AutoMachineRebuilt.Localization;
using AutoMachineRebuilt.Patches;
using AutoMachineRebuilt.Util;
using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Database;
using PeterHan.PLib.Options;

namespace AutoMachineRebuilt
{
    /// <summary>
    /// Entry point of the mod. Initialises PLib, registers the options screen
    /// and lets Harmony apply every annotated patch of this assembly.
    /// </summary>
    public sealed class AutoMachineMod : UserMod2
    {
        /// <summary>Called by the game when the assembly is loaded.</summary>
        /// <param name="harmony">Harmony instance dedicated to this mod.</param>
        public override void OnLoad(Harmony harmony)
        {
            try
            {
                base.OnLoad(harmony);
            }
            catch (System.Exception ex)
            {
                Log.Error("Non-fatal: one or more Harmony auto-patches failed to apply during PatchAll", ex);
            }

            PUtil.InitLibrary(false);

            // Register the mod's LocString keys BEFORE the options screen is
            // built. Without this the options dialog falls back to printing
            // the raw string keys, which also breaks its layout.
            SafeInvoke.Try("Registering mod strings", RegisterStrings);

            new POptions().RegisterOptions(this, typeof(AutoMachineOptions));
            new PLocalization().Register();

            // Apply the configured language immediately; the mods screen
            // patch below refreshes it every time the player opens options.
            OptionTextBinder.Apply();

            // Report an incomplete or mixed up translation table in the log
            // instead of letting it surface as a wrongly localized option row.
            LocalizationAudit.Run();

            // Self-register with ONIModFramework if present
            AutoMachineRebuilt.Integration.FrameworkIntegration.Register();

            // Auto-register with ONI Together (multiplayer mod) if present
            AutoMachineRebuilt.Integration.Multiplayer.MultiplayerManager.Initialize();

            // Compatibility shims for other community mods (e.g. No Manual Delivery, Customize Buildings)
            SafeInvoke.Try("NoManualDelivery compatibility shim (OnLoad)", delegate
            {
                NoManualDeliveryCompatibility.Patch(harmony);
            });
            SafeInvoke.Try("CustomizeBuildings compatibility shim (OnLoad)", delegate
            {
                CustomizeBuildingsCompatibility.Apply(harmony);
            });
            SafeInvoke.Try("ChemicalProcessing compatibility shim (OnLoad)", delegate
            {
                ChemicalProcessingCompatibility.Apply(harmony);
            });
            SafeInvoke.Try("SymbolOverrideController compatibility shim (OnLoad)", delegate
            {
                SymbolOverrideControllerCompatibility.Apply(harmony);
            });
            SafeInvoke.Try("OptionsDialog layout fix patch (OnLoad)", delegate
            {
                OptionsDialogLayoutFixPatch.Apply(harmony);
            });

            Log.Info("Loaded. Automation options can be changed in the mod settings.");
        }



        /// <summary>Creates the string table entries of this mod.</summary>
        private static void RegisterStrings()
        {
            LocString.CreateLocStringKeys(typeof(STRINGS.AUTOMACHINEREBUILT), "STRINGS.AUTOMACHINEREBUILT.");
        }
    }

    /// <summary>
    /// Replaces the option titles with the vanilla building names once the
    /// game database (and therefore every building string) is available.
    /// </summary>
    [HarmonyPatch(typeof(Db), "Initialize")]
    internal static class DbInitializePatch
    {
        /// <summary>Binds option texts after the database is built.</summary>
        internal static void Postfix()
        {
            SafeInvoke.Try("Binding option texts after Db.Initialize", OptionTextBinder.Apply);
            SafeInvoke.Try("NoManualDelivery compatibility shim (Db.Initialize)", delegate
            {
                Harmony harmony = new Harmony("AutoMachineRebuilt.Compatibility");
                NoManualDeliveryCompatibility.Patch(harmony);
            });
            SafeInvoke.Try("CustomizeBuildings compatibility shim (Db.Initialize)", delegate
            {
                Harmony harmony = new Harmony("AutoMachineRebuilt.Compatibility");
                CustomizeBuildingsCompatibility.Apply(harmony);
            });
            SafeInvoke.Try("ChemicalProcessing compatibility shim (Db.Initialize)", delegate
            {
                Harmony harmony = new Harmony("AutoMachineRebuilt.Compatibility");
                ChemicalProcessingCompatibility.Apply(harmony);
            });
            SafeInvoke.Try("SymbolOverrideController compatibility shim (Db.Initialize)", delegate
            {
                Harmony harmony = new Harmony("AutoMachineRebuilt.Compatibility");
                SymbolOverrideControllerCompatibility.Apply(harmony);
            });
        }
    }

    /// <summary>
    /// Refreshes the option texts every time the mods screen is opened so a
    /// language change takes effect without restarting the game.
    ///
    /// The target is resolved by name because referencing the screen type
    /// directly would pull the Unity UI assemblies into this mod.
    /// </summary>
    [HarmonyPatch]
    internal static class ModsScreenActivatePatch
    {
        /// <summary>Only patch when the screen type exists.</summary>
        internal static bool Prepare()
        {
            return TargetMethod() != null;
        }

        /// <summary>Resolves <c>ModsScreen.OnActivate</c> at runtime.</summary>
        internal static System.Reflection.MethodBase TargetMethod()
        {
            System.Type screen = AccessTools.TypeByName("ModsScreen");
            return screen == null ? null : AccessTools.Method(screen, "OnActivate");
        }

        /// <summary>Re-applies the localized option texts.</summary>
        internal static void Postfix()
        {
            SafeInvoke.Try("Refreshing option texts", OptionTextBinder.Apply);
        }
    }
}
