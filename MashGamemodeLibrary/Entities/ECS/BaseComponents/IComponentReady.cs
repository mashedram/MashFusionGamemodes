using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using MashGamemodeLibrary.Entities.ECS.Declerations;

namespace MashGamemodeLibrary.Entities.ECS.BaseComponents;

public interface IComponentReady : IBehaviour, IComponent
{
    void OnReady(NetworkEntity networkEntity, MarrowEntity marrowEntity);
}