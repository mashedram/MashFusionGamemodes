using Il2CppSLZ.Marrow.Interaction;
using MashGamemodeLibrary.Player.Actions;

namespace MashGamemodeLibrary.Entities.Tagging.Player.Base;

public interface IPlayerInputTag
{
    void OnInput(PlayerInputType type, bool state, Handedness handedness);
}