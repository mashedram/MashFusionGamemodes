using Clockhunt.Phase;
using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Entities.Interaction.Components;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Util;
using MelonLoader;

namespace Clockhunt.Entities.Tags;

public class AlarmOnGrabOwnClock : IEntityGrabCallback
{
    public void OnGrab(NetworkEntity entity, Hand hand)
    {
        if (!entity.HasTag<ClockMarker>()) return;
        if (!entity.TryGetTag<EntityOwner>(out var owner)) return;
        if (!NetworkPlayerManager.TryGetPlayer(hand.manager, out var player)) return;
        
        if (owner.OwnerId != player.PlayerID.SmallID) return;

        var context = Clockhunt.Context;
        
        if (!context.PhaseManager.IsPhase<HuntPhase>()) return;
        
        MelonLogger.Msg($"[Clockhunt] Player {player.PlayerID} grabbed their own clock, triggering alarm!");

        // TODO: Make these actual alarms
        context.AlarmAudioPlayer.PlayRandomAt(entity);
    }
}