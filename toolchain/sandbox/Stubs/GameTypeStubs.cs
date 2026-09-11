// Copyright (c) 2026 AutomaticIndustry Sandbox. Stub types for offline testing.
//
// These stubs reproduce ONLY the signatures and minimal behaviour of the game types
// that the AutomaticIndustry mod references. They are NOT complete implementations
// and must NOT be used as authoritative documentation of the game's internal logic.
//
// Purpose: allow the mod's pure C# logic paths to be exercised in a unit test
// without launching the game or loading Unity assemblies.

using System;
using System.Collections;
using System.Collections.Generic;

// ============================================================================
// UnityEngine stubs
// ============================================================================

namespace UnityEngine
{
    public class Object
    {
        public string name;
        public static implicit operator bool(Object obj) => obj != null;
    }

    public class Component : Object
    {
        public GameObject gameObject;
        public Transform transform;

        public T GetComponent<T>() where T : class
        {
            return gameObject?.GetComponent<T>();
        }

        public T[] GetComponents<T>() where T : class
        {
            return gameObject?.GetComponents<T>() ?? Array.Empty<T>();
        }

        public T GetSMI<T>() where T : class
        {
            // Stub: returns null
            return null;
        }
    }

    public class MonoBehaviour : Component
    {
        protected virtual void OnCleanUp() { }
    }

    public class Transform : Component
    {
        public void SetPosition(UnityEngine.Vector3 pos) { }
    }

    public class GameObject : Object
    {
        private readonly Dictionary<Type, List<object>> _components = new Dictionary<Type, List<object>>();

        public T AddComponent<T>() where T : Component, new()
        {
            var comp = new T();
            comp.gameObject = this;
            var type = typeof(T);
            if (!_components.ContainsKey(type))
                _components[type] = new List<object>();
            _components[type].Add(comp);
            return comp;
        }

        public T GetComponent<T>() where T : class
        {
            var type = typeof(T);
            if (_components.ContainsKey(type) && _components[type].Count > 0)
                return _components[type][0] as T;

            // Check base types and interfaces
            foreach (var kvp in _components)
            {
                if (typeof(T).IsAssignableFrom(kvp.Key) && kvp.Value.Count > 0)
                    return kvp.Value[0] as T;
            }
            return null;
        }

        public T[] GetComponents<T>() where T : class
        {
            var result = new List<T>();
            foreach (var kvp in _components)
            {
                if (typeof(T).IsAssignableFrom(kvp.Key))
                {
                    foreach (var comp in kvp.Value)
                    {
                        if (comp is T t) result.Add(t);
                    }
                }
            }
            return result.ToArray();
        }

        public void SetActive(bool active) { }
    }

    public struct Vector2
    {
        public float x, y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
    }

    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
    }

    public static class Time
    {
        public static float deltaTime = 0.2f;
    }

    public static class Mathf
    {
        public static float Max(float a, float b) => a > b ? a : b;
    }

    public static class Random
    {
        private static readonly System.Random _rng = new System.Random();
        public static float Range(float min, float max) => (float)(_rng.NextDouble() * (max - min) + min);
        public static float value => (float)_rng.NextDouble();
    }

    public struct HashedString
    {
        public string Value;
        public HashedString(string value) { Value = value; }
        public static implicit operator HashedString(string s) => new HashedString(s);
    }
}

// ============================================================================
// Game type stubs (Assembly-CSharp)
// ============================================================================

// KMonoBehaviour - the base class for all game components
public class KMonoBehaviour : UnityEngine.MonoBehaviour
{
    protected override void OnCleanUp() { }
    protected virtual void OnSpawn() { }
}

// Operational status of a building
public class Operational : KMonoBehaviour
{
    public bool IsOperational { get; set; } = true;
    public bool IsActive { get; set; }
    public void SetActive(bool active) { IsActive = active; }
}

// Base class for things that Duplicants can work on
public class Workable : KMonoBehaviour
{
    public float workTime = 10f;
    public WorkerBase GetWorker() => null;
}

// Base class for workers (Duplicants)
public class WorkerBase : KMonoBehaviour { }

// Storage component
public class Storage : KMonoBehaviour
{
    public float capacityKg = 100f;
    private float _storedMass = 0f;
    public float MassStored() => _storedMass;
    public void SetStoredMass(float mass) { _storedMass = mass; }
}

// Element converter
public class ElementConverter : KMonoBehaviour
{
    public OutputElement[] outputElements = Array.Empty<OutputElement>();

    public void SetWorkSpeedMultiplier(float speed) { }

    public struct OutputElement
    {
        public float massGenerationRate;
        public SimHashes elementHash;
    }
}

// Complex fabricator
public class ComplexFabricator : KMonoBehaviour
{
    public bool duplicantOperated = true;
}

public class ComplexFabricatorWorkable : Workable { }

// Manual delivery
public class ManualDeliveryKG : KMonoBehaviour
{
    public HashedString choreTypeIDHash;

    public struct HashedString
    {
        public string Value;
        public HashedString(string value) { Value = value; }
    }
}

// Pickupable
public class Pickupable : KMonoBehaviour { }

// Primary element
public class PrimaryElement : KMonoBehaviour
{
    public float Temperature { get; set; } = 300f;
    public float Mass { get; set; }
    public byte DiseaseIdx { get; set; }
    public int DiseaseCount { get; set; }
    public void AddDisease(byte idx, int count, string source) { }
}

// SimHashes enum stub
public enum SimHashes
{
    Petroleum,
    Methane,
    Water,
    CrudeOil,
    NaturalGas,
    Sand,
    Salt,
}

// Game singleton
public class Game : KMonoBehaviour
{
    public static Game Instance;
    public bool FastWorkersModeActive;
}

// Room tracker
public class RoomTracker : KMonoBehaviour
{
    public Requirement requirement = Requirement.Required;

    public enum Requirement
    {
        Required,
        Recommended,
        CustomRecommended
    }
}

// Chore system stubs
public class Chore
{
    public bool isComplete;
    public bool isNull;
    public object target;

    public bool InProgress() => false;
    public void Cancel(string reason) { }
}

public class FetchChore : Chore { }
public class FetchAreaChore : Chore { }

// Building definitions
public class BuildingDef
{
    public UnityEngine.GameObject BuildingComplete;
    public string PrefabID;
}

public class Assets
{
    public static List<BuildingDef> BuildingDefs = new List<BuildingDef>();
    public static UnityEngine.GameObject GetPrefab(Tag tag) => null;
}

public class GeneratedBuildings
{
    public static void LoadGeneratedBuildings() { }
}

// Tags
public struct Tag
{
    public string Name;
    public Tag(string name) { Name = name; }
}

// Database
public class Db
{
    public static Db Instance;
    public AmountsTable Amounts;
    public ChoreTypesContainer ChoreTypes;

    public static Db Get() => Instance;

    public class AmountsTable
    {
        public AmountDef HitPoints;
    }

    public class AmountDef
    {
        public AmountInstance Lookup(UnityEngine.GameObject go) => null;
    }

    public class ChoreTypesContainer
    {
        public ChoreTypeDef MachineFetch = new ChoreTypeDef();
    }

    public class ChoreTypeDef
    {
        public ManualDeliveryKG.HashedString IdHash;
    }
}

public class AmountInstance
{
    public float value;
    public float GetMax() => 100f;
    public void ApplyDelta(float delta) { value += delta; }
}

// Grid
public class Grid
{
    public enum SceneLayer { Ore }
    public static bool IsValidCell(int cell) => cell >= 0;
    public static int PosToCell(UnityEngine.GameObject go) => 0;
    public static int CellLeft(int cell) => cell - 1;
    public static int CellRight(int cell) => cell + 1;
    public static UnityEngine.Vector3 CellToPosCCC(int cell, SceneLayer layer) => default;
}

// Animation stubs
public class KAnimControllerBase : KMonoBehaviour
{
    public bool HasAnimation(UnityEngine.HashedString anim) => true;
    public void Play(UnityEngine.HashedString anim, KAnim.PlayMode mode = KAnim.PlayMode.Once) { }
    public void Queue(UnityEngine.HashedString anim, KAnim.PlayMode mode = KAnim.PlayMode.Once) { }
}

public class KBatchedAnimController : KAnimControllerBase { }

public class KAnim
{
    public enum PlayMode { Once, Loop, Paused }
}

// State machine stubs
public class StateMachine
{
    public class BaseState
    {
        public BaseState parent;
    }

    public class Instance
    {
        public object[] dataTable;
        public BaseState GetCurrentState() => null;
        public bool IsRunning() => true;
    }
}

public class StateMachineController : KMonoBehaviour, IEnumerable<StateMachine.Instance>
{
    private readonly List<StateMachine.Instance> _instances = new List<StateMachine.Instance>();

    public T GetDef<T>() where T : class => null;

    public IEnumerator<StateMachine.Instance> GetEnumerator() => _instances.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class StateMachineComponent : KMonoBehaviour
{
    public StateMachine.Instance GetSMI() => null;
}

// GameComps
public class GameComps
{
    public static FallerManager Fallers = new FallerManager();

    public class FallerManager
    {
        public bool Has(UnityEngine.GameObject go) => false;
        public void Remove(UnityEngine.GameObject go) { }
        public void Add(UnityEngine.GameObject go, UnityEngine.Vector2 velocity) { }
    }
}

// Valve
public class Valve : KMonoBehaviour { }

// LocString
public class LocString
{
    public static void CreateLocStringKeys(Type type, string prefix) { }
}

// Geyser
public class Geyser : KMonoBehaviour { }

// Research Center stub
public class ResearchCenter : Workable { }

// Effects (Klei.AI)
namespace Klei.AI
{
    public class Effects : KMonoBehaviour
    {
        public void Add(string effectId, bool shouldSave) { }
    }
}

// Ranch station stubs
public class RanchStation
{
    public class Def
    {
        public float WorkTime = 12f;
        public UnityEngine.HashedString RanchedPreAnim;
        public UnityEngine.HashedString RanchedLoopAnim;
        public UnityEngine.HashedString RanchedPstAnim;
        public Action<RanchedStates.Instance, Workable> OnRanchWorkBegins;
        public Action<UnityEngine.GameObject, float, Workable> OnRanchWorkTick;
        public Action<UnityEngine.GameObject, WorkerBase> OnRanchCompleteCb;
    }

    public class Instance : StateMachine.Instance
    {
        public Def def;
        public bool HasRancher;
        public bool IsRancherReady;
        public bool IsCritterAvailableForRanching;
        public RanchedStates.Instance ActiveRanchable;

        public void FindRanchable() { }
        public void MessageRancherReady() { IsRancherReady = true; }
        public void RanchCreature() { }
        public void TriggerRanchStationNoLongerAvailable() { IsRancherReady = false; }
    }
}

public class RanchedStates
{
    public class Instance : StateMachine.Instance
    {
        public KBatchedAnimController AnimController;

        public bool IsNullOrStopped() => this == null || !IsRunning();
    }
}

public class RanchableMonitor
{
    public class Instance : StateMachine.Instance
    {
        public RanchStation.Instance TargetRanchStation;
    }
}

public class RancherChore
{
    public class RancherWorkable : Workable { }
}

public interface IShearable
{
    Tuple<Tag, float> GetItemDroppedOnShear();
    void Shear();
}

public interface IMilkable
{
    void MilkingComplete(Storage storage);
}

public class UnderwaterShearingStaion : KMonoBehaviour
{
    public void HideShearableSymbol() { }
}

// GeoTuner stubs
public class GeoTuner : StateMachine
{
    public static string liquidGeyserTuningSoundPath;
    public static string gasGeyserTuningSoundPath;
    public static string metalGeyserTuningSoundPath;
    public class OperationalState : BaseState
    {
        public GeyserSelectedState geyserSelected;
    }

    public class GeyserSelectedState : BaseState
    {
        public ResearchState researcherInteractionNeeded;
    }

    public class ResearchState : BaseState
    {
        public BaseState available;
        public BaseState blocked;
    }

    public new class Instance : StateMachine.Instance
    {
        public GeoTuner sm;
        public Storage storage;

        public Geyser GetFutureGeyser() => null;
        public Geyser GetAssignedGeyser() => null;
        public void AssignGeyser(Geyser geyser) { }
    }

    public static void OnResearchCompleted(GeoTuner.Instance smi) { }
}

public class GeoTunerConfig
{
    public const string ID = "GeoTuner";
}

// Oil Refinery stubs
public class OilRefinery : KMonoBehaviour
{
    public StatesInstance smi;

    public class States
    {
        public StateMachine.BaseState ready;
    }

    public class StatesInstance : StateMachine.Instance
    {
        public States sm;
    }
}

public class OilRefineryConfig
{
    public const string ID = "OilRefinery";
}

// Oil Well Cap stub
public class OilWellCap : KMonoBehaviour { }

// SpiceGrinder stubs
public class SpiceGrinder : KMonoBehaviour
{
    public class StatesInstance : StateMachine.Instance { }
}

public class SpiceGrinderWorkable : Workable { }

// ISim interfaces
public interface ISim200ms { void Sim200ms(float dt); }
public interface ISim1000ms { void Sim1000ms(float dt); }

// Util
public static class Util
{
    public static UnityEngine.GameObject KInstantiate(UnityEngine.GameObject prefab) => new UnityEngine.GameObject();
}

// Misc attribute markers
[AttributeUsage(AttributeTargets.Field)]
public class MyCmpReqAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Field)]
public class SerializeAttribute : Attribute { }

// ============================================================================
// ONI Together Multiplayer Stubs (v0.7.2)
// ============================================================================

namespace ONI_Together.Networking
{
    public enum PacketSendMode
    {
        Unreliable,
        Immediate,
        UnreliableImmediate,
        NoDelay,
        UnreliableNoDelay,
        Reliable,
        ReliableImmediate
    }
}

namespace ONI_Together.Networking.Packets.Architecture
{
    public interface IPacket
    {
        void Serialize(System.IO.BinaryWriter writer);
        void Deserialize(System.IO.BinaryReader reader);
        void OnDispatched();
    }
}

namespace ONI_Together_API
{
    public static class MP_Mod_Info
    {
        public static bool MultiplayerModPresent { get; set; } = false;
        public static Type MainMpModType => typeof(MP_Mod_Info);
    }

    public static class SessionInfoAPI
    {
        public static bool MultiplayerModPresent => MP_Mod_Info.MultiplayerModPresent;
        public static bool InSession { get; set; } = false;
        public static bool IsHost { get; set; } = false;
        public static bool IsClient { get; set; } = false;
        public static ulong LocalUserID { get; set; } = 123456789UL;
        public static ulong HostUserID { get; set; } = 123456789UL;
    }
}

namespace ONI_Together_API.Networking
{
    public static class PacketRegistryAPI
    {
        public static readonly List<Type> RegisteredPackets = new List<Type>();

        public static void TryRegister(Type packetType, string nameOverride = null)
        {
            if (!RegisteredPackets.Contains(packetType))
                RegisteredPackets.Add(packetType);
        }

        public static void AutoRegisterAll(System.Reflection.Assembly assembly)
        {
            if (assembly == null) return;
            foreach (var t in assembly.GetTypes())
            {
                if (typeof(ONI_Together.Networking.Packets.Architecture.IPacket).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                {
                    TryRegister(t);
                }
            }
        }
    }

    public static class PacketSenderAPI
    {
        public static readonly List<ONI_Together.Networking.Packets.Architecture.IPacket> SentPackets = new List<ONI_Together.Networking.Packets.Architecture.IPacket>();

        public static void SendToAllOtherPeers(ONI_Together.Networking.Packets.Architecture.IPacket packet)
        {
            if (packet != null)
                SentPackets.Add(packet);
        }
    }
}

namespace AutoMachineRebuilt.Integration.Multiplayer
{
    public class BuildingAutomationSyncPacket : ONI_Together.Networking.Packets.Architecture.IPacket
    {
        public int Cell { get; set; }
        public string PrefabId { get; set; }
        public byte ActionType { get; set; }
        public int OverrideState { get; set; }
        public ulong SenderId { get; set; }

        public BuildingAutomationSyncPacket() { }

        public BuildingAutomationSyncPacket(int cell, string prefabId, byte actionType, int overrideState, ulong senderId)
        {
            Cell = cell;
            PrefabId = prefabId ?? string.Empty;
            ActionType = actionType;
            OverrideState = overrideState;
            SenderId = senderId;
        }

        public void Serialize(System.IO.BinaryWriter writer)
        {
            writer.Write(Cell);
            writer.Write(PrefabId ?? string.Empty);
            writer.Write(ActionType);
            writer.Write(OverrideState);
            writer.Write(SenderId);
        }

        public void Deserialize(System.IO.BinaryReader reader)
        {
            Cell = reader.ReadInt32();
            PrefabId = reader.ReadString();
            ActionType = reader.ReadByte();
            OverrideState = reader.ReadInt32();
            SenderId = reader.ReadUInt64();
        }

        public void OnDispatched()
        {
        }
    }

    public static class MultiplayerManager
    {
        public static bool IsMultiplayerAvailable => ONI_Together_API.MP_Mod_Info.MultiplayerModPresent;
        public static bool IsInSession => ONI_Together_API.SessionInfoAPI.InSession;
        public static bool IsHost => ONI_Together_API.SessionInfoAPI.IsHost;
        public static bool IsClient => ONI_Together_API.SessionInfoAPI.IsClient;
        public static ulong LocalUserId => ONI_Together_API.SessionInfoAPI.LocalUserID;
        public static ulong HostUserId => ONI_Together_API.SessionInfoAPI.HostUserID;

        public static void SendBuildingSync(int cell, string prefabId, byte actionType, int overrideState)
        {
            if (!IsInSession) return;
            var packet = new BuildingAutomationSyncPacket(cell, prefabId, actionType, overrideState, LocalUserId);
            ONI_Together_API.Networking.PacketSenderAPI.SendToAllOtherPeers(packet);
        }
    }
}

namespace ONIModFramework.API.Multiplayer
{
    public static class MultiplayerBridge
    {
        public static bool IsMultiplayerAvailable => ONI_Together_API.MP_Mod_Info.MultiplayerModPresent;
        public static bool IsInSession => ONI_Together_API.SessionInfoAPI.InSession;
        public static bool IsHost => ONI_Together_API.SessionInfoAPI.IsHost;
        public static bool IsClient => ONI_Together_API.SessionInfoAPI.IsClient;
    }
}


