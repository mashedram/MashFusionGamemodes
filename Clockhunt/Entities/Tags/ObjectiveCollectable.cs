using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Entities.Interaction.Components;
using MashGamemodeLibrary.Entities.Tagging.Base;

namespace Clockhunt.Entities.Tags;

public class ObjectiveCollectable : EntityTag, IEntityGrabCallback, IEntityDropCallback
{
    public bool IsGrabbed;

    public void OnDrop(GrabData grab)
    {
        IsGrabbed = false;
    }

    public void OnGrab(GrabData grab)
    {
        IsGrabbed = true;
    }
}