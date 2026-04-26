using MashGamemodeLibrary.Entities.Behaviour.Cache;
using MashGamemodeLibrary.Entities.ECS.Declerations;

namespace MashGamemodeLibrary.Entities.ECS.BaseComponents;

public interface IUpdate : IBehaviour, IComponent
{
    void Update(float delta);
}