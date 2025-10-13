using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using MashGamemodeLibrary.Entities.Tagging.Base;

namespace MashGamemodeLibrary.Entities.Interaction.Components;

public interface IEntityGrabCallback : IAbstractEntityTag
{
    double GrabCooldown => 0.1f;
    void OnGrab(GrabData grab);
}