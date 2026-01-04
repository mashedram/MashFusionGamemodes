using BoneLib;
using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using LabFusion.Marrow.Extenders;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Utilities;
using MashGamemodeLibrary.Entities.Behaviour;
using MashGamemodeLibrary.Entities.Interaction;
using MelonLoader;
using UnityEngine;
using Avatar = Il2CppSLZ.VRMK.Avatar;

namespace MashGamemodeLibrary.Player.Visibility;

public static class PlayerHider
{
    private static bool _hideSpecials;
    private static int _currentIndex;
    private static readonly List<byte> UpdateList = new();
    private static readonly Dictionary<byte, PlayerVisibilityState> PlayerStates = new();

    public static void Register()
    {
        Hooking.OnSwitchAvatarPostfix += OnAvatarChanged;
        MultiplayerHooking.OnPlayerLeft += OnPlayerLeft;
    }

    private static IEnumerable<PlayerVisibilityState> GetOrCreateAllStates()
    {
        return NetworkPlayer.Players.Select(player => GetOrCreateState(player.PlayerID))
            .OfType<PlayerVisibilityState>();
    }

    private static PlayerVisibilityState? GetOrCreateState(byte smallId)
    {
        if (PlayerStates.TryGetValue(smallId, out var state)) return state;

        if (!NetworkPlayerManager.TryGetPlayer(smallId, out var player))
        {
            MelonLogger.Error(
                "Failed to update player visibility, could not get or create state. Is the player connected?");
            return null;
        }

        state = new PlayerVisibilityState(player, _hideSpecials);
        UpdateList.Add(smallId);
        PlayerStates[smallId] = state;

        return state;
    }

    private static void OnAvatarChanged(Avatar avatar)
    {
        var rigManager = avatar.GetComponentInParent<RigManager>();
        if (!rigManager)
            return;

        if (!NetworkPlayerManager.TryGetPlayer(rigManager, out var player))
            return;

        var state = GetOrCreateState(player.PlayerID);

        state?.PopulateRenderers();
    }

    private static void OnPlayerLeft(PlayerID playerId)
    {
        UpdateList.Remove(playerId);
        PlayerStates.Remove(playerId);
    }

    public static bool IsHidden(this PlayerID playerID)
    {
        if (!NetworkInfo.HasServer)
            return false;

        return !PlayerStates.TryGetValue(playerID, out var state) || state.IsHidden;
    }

    public static void SetHidden(this PlayerID playerID, string key, bool hidden)
    {
        var state = GetOrCreateState(playerID);

        state?.SetHidden(key, hidden);
    }

    public static void HideAllSpecials()
    {
        _hideSpecials = true;

        foreach (var state in GetOrCreateAllStates()) state.SetSpecialsHidden(true);
    }

    public static void Reset()
    {
        _hideSpecials = false;

        foreach (var state in PlayerStates.Values) state.Reset();
        PlayerStates.Clear();
        UpdateList.Clear();
    }

    public static void Refresh(PlayerID player)
    {
        GetOrCreateState(player)?.RefreshRenderers();
    }

    internal static void OnGrab(GrabData hand)
    {
        if (hand.NetworkPlayer == null)
            return;

        GetOrCreateState(hand.NetworkPlayer.PlayerID)?.OnGrab(hand);
    }

    internal static void OnDrop(GrabData hand)
    {
        if (hand.NetworkPlayer == null)
            return;

        GetOrCreateState(hand.NetworkPlayer.PlayerID)?.OnDrop(hand);
    }

    internal static void OnHolster(InventorySlotReceiver receiver)
    {
        if (!InventorySlotReceiverExtender.Cache.TryGet(receiver, out var networkEntity))
            return;

        var id = (byte)networkEntity.ID;
        GetOrCreateState(id)?.OnHolster(receiver);
    }

    internal static void OnUnholster(InventorySlotReceiver receiver)
    {
        if (!InventorySlotReceiverExtender.Cache.TryGet(receiver, out var networkEntity))
            return;

        var id = (byte)networkEntity.ID;
        GetOrCreateState(id)?.OnUnholster(receiver);
    }

    public static void UpdateAmmoHolster(InventoryAmmoReceiver inventoryAmmoReceiver)
    {
        if (inventoryAmmoReceiver._parentRigManager == null)
            return;
        
        if (!NetworkPlayerManager.TryGetPlayer(inventoryAmmoReceiver._parentRigManager, out var player))
            return;

        var id = player.PlayerID.SmallID;
        GetOrCreateState(id)?.OnAmmoChange();
    }

    public static void Update()
    {
        if (!NetworkInfo.HasServer)
            return;

        if (UpdateList.Count <= 0)
            return;

        const int maxPlayers = byte.MaxValue + 1;
        const int maxUpdatesPerTick = 4;
        const int stepSize = maxPlayers / maxUpdatesPerTick;

        // Add one to offset the division to a 1 based index
        var updatesPerTick = stepSize / UpdateList.Count + 1;

        for (var i = 0; i < updatesPerTick; i++)
        {
            // Correct index
            _currentIndex %= UpdateList.Count;

            var playerId = UpdateList[_currentIndex];
            var state = PlayerStates[playerId];

            state.Update();
            _currentIndex++;
        }
    }
}