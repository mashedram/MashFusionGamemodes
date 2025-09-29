using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using MashGamemodeLibrary.Entities.Interaction.Components;

namespace Clockhunt.Entities.Tags;

public class GrabTracker : IEntityGrabCallback, IEntityDropCallback
{
    public bool IsGrabbed { get; private set; }
    
    public void OnGrab(NetworkEntity entity, Hand hand)
    {
        IsGrabbed = true;
    }

    public void OnDrop(NetworkEntity networkEntity, Hand hand, MarrowEntity entity)
    {
        IsGrabbed = false;
    }
}