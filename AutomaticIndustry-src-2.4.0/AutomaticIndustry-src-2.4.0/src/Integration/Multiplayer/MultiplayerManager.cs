// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System;
using System.Reflection;
using AutoMachineRebuilt.Util;
using ONI_Together.Networking;
using ONI_Together_API;
using ONI_Together_API.Networking;

namespace AutoMachineRebuilt.Integration.Multiplayer
{
    /// <summary>
    /// Coordinates integration with "ONI Together: A Multiplayer Mod".
    /// Provides session detection, packet auto-registration, and building automation sync.
    /// </summary>
    public static class MultiplayerManager
    {
        private static bool isInitialized;

        /// <summary>
        /// Gets whether ONI Together mod is detected and active in the current game instance.
        /// </summary>
        public static bool IsMultiplayerAvailable
        {
            get
            {
                try
                {
                    return MP_Mod_Info.MultiplayerModPresent;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets whether the game is currently inside an active multiplayer game session.
        /// </summary>
        public static bool IsInSession
        {
            get
            {
                try
                {
                    return SessionInfoAPI.InSession;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets whether local player is the host of the multiplayer session.
        /// </summary>
        public static bool IsHost
        {
            get
            {
                try
                {
                    return SessionInfoAPI.IsHost;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets whether local player is a client in the multiplayer session.
        /// </summary>
        public static bool IsClient
        {
            get
            {
                try
                {
                    return SessionInfoAPI.IsClient;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets the local player's network ID (Steam/Multiplayer ID).
        /// </summary>
        public static ulong LocalUserId
        {
            get
            {
                try
                {
                    return SessionInfoAPI.LocalUserID;
                }
                catch
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Gets the host's network ID.
        /// </summary>
        public static ulong HostUserId
        {
            get
            {
                try
                {
                    return SessionInfoAPI.HostUserID;
                }
                catch
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Initializes the multiplayer integration, auto-registering custom packets.
        /// </summary>
        public static void Initialize()
        {
            if (isInitialized) return;
            isInitialized = true;

            try
            {
                if (IsMultiplayerAvailable)
                {
                    PacketRegistryAPI.AutoRegisterAll(typeof(MultiplayerManager).Assembly);
                    Log.Info("[Multiplayer] ONI Together detected and packet sync registered successfully.");
                }
            }
            catch (Exception ex)
            {
                Log.Warn($"[Multiplayer] Failed to register packets with ONI Together: {ex.Message}");
            }
        }

        /// <summary>
        /// Broadcasts building automation customization state changes across the multiplayer network.
        /// </summary>
        /// <param name="cell">The building's root/primary grid cell.</param>
        /// <param name="prefabId">Building PrefabID name.</param>
        /// <param name="actionType">0: SetOverride, 1: CancelChore, 2: ResetToGlobalDefault</param>
        /// <param name="overrideState">0: Unset/Inherit, 1: Automated, 2: Manual</param>
        public static void SendBuildingSync(int cell, string prefabId, byte actionType, int overrideState)
        {
            if (!IsInSession) return;

            try
            {
                var packet = new BuildingAutomationSyncPacket(cell, prefabId, actionType, overrideState, LocalUserId);
                PacketSenderAPI.SendToAllOtherPeers(packet);
            }
            catch (Exception ex)
            {
                Log.Warn($"[Multiplayer] Failed to broadcast BuildingAutomationSyncPacket: {ex.Message}");
            }
        }
    }
}
