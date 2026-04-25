using MashGamemodeLibrary.Entities.Behaviour.Cache;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Entities.Interaction.Grabbing;

namespace MashGamemodeLibrary.Entities.ECS.BaseComponents;

public interface IGrabCallback : IBehaviour
{
    void OnGrabbed(GrabRequest grabRequest);
}