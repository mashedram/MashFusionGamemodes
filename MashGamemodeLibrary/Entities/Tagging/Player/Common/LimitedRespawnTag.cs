using Il2CppSLZ.Bonelab;
using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.SDK.Gamemodes;
using LabFusion.Senders;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Entities.Tagging.Base;
using MashGamemodeLibrary.Entities.Tagging.Player.Base;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.Player.Spectating;
using UnityEngine.UIElements;

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

public class LimitedRespawnTag : PlayerTag, ITagRemoved, IPlayerActionTag
{
    private static readonly RemoteEvent<RespawnCountPacket> RespawnCountChangedEvent = new(OnRespawnsChanged, CommonNetworkRoutes.HostToAll);
    
    public int Respawns { get; private set; }
    public bool IsEliminated { get; private set; }
    private bool _isEliminationHandled;
    private bool _canEnterSpectator;
    
    public delegate bool PlayerSpectatePredicate(NetworkPlayer player);
    private static readonly Dictionary<Type, PlayerSpectatePredicate> GlobalPredicates = new();

    public static void RegisterSpectatePredicate<T>(PlayerSpectatePredicate predicate) where T : Gamemode
    {
        GlobalPredicates[typeof(T)] = predicate;
    }
    
    // Keep, a default constructor is required for networking to function
    // ReSharper disable once UnusedMember.Global
    public LimitedRespawnTag() {}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="respawns"></param>
    public LimitedRespawnTag(int respawns)
    {
        Respawns = respawns;
    }
    
    public void SetRespawns(int respawns)
    {
        Executor.RunIfHost(() =>
        {
            Respawns = respawns;
        });
    }
    
    public void OnRemoval(ushort entityID)
    {
        Executor.RunIfHost(() =>
        {
            Owner.PlayerID.SetSpectating(false);
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
                             predicate.Invoke(Owner);
    }

    private void OnDeath()
    {
        if (_isEliminationHandled)
            return;

        if (Respawns >= 0)
        {
            RespawnCountChangedEvent.CallFor(Owner.PlayerID, new RespawnCountPacket
            {
                Respawns = Respawns
            });
            return;
        }
        
        if (!_canEnterSpectator)
            return;
        
        // We don't want the spectator message to overlap with the game lost message
        RespawnCountChangedEvent.CallFor(Owner.PlayerID, new RespawnCountPacket
        {
            Respawns = Respawns
        });

        Owner.PlayerID.SetSpectating(true);
        _isEliminationHandled = true;
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