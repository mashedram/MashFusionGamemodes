using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using MashGamemodeLibrary.Entities.ECS.Declerations;

namespace MashGamemodeLibrary.Entities.ECS.BaseComponents;

public interface IComponentReady : IBehaviour, IComponent
{
    public NetworkEntity NetworkEntity { get; set; }
    public MarrowEntity MarrowEntity { get; set; }
    void OnReady(NetworkEntity networkEntity, MarrowEntity marrowEntity);
}