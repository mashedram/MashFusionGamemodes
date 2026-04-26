using MashGamemodeLibrary.Entities.Behaviour.Cache;
using MashGamemodeLibrary.Entities.ECS.Declerations;

namespace MashGamemodeLibrary.Entities.ECS.BaseComponents;

public interface IRemoved : IBehaviour, IComponent
{
    void OnRemoved();
}