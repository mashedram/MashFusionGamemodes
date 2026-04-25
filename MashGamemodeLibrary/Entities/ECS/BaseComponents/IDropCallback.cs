using MashGamemodeLibrary.Entities.Behaviour.Cache;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Entities.Interaction.Grabbing;

namespace MashGamemodeLibrary.Entities.ECS.BaseComponents;

public interface IDropCallback : IBehaviour
{
    void OnDropped(GrabRequest grabRequest);
}