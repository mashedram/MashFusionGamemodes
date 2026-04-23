using MashGamemodeLibrary.Entities.Behaviour.Cache;
using MashGamemodeLibrary.Entities.Interaction;

namespace MashGamemodeLibrary.Entities.ECS.BaseComponents;

public interface IGrabCallback : IBehaviour
{
    void OnGrabbed(GrabData grab);
}