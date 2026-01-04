using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Entities.Interaction;

namespace MashGamemodeLibrary.Entities.ECS.BaseComponents;

public interface IGrabPredicate : IBehaviour
{
    bool CanGrab(GrabData grab);
}