using Il2CppSLZ.Marrow;
using Il2CppSLZ.VRMK;

namespace MashGamemodeLibrary.Player.Spectating.Data.Components.Visibility;

public interface IPlayerVisibility
{
    void SetVisible(bool isVisible);
    void OnRigChanged(RigManager? rigManager);
}