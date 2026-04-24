using Il2CppSLZ.Marrow;
using Il2CppSLZ.VRMK;
using LabFusion.Entities;

namespace MashGamemodeLibrary.Player.Data.Extenders.Visibility;

public interface IPlayerVisibility
{
    void SetVisible(PlayerVisibility visibility);
    /// <summary>
    /// Gets called when the player changes
    /// Will also get called if the player's rig changes
    /// </summary>
    void OnPlayerChanged(NetworkPlayer networkPlayer, RigManager rigManager);
    void OnAvatarChanged(Avatar avatar);
}