using Il2CppSLZ.Marrow.Interaction;
using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Player.Actions;

namespace MashGamemodeLibrary.Entities.ECS.BaseComponents;

public interface IPlayerInputCallback : IBehaviour
{
    void OnInput(PlayerInputType type, bool state, Handedness handedness);
}