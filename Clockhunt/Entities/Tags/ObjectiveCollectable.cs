using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using MashGamemodeLibrary.Entities.ECS;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Entities.Queries;

namespace Clockhunt.Entities.Tags;

public class ObjectiveCollectable : IComponent, IComponentReady, IGrabCallback, IDropCallback
{
    public static readonly CachedQuery<ObjectiveCollectable> Query = CachedQueryManager.Create<ObjectiveCollectable>();

    public NetworkEntity? NetworkEntity;
    public MarrowEntity? MarrowEntity;
    public bool IsGrabbed;

    public void OnDropped(GrabData grab)
    {
        IsGrabbed = false;
    }

    public void OnGrabbed(GrabData grab)
    {
        IsGrabbed = true;
    }
    
    public void OnReady(NetworkEntity networkEntity, MarrowEntity marrowEntity)
    {
        NetworkEntity = networkEntity;
        MarrowEntity = marrowEntity;
    }
}