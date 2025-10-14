using MashGamemodeLibrary.Entities.Tagging.Base;

namespace MashGamemodeLibrary.Entities.Interaction.Components;

public interface IEntityGrabPredicate : IAbstractEntityTag
{
    bool CanGrab(GrabData grab);
}