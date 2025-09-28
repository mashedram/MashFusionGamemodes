using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using MashGamemodeLibrary.Entities.Tagging.Tags;

namespace MashGamemodeLibrary.Entities.Interaction.Components;

public interface IEntityGrabPredicate : IEntityTag
{
    bool CanGrab(NetworkPlayer grabber, NetworkEntity entity, MarrowEntity marrowEntity);
}