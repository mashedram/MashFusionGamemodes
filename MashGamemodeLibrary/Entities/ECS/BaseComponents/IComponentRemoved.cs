using LabFusion.Entities;
using MashGamemodeLibrary.Entities.ECS.Declerations;

namespace MashGamemodeLibrary.Entities.ECS.BaseComponents;

public interface IComponentRemoved : IBehaviour, IComponent
{
    void OnRemoved(NetworkEntity networkEntity);
}