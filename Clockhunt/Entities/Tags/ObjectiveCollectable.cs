using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Marrow;
using LabFusion.Marrow.Pool;
using MashGamemodeLibrary.Entities.Interaction.Components;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Clockhunt.Entities.Tags;

public class ObjectiveCollectable : IEntityGrabCallback, IEntityDropCallback
{
    public bool IsGrabbed;
    
    public void OnGrab(NetworkEntity entity, Hand hand)
    {
        IsGrabbed = true;
    }

    public void OnDrop(NetworkEntity networkEntity, Hand hand, MarrowEntity entity)
    {
        IsGrabbed = false;
    }
}