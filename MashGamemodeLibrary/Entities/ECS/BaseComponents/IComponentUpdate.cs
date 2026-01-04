using MashGamemodeLibrary.Entities.ECS.Declerations;

namespace MashGamemodeLibrary.Entities.ECS.BaseComponents;

public interface IComponentUpdate : IBehaviour, IComponent
{
    void Update(float delta);
}