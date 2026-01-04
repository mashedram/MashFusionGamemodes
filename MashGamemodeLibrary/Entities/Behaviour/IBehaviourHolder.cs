using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;

namespace MashGamemodeLibrary.Entities.ECS.Declerations;

public interface IBehaviourHolder
{
    ushort EntityId { get; }
    NetworkEntity? NetworkEntity { get; }
    MarrowEntity? MarrowEntity { get; }
    bool IsReady { get; }
}