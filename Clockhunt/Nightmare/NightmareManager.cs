using System.Reflection;
using Clockhunt.Game;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.networking;
using MashGamemodeLibrary.Player;
using MelonLoader;

namespace Clockhunt.Nightmare;

class PlayerNightmarePair : INetSerializable
{
    public byte PlayerID;
    public ulong NightmareID;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref PlayerID);
        serializer.SerializeValue(ref NightmareID);
    }
}

class SyncNightmarePacket : INetSerializable
{
    public List<PlayerNightmarePair> Pairs = new();
    
    public void Serialize(INetSerializer serializer)
    {
        var length = Pairs.Count;
        serializer.SerializeValue(ref length);

        if (serializer.IsReader)
        {
            for (var i = 0; i < length; i++)
            {
                var pair = new PlayerNightmarePair();
                pair.Serialize(serializer);
                Pairs.Add(pair);
            }
        }
        else
        {
            for (var i = 0; i < length; i++)
            {
                Pairs[i].Serialize(serializer);
            }
        }
    }
}

public static class NightmareManager
{
    private static readonly Dictionary<ulong, NightmareDescriptor> NightmareDescriptors = new();
    
    public static IReadOnlyDictionary<ulong, NightmareDescriptor> Descriptors => NightmareDescriptors;
    
    private static readonly Dictionary<byte, NightmareInstance> NightmareInstances = new();
    private static readonly RemoteEvent<SyncNightmarePacket> NightmareSyncEvent = new("SyncNightmares", OnNightmareSync, false);

    public static IReadOnlyCollection<NightmareInstance> Nightmares => NightmareInstances.Values;
    
    public static void Register<T>() where T : NightmareDescriptor
    {
        var descriptor = (T)Activator.CreateInstance(typeof(T), true)!;
        var id = descriptor.ID;
        if (!NightmareDescriptors.TryAdd(id, descriptor))
            throw new Exception($"Nightmare with ID {id} is already registered");
        
        descriptor.Register();
    }
    
    public static void RegisterAll<T>()
    {
        var assembly = typeof(T).Assembly;
        var method = typeof(NightmareManager).GetMethod(nameof(Register)) ?? throw new Exception("Failed to find RegisterTag method");
        assembly.GetTypes().Where(t => typeof(NightmareDescriptor).IsAssignableFrom(t) && t is { IsClass: true, IsAbstract: false }).ForEach(t =>
        {
            method.MakeGenericMethod(t).Invoke(null, null);
        });
    }
    
    private static void SendNightmareSync()
    {
        if (!NetworkInfo.IsHost) return;
        
        var packet = new SyncNightmarePacket();
        foreach (var (playerId, instance) in NightmareInstances)
        {
            packet.Pairs.Add(new PlayerNightmarePair()
            {
                PlayerID = playerId,
                NightmareID = instance.Descriptor.ID
            });
        }
        
        NightmareSyncEvent.Call(packet);
    }

    private static void SetNightmare(byte playerId, ulong nightmareId)
    {
        if (!NetworkPlayerManager.TryGetPlayer(playerId, out var player))
        {
            MelonLogger.Error("Tried to set nightmare for invalid player ID");
            return;
        }
        
        var descriptor = NightmareDescriptors[nightmareId];
        var instance = descriptor.CreateInstance(player);
        NightmareInstances[playerId] = instance;
        instance.Apply();
        
        if (!NetworkInfo.IsHost) return;
        SendNightmareSync();
    }
    
    private static void RemoveNightmare(byte playerId)
    {
        if (!NightmareInstances.TryGetValue(playerId, out var instance))
        {
            MelonLogger.Error($"Failed to remove nightmare for player ID {playerId} - they are not a nightmare");
            return;
        }
        
        instance.Remove();
        NightmareInstances.Remove(playerId);
        
        if (!NetworkInfo.IsHost) return;
        SendNightmareSync();
    }
    
    public static void SetNightmare<T>(NetworkPlayer player) where T : NightmareDescriptor
    {
        if (!NetworkInfo.IsHost)
        {
            MelonLogger.Error("Only the host can set nightmares");
            return;
        }
        
        var type = typeof(T);
        var descriptor = NightmareDescriptors.Values.FirstOrDefault(d => d.GetType() == type);
        if (descriptor == null)
        {
            MelonLogger.Error($"No nightmare of type {type.Name} is registered");
            return;
        }

        SetNightmare(player.PlayerID.SmallID, descriptor.ID);
    }
    
    public static void SetRandomNightmare(NetworkPlayer player)
    {
        if (!NetworkInfo.IsHost)
        {
            MelonLogger.Error("Only the host can set nightmares");
            return;
        }
        
        var enabledDescriptors = NightmareDescriptors.Values.Where(d => d.IsEnabled).ToList();
        var totalWeight = enabledDescriptors.Sum(d => d.Weight);
        var choice = Random.Shared.Next(0, totalWeight);
        foreach (var descriptor in enabledDescriptors)
        {
            choice -= descriptor.Weight;
            if (choice > 0) continue;
            SetNightmare(player.PlayerID.SmallID, descriptor.ID);
            return;
        }
        
        MelonLogger.Error("Failed to select a random nightmare - this should never happen");
    }

    public static bool IsNightmare(PlayerID playerID)
    {
        return NightmareInstances.ContainsKey(playerID.SmallID);
    }

    public static bool TryGetNightmare(PlayerID playerID, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out NightmareInstance instance)
    {
        return NightmareInstances.TryGetValue(playerID.SmallID, out instance);
    }
    
    public static void Update(float delta)
    {
        foreach (var (playerId, nightmareInstance) in NightmareInstances)
        {
            if (!NetworkPlayerManager.TryGetPlayer(playerId, out var player)) continue;
            nightmareInstance.Update(player, delta);
        }
    }
    
    // Remote
    
    private static void OnNightmareSync(SyncNightmarePacket obj)
    {
        var goal = obj.Pairs.ToDictionary(e => e.PlayerID, e => e.NightmareID);
        var differences = new Dictionary<byte, bool>();
        
        var allIds = goal.Keys.Union(NightmareInstances.Keys);

        foreach (var id in allIds)
        {
            var isCurrentlyNightmare = NightmareInstances.ContainsKey(id);
            var shouldBeNightmare = goal.ContainsKey(id);
            
            if (isCurrentlyNightmare != shouldBeNightmare)
                differences[id] = shouldBeNightmare;
        }

        foreach (var (id, wantedState) in differences)
        {
            if (wantedState)
            {
                var nightmareId = goal[id];
                SetNightmare(id, nightmareId);
            }
            else
            {
                RemoveNightmare(id);
            }
        }
    }

    public static void ClearNightmares()
    {
        if (!NetworkInfo.IsHost)
        {
            MelonLogger.Error("Only the host can clear nightmares");
            return;
        }
        
        NightmareInstances.ForEach(pair => pair.Value.Remove());
        NightmareInstances.Clear();
        
        SendNightmareSync();
    }
}