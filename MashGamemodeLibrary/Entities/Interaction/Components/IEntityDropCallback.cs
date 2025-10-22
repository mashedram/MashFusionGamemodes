using MashGamemodeLibrary.Entities.Tagging.Base;

namespace MashGamemodeLibrary.Entities.Interaction.Components;

public interface IEntityDropCallback : IAbstractEntityTag
{
    double DropCooldown => 0.1f;
    void OnDrop(GrabData grab);
}