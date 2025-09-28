using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using MashGamemodeLibrary.Entities.Tagging.Tags;

namespace MashGamemodeLibrary.Entities.Interaction.Components;

public interface IEntityGrabCallback : IEntityTag
{
    double GrabCooldown => 0.1f;
    void OnGrab(NetworkEntity entity, Hand hand);
}