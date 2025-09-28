using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using MashGamemodeLibrary.Entities.Tagging.Tags;

namespace MashGamemodeLibrary.Entities.Interaction.Components;

public interface IEntityDropCallback : IEntityTag
{
    double DropCooldown => 0.1f;
    void OnDrop(NetworkEntity networkEntity, Hand hand, MarrowEntity entity);
}