using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using MashGamemodeLibrary.Entities.Behaviour.Cache;
using MashGamemodeLibrary.Entities.ECS.Declerations;

namespace MashGamemodeLibrary.Entities.ECS.BaseComponents;

public interface IEntityAttached : IBehaviour, IComponent
{
    void OnReady(NetworkEntity networkEntity, MarrowEntity marrowEntity);
}