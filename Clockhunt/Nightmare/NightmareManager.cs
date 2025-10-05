using System.Reflection;
using Clockhunt.Game;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.networking;
using MashGamemodeLibrary.networking.Variable;
using MashGamemodeLibrary.networking.Variable.Impl;
using MashGamemodeLibrary.Player;
using MelonLoader;

namespace Clockhunt.Nightmare;

internal class AnnounceNightmarePacket : INetSerializable
{
    public byte PlayerID;
    public ulong NightmareID;
    
    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref PlayerID);
        serializer.SerializeValue(ref NightmareID);
    }
}

public static class NightmareManager
{
    private static readonly Dictionary<ulong, NightmareDescriptor> NightmareDescriptors = new();
    
    public static IReadOnlyDictionary<ulong, NightmareDescriptor> Descriptors => NightmareDescriptors;
    
    private static readonly IDToHashSyncedDictionary PlayerNightmareIds = new("NightmareManager_PlayerNightmareIds");
    private static readonly Dictionary<byte, NightmareInstance> NightmareInstances = new();

    public static IReadOnlyCollection<NightmareInstance> Nightmares => NightmareInstances.Values;

    static NightmareManager()
    {
        PlayerNightmareIds.OnValueChanged += OnPlayerNightmareChange;
        PlayerNightmareIds.OnValueRemoved += OnPlayerNightmareRemove;
    }
    
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

    private static void SetNightmare(byte playerId, ulong nightmareId)
    {
        PlayerNightmareIds[playerId] = nightmareId;
    }
    
    private static void RemoveNightmare(byte playerId)
    {
        PlayerNightmareIds.Remove(playerId);
    }
    
    // TODO: Make this not break when there is more than 1 nightmare
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
            PlayerNightmareIds[player.PlayerID.SmallID] = descriptor.ID;
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

    private static void OnPlayerNightmareChange(byte playerId, ulong nightmareId)
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

        if (player.PlayerID.IsMe)
        {
            Notifier.Send(new Notification
            {
                Title = descriptor.Name,
                Message = descriptor.HunterDescription,
                PopupLength = 5f,
                SaveToMenu = false,
                ShowPopup = true,
                Type = NotificationType.INFORMATION
            });
        }
        else
        {
            Notifier.Send(new Notification
            {
                Title = descriptor.Name,
                Message = descriptor.SurvivorDescription,
                PopupLength = 5f,
                SaveToMenu = false,
                ShowPopup = true,
                Type = NotificationType.INFORMATION
            });
        }
    }
    
    private static void OnPlayerNightmareRemove(byte playerId, ulong nightmareID)
    {
        if (!NightmareInstances.TryGetValue(playerId, out var instance))
        {
            MelonLogger.Error($"Failed to remove nightmare for player ID {playerId} - they are not a nightmare");
            return;
        }
        
        instance.Remove();
        NightmareInstances.Remove(playerId);
    }
    
    public static void ClearNightmares()
    {
        if (!NetworkInfo.IsHost)
        {
            MelonLogger.Error("Only the host can clear nightmares");
            return;
        }

        PlayerNightmareIds.Clear();
    }
}