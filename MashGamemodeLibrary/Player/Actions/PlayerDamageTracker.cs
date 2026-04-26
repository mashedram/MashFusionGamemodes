using System.Runtime.InteropServices;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Combat;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.Utilities;
using MashGamemodeLibrary.Entities.Association.Impl;
using MashGamemodeLibrary.Entities.Behaviour;
using MashGamemodeLibrary.Entities.Behaviour.Cache;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Player.Actions;

public enum PlayerDamageStatistics
{
    Kills,
    Assists,
    Deaths
}

[RequireStaticConstructor]
public static class PlayerDamageTracker
{
    private static readonly IAssociatedBehaviourCache<NetworkEntityAssociation, IPlayerTakeDamageCallback> DamageCallbacks = 
        BehaviourManager.CreateCache<NetworkEntityAssociation, IPlayerTakeDamageCallback>();
    private static readonly Dictionary<byte, HashSet<byte>> DamageMarks = new();

    static PlayerDamageTracker()
    {
        MultiplayerHooking.OnPlayerAction += OnAction;

        PlayerStatisticsTracker.Register(PlayerDamageStatistics.Kills, v => v * 10);
        PlayerStatisticsTracker.Register(PlayerDamageStatistics.Assists, v => v * 5);
        PlayerStatisticsTracker.Register(PlayerDamageStatistics.Deaths, v => v * -5);

        LocalHealth.OnAttackedByPlayer += OnDamagedSelf;
    }
    private static void OnDamagedSelf(Attack attack, PlayerDamageReceiver.BodyPart bodyPart, PlayerID damager)
    {
        DamageCallbacks.ForEach(PlayerIDManager.LocalSmallID, p => p.OnDamageTaken(damager));
    }

    public static void Reset()
    {
        DamageMarks.Clear();
    }

    private static HashSet<byte> GetInner(byte attackedID)
    {
        ref var innerDict = ref CollectionsMarshal.GetValueRefOrAddDefault(DamageMarks, attackedID, out var exists);
        if (!exists)
        {
            innerDict = new HashSet<byte>();
        }

        return innerDict!;
    }

    private static void OnDamage(PlayerID damager, PlayerID damaged)
    {
        if (damaged.IsMe)
            return;
        if (damaged.Equals(damager))
            return;

        var map = GetInner(damaged);
        map.Add(damager);
    }

    private static void OnDeath(PlayerID damager, PlayerID damaged)
    {
        if (damaged.IsMe)
            return;

        if (damaged.Equals(damager))
            return;

        var map = GetInner(damaged);

        if (damager.IsMe)
            PlayerStatisticsTracker.Increment(PlayerDamageStatistics.Kills);
        else if (map.Contains(PlayerIDManager.LocalSmallID))
            PlayerStatisticsTracker.Increment(PlayerDamageStatistics.Assists);

        map.Clear();
    }

    private static void OnAction(PlayerID playerId, PlayerActionType type, PlayerID otherPlayer)
    {
        switch (type)
        {
            case PlayerActionType.DEALT_DAMAGE_TO_OTHER_PLAYER:
                OnDamage(playerId, otherPlayer);
                break;
            case PlayerActionType.DYING_BY_OTHER_PLAYER:
                OnDeath(otherPlayer, playerId);
                break;
            case PlayerActionType.DYING:
                if (playerId.IsMe)
                    PlayerStatisticsTracker.Increment(PlayerDamageStatistics.Deaths);
                break;
        }
    }
}