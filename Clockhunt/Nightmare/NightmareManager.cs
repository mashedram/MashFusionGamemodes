using System.Diagnostics.CodeAnalysis;
using Clockhunt.Game;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Loadout;
using MashGamemodeLibrary.networking.Variable.Impl;
using MashGamemodeLibrary.Phase;
using MelonLoader;

namespace Clockhunt.Nightmare;

internal class AnnounceNightmarePacket : INetSerializable
{
    public ulong NightmareID;
    public byte PlayerID;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref PlayerID);
        serializer.SerializeValue(ref NightmareID);
    }
}

public static class NightmareManager
{
    private static readonly Dictionary<ulong, NightmareDescriptor> NightmareDescriptors = new();

    private static readonly IDToHashSyncedDictionary PlayerNightmareIds = new("NightmareManager_PlayerNightmareIds");
    private static readonly Dictionary<byte, NightmareInstance> NightmareInstances = new();

    private static readonly WeightedPlayerSelector PlayerSelector = new();

    static NightmareManager()
    {
        PlayerNightmareIds.OnValueChanged += OnPlayerNightmareChange;
        PlayerNightmareIds.OnValueRemoved += OnPlayerNightmareRemove;
    }

    public static IReadOnlyDictionary<ulong, NightmareDescriptor> Descriptors => NightmareDescriptors;

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
        var method = typeof(NightmareManager).GetMethod(nameof(Register)) ??
                     throw new Exception("Failed to find RegisterTag method");
        assembly.GetTypes()
            .Where(t => typeof(NightmareDescriptor).IsAssignableFrom(t) && t is { IsClass: true, IsAbstract: false })
            .ForEach(t => { method.MakeGenericMethod(t).Invoke(null, null); });
    }

    private static Handedness? GetInput(NetworkPlayer player)
    {
        if (player.RigRefs.LeftHand._controller._menuTap) return Handedness.LEFT;
        if (player.RigRefs.RightHand._controller._menuTap) return Handedness.RIGHT;
        return null;
    }

    private static void UpdateAbility(NightmareInstance instance)
    {
        if (!instance.IsAbilityReady())
            return;
        var keyHand = GetInput(instance.Owner);
        if (keyHand == null)
            return;
        instance.ResetAbilityTimer();
        instance.OnAbilityKeyTapped(keyHand.Value);
    }

    public static void SetRandomNightmare()
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
            var player = PlayerSelector.GetRandomPlayerID();
            PlayerNightmareIds[player] = descriptor.ID;
            return;
        }

        MelonLogger.Error("Failed to select a random nightmare - this should never happen");
        WinStateManager.ForceWin(GameTeam.Survivors);
    }

    public static bool IsNightmare(PlayerID playerID)
    {
        return NightmareInstances.ContainsKey(playerID.SmallID);
    }

    public static bool TryGetNightmare(PlayerID playerID, [MaybeNullWhen(false)] out NightmareInstance instance)
    {
        return NightmareInstances.TryGetValue(playerID.SmallID, out instance);
    }

    public static void Update(float delta)
    {
        foreach (var nightmareInstance in NightmareInstances.Values) nightmareInstance.Update(delta);

        if (!TryGetNightmare(PlayerIDManager.LocalID, out var instance))
            return;

        UpdateAbility(instance);
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
        var instance = descriptor.CreateInstance(playerId);
        NightmareInstances[playerId] = instance;
        instance.Apply();

        Executor.RunIfHost(() => { Loadout.ClearPlayerLoadout(player.RigRefs); });

        if (player.PlayerID.IsMe)
            Notifier.Send(new Notification
            {
                Title = $"You are the {descriptor.Name}",
                Message = descriptor.HunterDescription,
                PopupLength = 5f,
                SaveToMenu = false,
                ShowPopup = true,
                Type = NotificationType.INFORMATION
            });
        else
            Notifier.Send(new Notification
            {
                Title = $"There is a {descriptor.Name}",
                Message = descriptor.SurvivorDescription,
                PopupLength = 5f,
                SaveToMenu = false,
                ShowPopup = true,
                Type = NotificationType.INFORMATION
            });
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

    public static void OnAction(PlayerID playerId, PhaseAction action, Handedness handedness)
    {
        foreach (var instance in NightmareInstances.Values) instance.OnPlayerAction(playerId, action, handedness);
    }
}