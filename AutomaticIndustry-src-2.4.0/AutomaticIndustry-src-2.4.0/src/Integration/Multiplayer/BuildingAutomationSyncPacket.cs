// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System.IO;
using AutoMachineRebuilt.Components;
using AutoMachineRebuilt.Util;
using ONI_Together.Networking.Packets.Architecture;
using UnityEngine;

namespace AutoMachineRebuilt.Integration.Multiplayer
{
    /// <summary>
    /// Network packet transmitted across ONI Together multiplayer sessions
    /// when any player toggles or modifies building automation customization.
    /// </summary>
    public class BuildingAutomationSyncPacket : IPacket
    {
        public int Cell { get; set; }
        public string PrefabId { get; set; }
        public byte ActionType { get; set; } // 0: SetOverride, 1: CancelChore, 2: ResetToGlobalDefault
        public int OverrideState { get; set; } // 0: InheritGlobal, 1: Automated, 2: Manual
        public ulong SenderId { get; set; }

        public BuildingAutomationSyncPacket()
        {
        }

        public BuildingAutomationSyncPacket(int cell, string prefabId, byte actionType, int overrideState, ulong senderId)
        {
            Cell = cell;
            PrefabId = prefabId ?? string.Empty;
            ActionType = actionType;
            OverrideState = overrideState;
            SenderId = senderId;
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Cell);
            writer.Write(PrefabId ?? string.Empty);
            writer.Write(ActionType);
            writer.Write(OverrideState);
            writer.Write(SenderId);
        }

        public void Deserialize(BinaryReader reader)
        {
            Cell = reader.ReadInt32();
            PrefabId = reader.ReadString();
            ActionType = reader.ReadByte();
            OverrideState = reader.ReadInt32();
            SenderId = reader.ReadUInt64();
        }

        public void OnDispatched()
        {
            try
            {
                if (!Grid.IsValidCell(Cell)) return;

                // Attempt to resolve building at cell
                GameObject buildingObj = Grid.Objects[Cell, (int)ObjectLayer.Building];
                if (buildingObj == null)
                {
                    buildingObj = Grid.Objects[Cell, (int)ObjectLayer.FoundationTile];
                }

                if (buildingObj != null)
                {
                    var customizer = buildingObj.GetComponent<AutoBuildingCustomizer>();
                    if (customizer != null)
                    {
                        customizer.ApplyRemoteSync(ActionType, OverrideState, SenderId);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Warn($"[Multiplayer] Error dispatching BuildingAutomationSyncPacket at cell {Cell}: {ex.Message}");
            }
        }
    }
}
