using System.Reflection;
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
    public int NightmareID;

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

public class NightmareManager
{
    private static readonly Dictionary<int, NightmareDescriptor> _nightmareDescriptors = new();
    private readonly Dictionary<byte, NightmareInstance> _nightmareInstances = new();
    private readonly RemoteEvent<SyncNightmarePacket> _nightmareSyncEvent;

    public static void Register<T>() where T : NightmareDescriptor
    {
        var descriptor = (T)Activator.CreateInstance(typeof(T), true)!;
        var id = descriptor.ID;
        if (!_nightmareDescriptors.TryAdd(id, descriptor))
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
    
    public NightmareManager()
    {
        _nightmareSyncEvent = new RemoteEvent<SyncNightmarePacket>("SyncNightmares", OnNightmareSync, false);
    }
    
    private void SendNightmareSync()
    {
        if (!NetworkInfo.IsHost) return;
        
        var packet = new SyncNightmarePacket();
        foreach (var (playerId, instance) in _nightmareInstances)
        {
            packet.Pairs.Add(new PlayerNightmarePair()
            {
                PlayerID = playerId,
                NightmareID = instance.Descriptor.ID
            });
        }
        
        _nightmareSyncEvent.Call(packet);
    }

    private void SetNightmare(byte playerId, int nightmareId)
    {
        if (!NetworkPlayerManager.TryGetPlayer(playerId, out var player))
        {
            MelonLogger.Error("Tried to set nightmare for invalid player ID");
            return;
        }
        
        var descriptor = _nightmareDescriptors[nightmareId];
        var instance = descriptor.CreateInstance(player);
        _nightmareInstances[playerId] = instance;
        instance.OnApplied();

        if (player.PlayerID.IsMe)
        {
            if (descriptor.Avatar != null)
                LocalAvatar.AvatarOverride = descriptor.Avatar;
            
            PlayerStatManager.SetStats(descriptor.Stats);
        }
        
        if (!NetworkInfo.IsHost) return;
        SendNightmareSync();
    }
    
    private void RemoveNightmare(byte playerId)
    {
        if (!_nightmareInstances.TryGetValue(playerId, out var instance))
        {
            MelonLogger.Error($"Failed to remove nightmare for player ID {playerId} - they are not a nightmare");
            return;
        }
        
        instance.OnRemoved();
        _nightmareInstances.Remove(playerId);

        if (instance.Owner.PlayerID.IsMe)
        {
            LocalAvatar.AvatarOverride = null;
            PlayerStatManager.ResetStats();
        }
        
        if (!NetworkInfo.IsHost) return;
        SendNightmareSync();
    }
    
    public void SetNightmare<T>(NetworkPlayer player) where T : NightmareDescriptor
    {
        if (!NetworkInfo.IsHost)
        {
            MelonLogger.Error("Only the host can set nightmares");
            return;
        }
        
        var type = typeof(T);
        var descriptor = _nightmareDescriptors.Values.FirstOrDefault(d => d.GetType() == type);
        if (descriptor == null)
        {
            MelonLogger.Error($"No nightmare of type {type.Name} is registered");
            return;
        }

        SetNightmare(player.PlayerID.SmallID, descriptor.ID);
    }

    public bool IsNightmare(PlayerID playerID)
    {
        return _nightmareInstances.ContainsKey(playerID.SmallID);
    }

    public bool TryGetNightmare(PlayerID playerID, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out NightmareInstance instance)
    {
        return _nightmareInstances.TryGetValue(playerID.SmallID, out instance);
    }
    
    public void Update(float delta)
    {
        foreach (var (playerId, nightmareInstance) in _nightmareInstances)
        {
            if (!NetworkPlayerManager.TryGetPlayer(playerId, out var player)) continue;
            nightmareInstance.Update(player, delta);
        }
    }
    
    // Remote
    
    private void OnNightmareSync(SyncNightmarePacket obj)
    {
        var goal = obj.Pairs.ToDictionary(e => e.PlayerID, e => e.NightmareID);
        var differences = new Dictionary<byte, bool>();
        
        var allIds = goal.Keys.Union(_nightmareInstances.Keys);

        foreach (var id in allIds)
        {
            var isCurrentlyNightmare = _nightmareInstances.ContainsKey(id);
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
}