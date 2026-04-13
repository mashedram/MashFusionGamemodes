using Il2CppSLZ.Marrow;
using Il2CppSLZ.VRMK;
using LabFusion.Entities;
using MashGamemodeLibrary.Player.Spectating.Data.Components.Visibility;

namespace MashGamemodeLibrary.Player.Data.Components.Visibility.Parts;

public class PlayerAvatarVisibility : IPlayerVisibility
{
    private RigManager? _rigManager;
    private bool _isVisible = true;

    public void SetVisible(bool isVisible)
    {
        _isVisible = isVisible;
        
        if (_rigManager == null)
            return;
        
        var avatar = _rigManager._avatar;
        if (avatar == null)
            return;
        
        avatar.gameObject.SetActive(isVisible);
    }
    
    public void OnPlayerChanged(NetworkPlayer networkPlayer, RigManager rigManager)
    {
        _rigManager = rigManager;
        SetVisible(_isVisible);
    }
    
    public void OnAvatarChanged(Avatar avatar)
    {
        // Reload avatar visibility
        SetVisible(_isVisible);
    }
}