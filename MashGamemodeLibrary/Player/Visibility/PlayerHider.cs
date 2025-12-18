using BoneLib;
using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Marrow.Extenders;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Utilities;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Player.Visibility;
using MashGamemodeLibrary.Player.Visibility.Tags;
using MashGamemodeLibrary.Util;
using MelonLoader;
using Steamworks.Data;
using Avatar = Il2CppSLZ.VRMK.Avatar;

namespace MashGamemodeLibrary.Vision;

public static class PlayerHider
{
    private static bool _hideSpecials;
    private static int _currentIndex = 0;
    private static readonly List<byte> _updateList = new();
    private static readonly Dictionary<byte, PlayerVisibilityState> _playerStates = new();

    private static readonly EntityTagCache<IPlayerHiddenTag> PlayerHiddenTags = EntityTagManager.RegisterCache<EntityTagCache<IPlayerHiddenTag>>();

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
        if (_playerStates.TryGetValue(smallId, out var state)) return state;

        if (!NetworkPlayerManager.TryGetPlayer(smallId, out var player))
        {
            MelonLogger.Error(
                "Failed to update player visibility, could not get or create state. Is the player connected?");
            return null;
        }

        state = new PlayerVisibilityState(player, _hideSpecials);
        _updateList.Add(smallId);
        _playerStates[smallId] = state;

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
        _updateList.Remove(playerId);
        _playerStates.Remove(playerId);
    }

    public static bool IsHidden(this PlayerID playerID)
    {
        if (!NetworkInfo.HasServer)
            return false;

        return !_playerStates.TryGetValue(playerID, out var state) || state.IsHidden;
    }

    public static void SetHidden(this PlayerID playerID, string key, bool hidden)
    {
        var state = GetOrCreateState(playerID);
        
        state?.SetHidden(key, hidden);
        
        // Set states for remote values
        if (!PlayerHiddenTags.TryGet(playerID.SmallID, out var set))
            return;
        
        foreach (var playerHiddenTag in set)
        {
            playerHiddenTag.InvokeSafely(t => t.SetHiddenState(hidden));
        }
    }

    public static void HideAllSpecials()
    {
        _hideSpecials = true;

        foreach (var state in GetOrCreateAllStates()) state.SetSpecialsHidden(true);
    }

    public static void Reset()
    {
        _hideSpecials = false;

        foreach (var state in _playerStates.Values) state.Reset();
        _playerStates.Clear();
        _updateList.Clear();
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
        if (!NetworkInfo.HasServer)
            return;
        
        if (_updateList.Count <= 0) 
            return;

        const int maxPlayers = byte.MaxValue + 1;
        const int maxUpdatesPerTick = 4;
        const int stepSize = maxPlayers / maxUpdatesPerTick;
        
        // Add one to offset the division to a 1 based index
        var updatesPerTick = stepSize / _updateList.Count + 1;

        for (var i = 0; i < updatesPerTick; i++)
        {
            // Correct index
            _currentIndex %= _updateList.Count;
            
            var playerId = _updateList[_currentIndex];
            var state = _playerStates[playerId];
            
            state.Update();
            _currentIndex++;
        }
    }
}