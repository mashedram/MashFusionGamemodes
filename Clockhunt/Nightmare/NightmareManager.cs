using System.Diagnostics.CodeAnalysis;
using Clockhunt.Game;
using Clockhunt.Game.Teams;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Data.Random;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Loadout;
using MashGamemodeLibrary.networking.Variable;
using MashGamemodeLibrary.networking.Variable.Encoder.Impl;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Registry.Typed;
using MelonLoader;

namespace Clockhunt.Nightmare;

public static class NightmareManager
{
    private static readonly SingletonTypedRegistry<NightmareDescriptor> NightmareRegistry = new();

    private static readonly CircularRandomProvider<ulong> NightmareRandomProvider = new(() => NightmareRegistry
        .Where(v => v.Value.IsEnabled)
        .Select(v => v.Key)
        .ToList()
    );

    private static readonly SyncedDictionary<byte, ulong> PlayerNightmareIds = new("NightmareManager_PlayerNightmareIds", new ByteEncoder(), new ULongEncoder());
    private static readonly Dictionary<byte, NightmareInstance> NightmareInstances = new();

    static NightmareManager()
    {
        
        
        PlayerNightmareIds.OnValueChanged += OnPlayerNightmareChange;
        PlayerNightmareIds.OnValueRemoved += OnPlayerNightmareRemove;
    }

    public static SingletonTypedRegistry<NightmareDescriptor> Registry => NightmareRegistry;

    public static IEnumerable<NightmareInstance> Nightmares => NightmareInstances.Values;

    private static Handedness? GetInput(NetworkPlayer player)
    {
        if (player.RigRefs.LeftHand._controller._menuTap) return Handedness.LEFT;
        if (player.RigRefs.RightHand._controller._menuTap) return Handedness.RIGHT;

        return null;
    }

    private static void UpdateAbility(NightmareInstance instance)
    {
        // TODO: Move to gameactions
        if (!instance.IsAbilityReady())
            return;

        var keyHand = GetInput(instance.Owner);
        if (keyHand == null)
            return;

        instance.ResetAbilityTimer();
        instance.OnAbilityKeyTapped(keyHand.Value);
    }

    public static void SetRandomNightmare(PlayerID playerID)
    {
        if (!NetworkInfo.IsHost)
        {
            MelonLogger.Error("Only the host can set nightmares");
            return;
        }

        var descriptorID = NightmareRandomProvider.GetRandomValue();
        
        // Default value means no id
        if (descriptorID == 0)
            throw new Exception($"Failed to assign nightmare to player {playerID}");
        
        PlayerNightmareIds[playerID] = descriptorID;
    }

    public static void RemoveNightmare(PlayerID playerID)
    {
        if (!NetworkInfo.IsHost)
        {
            MelonLogger.Error("Only the host can remove nightmares");
            return;
        }

        PlayerNightmareIds.Remove(playerID);
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

        if (!NightmareRegistry.TryGet(nightmareId, out var descriptor))
        {
            MelonLogger.Error($"Failed to find a nightmare by: {nightmareId}");
            return;
        }
        
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

    public static void OnAction(PlayerID playerId, PlayerGameActions action, Handedness handedness)
    {
        foreach (var instance in NightmareInstances.Values) instance.OnPlayerAction(playerId, action, handedness);
    }
}