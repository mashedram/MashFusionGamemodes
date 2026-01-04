using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.SDK.Gamemodes;
using LabFusion.Senders;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Entities.ECS;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Entities.ECS.Query;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.Player.Spectating;

namespace MashGamemodeLibrary.Entities.Tagging.Player.Common;

internal class RespawnCountPacket : INetSerializable
{
    public int Respawns;

    public int? GetSize()
    {
        return sizeof(int);
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Respawns);
    }
}

public class LimitedRespawnComponent : IComponentRemoved, IComponentPlayerReady, IPlayerActionCallback
{
    public static readonly CachedQuery<LimitedRespawnComponent> Query = EcsManager.CacheQuery<LimitedRespawnComponent>();
    
    public delegate bool PlayerSpectatePredicate(NetworkPlayer player);
    private static readonly RemoteEvent<RespawnCountPacket> RespawnCountChangedEvent = new(OnRespawnsChanged, CommonNetworkRoutes.HostToAll);
    private static readonly Dictionary<Type, PlayerSpectatePredicate> GlobalPredicates = new();

    private NetworkPlayer _owner = null!;
    private bool _canEnterSpectator;
    private bool _isEliminationHandled;

    // Keep, a default constructor is required for networking to function
    // ReSharper disable once UnusedMember.Global
    public LimitedRespawnComponent()
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="respawns"></param>
    public LimitedRespawnComponent(int respawns)
    {
        Respawns = respawns;
    }

    public int Respawns { get; private set; }
    public bool IsEliminated { get; private set; }

    public void OnReady(NetworkPlayer networkPlayer, MarrowEntity marrowEntity)
    {
        _owner = networkPlayer;
    }

    public void OnAction(PlayerActionType action, PlayerID otherPlayer)
    {
        Executor.RunIfHost(() =>
        {
            switch (action)
            {
                case PlayerActionType.DYING:
                {
                    OnDying();
                    break;
                }
                case PlayerActionType.DEATH:
                {
                    OnDeath();
                    break;
                }
            }
        });
    }

    public void OnRemoved(NetworkEntity networkEntity)
    {
        Executor.RunIfHost(() =>
        {
            _owner?.PlayerID?.SetSpectating(false);
        });
    }

    public static void RegisterSpectatePredicate<T>(PlayerSpectatePredicate predicate) where T : Gamemode
    {
        GlobalPredicates[typeof(T)] = predicate;
    }

    public void SetRespawns(int respawns)
    {
        Executor.RunIfHost(() =>
        {
            Respawns = respawns;
        });
    }

    private void OnDying()
    {
        if (IsEliminated)
            return;

        Respawns--;

        if (Respawns >= 0)
            return;

        IsEliminated = true;
        _canEnterSpectator = GamemodeManager.IsGamemodeStarted &&
                             GlobalPredicates.TryGetValue(GamemodeManager.ActiveGamemode.GetType(), out var predicate) &&
                             predicate.Invoke(_owner);
    }

    private void OnDeath()
    {
        if (_isEliminationHandled)
            return;

        if (Respawns >= 0)
        {
            RespawnCountChangedEvent.CallFor(_owner.PlayerID, new RespawnCountPacket
            {
                Respawns = Respawns
            });
            return;
        }

        if (!_canEnterSpectator)
            return;

        // We don't want the spectator message to overlap with the game lost message
        RespawnCountChangedEvent.CallFor(_owner.PlayerID, new RespawnCountPacket
        {
            Respawns = Respawns
        });

        _owner.PlayerID.SetSpectating(true);
        _isEliminationHandled = true;
    }

    private static void OnRespawnsChanged(RespawnCountPacket packet)
    {
        var respawns = packet.Respawns;

        switch (respawns)
        {
            case < 0:
                Notifier.Send(new Notification
                {
                    Title = "Game over",
                    Message = "You are now spectating",
                    PopupLength = 3f,
                    SaveToMenu = false,
                    ShowPopup = true,
                    Type = NotificationType.ERROR
                });
                break;
            case 0:
                Notifier.Send(new Notification
                {
                    Title = "Player Died",
                    Message = "This is your last try.",
                    PopupLength = 3f,
                    SaveToMenu = false,
                    ShowPopup = true,
                    Type = NotificationType.ERROR
                });
                break;
            case > 0:
                Notifier.Send(new Notification
                {
                    Title = "Player Died",
                    Message = $"You have {respawns} respawns remaining.",
                    PopupLength = 3f,
                    SaveToMenu = false,
                    ShowPopup = true,
                    Type = NotificationType.ERROR
                });
                break;
        }
    }
}