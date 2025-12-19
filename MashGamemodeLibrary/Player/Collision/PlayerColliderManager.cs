using LabFusion.Entities;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;

namespace MashGamemodeLibrary.Player.Collision;

public static class PlayerColliderManager
{
    private static readonly HashSet<byte> IgnoredCollisionPlayers = new();
    private static readonly Dictionary<byte, PlayerColliderCache> PlayerColliders = new();
    private static PlayerColliderCache? _localCache;

    private static readonly RemoteEvent<IgnorePropPacket> IgnorePropEvent = new(packet =>
    {
        if (!IgnoredCollisionPlayers.Contains(packet.SenderPlayerID))
            return;

        if (!PlayerColliders.TryGetValue(packet.SenderPlayerID, out var cache))
            return;

        cache.StopPropColliding(packet.Reference);
    }, CommonNetworkRoutes.AllToAll);

    public static void GenerateColliderCache(NetworkPlayer player)
    {
        if (!player.HasRig) return;

        var rig = player.RigRefs.RigManager.physicsRig;
        if (PlayerColliders.TryGetValue(player.PlayerID, out var cache))
        {
            cache.SetRig(rig);
        }
        else
        {
            var newCache = new PlayerColliderCache(rig);
            if (player.PlayerID.IsMe)
            {
                _localCache = newCache;
            }
            PlayerColliders[player.PlayerID] = newCache;
        }
    }

    public static void SetColliders(NetworkPlayer player, bool state)
    {
        IgnoredCollisionPlayers.Add(player.PlayerID);

        if (!player.HasRig)
            return;

        if (!PlayerColliders.TryGetValue(player.PlayerID, out var cache))
            return;

        cache.SetIgnoreRaycast(player.PlayerID, state);

        if (state)
        {
            cache.ClearPropColliders();
        }

        foreach (var otherCache in PlayerColliders.Values.Where(otherCollider => otherCollider != cache))
        {
            if (state)
                cache.StartColliding(otherCache);
            else
                cache.StopColliding(otherCache);
        }
    }

    public static void RefreshPlayer(NetworkPlayer player)
    {
        GenerateColliderCache(player);
    }

    public static void OnGrab(GrabData grab)
    {
        if (!grab.IsHoldingItem(out var item)) return;
        if (item.GameObject == null) return;

        var player = grab.NetworkPlayer;
        if (player == null)
            return;
        if (!PlayerColliders.TryGetValue(player.PlayerID, out var cache))
            return;

        cache.AddItem(item.GameObject);
    }

    public static void OnDrop(GrabData grab)
    {
        if (!grab.IsHoldingItem(out var item)) return;
        if (item.GameObject == null) return;

        var player = grab.NetworkPlayer;
        if (player == null) return;
        if (!PlayerColliders.TryGetValue(player.PlayerID, out var cache)) return;

        cache.RemoveItem(item.GameObject);
    }

    public static void Clear()
    {
        IgnoredCollisionPlayers.Clear();
        PlayerColliders.Clear();

        _localCache?.ClearPropColliders();
        _localCache = null;
    }

    public static void StartIgnoring(NetworkEntity networkEntity)
    {
        IgnorePropEvent.Call(new IgnorePropPacket
        {
            Reference = new NetworkEntityReference(networkEntity.ID)
        });
    }
}