using MashGamemodeLibrary.Entities.ECS;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Entities.ECS.Query;
using MashGamemodeLibrary.Entities.Interaction;

namespace Clockhunt.Entities.Tags;

public class ObjectiveCollectable : IComponent, IGrabCallback, IDropCallback
{
    public static readonly CachedQuery<ObjectiveCollectable> Query = EcsManager.CacheQuery<ObjectiveCollectable>();
    
    public bool IsGrabbed;

    public void OnDropped(GrabData grab)
    {
        IsGrabbed = false;
    }

    public void OnGrabbed(GrabData grab)
    {
        IsGrabbed = true;
    }
}