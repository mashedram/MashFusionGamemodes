using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Entities.Interaction;

namespace MashGamemodeLibrary.Entities.ECS.BaseComponents;

public interface IDropCallback : IBehaviour
{
    void OnDropped(GrabData grab);
}