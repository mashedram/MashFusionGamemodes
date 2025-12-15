using System.Runtime.InteropServices;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.Utilities;

namespace MashGamemodeLibrary.Player.Actions;

public static class PlayerDamageTracker
{
    private static readonly Dictionary<byte, HashSet<byte>> DamageMarks = new();

    static PlayerDamageTracker()
    {
        MultiplayerHooking.OnPlayerAction += OnAction;
        
        PlayerStatisticsTracker.Register(PlayerStatisticsKeys.Kills, v => v * 10);
        PlayerStatisticsTracker.Register(PlayerStatisticsKeys.Assists, v => v * 5);
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
        if (damaged.Equals(damager))
            return;
        
        var map = GetInner(damaged);

        if (damager.IsMe)
            PlayerStatisticsTracker.Increment(PlayerStatisticsKeys.Kills);
        else if (map.Contains(PlayerIDManager.LocalSmallID)) 
            PlayerStatisticsTracker.Increment(PlayerStatisticsKeys.Assists);
            
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
        }
    }
}