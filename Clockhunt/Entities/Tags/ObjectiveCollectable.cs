using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Marrow;
using LabFusion.Marrow.Pool;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Entities.Interaction.Components;
using MashGamemodeLibrary.Entities.Tagging.Base;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Clockhunt.Entities.Tags;

public class ObjectiveCollectable : IEntityTag, IEntityGrabCallback, IEntityDropCallback
{
    public bool IsGrabbed;
    
    public void OnGrab(GrabData grab)
    {
        IsGrabbed = true;
    }

    public void OnDrop(GrabData grab)
    {
        IsGrabbed = false;
    }
}