using BoneLib;
using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Marrow.Extenders;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Utilities;
using MashGamemodeLibrary.Entities.Interaction;
using MelonLoader;
using Avatar = Il2CppSLZ.VRMK.Avatar;

namespace MashGamemodeLibrary.Vision;

public static class PlayerHider
{
    private static bool _hideSpecials;
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

    private static PlayerVisibilityState? GetOrCreateState(byte playerId)
    {
        if (PlayerStates.TryGetValue(playerId, out var state)) return state;

        if (!NetworkPlayerManager.TryGetPlayer(playerId, out var player))
        {
            MelonLogger.Error(
                "Failed to update player visibility, could not get or create state. Is the player connected?");
            return null;
        }

        state = new PlayerVisibilityState(player, _hideSpecials);
        PlayerStates[playerId] = state;

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
        if (!InventoryAmmoReceiverExtender.Cache.TryGet(inventoryAmmoReceiver, out var networkEntity))
            return;

        var id = (byte)networkEntity.ID;
        GetOrCreateState(id)?.OnAmmoChange();
    }

    public static void Update()
    {
        PlayerStates.Values.ForEach(p => p.Update());
    }
}