using Il2CppSLZ.Marrow;
using Il2CppSLZ.VRMK;
using LabFusion.Entities;
using MashGamemodeLibrary.Player.Data.Extenders.Visibility;

namespace MashGamemodeLibrary.Player.Data.Components.Visibility.Parts;

public class PlayerAvatarVisibility : IPlayerVisibility
{
    private RigManager? _rigManager;
    private bool _isVisible = true;
    
    private void UpdateAvatarVisibility()
    {
        if (_rigManager == null)
            return;

        var avatar = _rigManager._avatar;
        if (avatar == null)
            return;

        avatar.gameObject.SetActive(_isVisible);
    }

    public void SetVisible(PlayerVisibility visibility)
    {
        _isVisible = visibility.VisibleForLocalPlayer;
        UpdateAvatarVisibility();
    }

    public void OnPlayerChanged(NetworkPlayer networkPlayer, RigManager rigManager)
    {
        _rigManager = rigManager;
        UpdateAvatarVisibility();
    }

    public void OnAvatarChanged(Avatar avatar)
    {
        // Reload avatar visibility
        UpdateAvatarVisibility();
    }
}