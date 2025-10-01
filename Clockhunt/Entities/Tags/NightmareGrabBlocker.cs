using Clockhunt.Nightmare;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Entities.Interaction.Components;

namespace Clockhunt.Entities.Tags;

public class NightmareGrabBlocker : IEntityGrabPredicate
{
    public bool CanGrab(NetworkPlayer grabber, NetworkEntity entity, MarrowEntity marrowEntity)
    {
        var context = Clockhunt.Context;
        return !context.NightmareManager.TryGetNightmare(grabber.PlayerID, out var instance) || instance.CanGrab(grabber, entity, marrowEntity);
    }
}