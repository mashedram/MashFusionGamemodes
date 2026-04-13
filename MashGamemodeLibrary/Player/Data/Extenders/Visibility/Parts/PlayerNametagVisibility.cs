using Il2CppSLZ.Marrow;
using Il2CppSLZ.VRMK;
using LabFusion.Entities;
using MashGamemodeLibrary.Player.Spectating.Data.Components.Visibility;

namespace MashGamemodeLibrary.Player.Data.Extenders.Visibility.Parts;

public class PlayerNametagVisibility : IPlayerVisibility
{
    private NetworkPlayer? _player;
    private bool _isVisible = true;
    
    public void SetVisible(bool isVisible)
    {
        _isVisible = isVisible;
        
        if (_player == null)
            return;
        _player.HeadUI.Visible = isVisible;
    }
    
    public void OnPlayerChanged(NetworkPlayer networkPlayer, RigManager rigManager)
    {
        _player = networkPlayer;
        _player.HeadUI.Visible = _isVisible;
    }
       
    public void OnAvatarChanged(Avatar avatar)
    {
        // No-Op
    }
}