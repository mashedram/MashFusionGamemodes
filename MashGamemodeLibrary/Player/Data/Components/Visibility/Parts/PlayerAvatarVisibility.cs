using Il2CppSLZ.Marrow;
using Il2CppSLZ.VRMK;
using LabFusion.Entities;
using MashGamemodeLibrary.Player.Spectating.Data.Components.Visibility;

namespace MashGamemodeLibrary.Player.Data.Components.Visibility.Parts;

public class PlayerAvatarVisibility : IPlayerVisibility
{
    private NetworkPlayer _player;
    private bool _isVisible = true;
    
    public PlayerAvatarVisibility(NetworkPlayer player)
    {
        _player = player;
        _player.AvatarSetter.OnAvatarChanged += OnAvatarChanged;
    }
    
    ~PlayerAvatarVisibility()
    {
        _player.AvatarSetter.OnAvatarChanged -= OnAvatarChanged;
    }
    
    private void OnAvatarChanged()
    {
        SetVisible(_isVisible);
    }

    public void SetVisible(bool isVisible)
    {
        _isVisible = isVisible;

        var avatar = _player.RigRefs?.RigManager?._avatar;
        if (avatar == null)
            return;
        
        avatar.gameObject.SetActive(isVisible);
    }
    
    public void OnRigChanged(RigManager? rigManager)
    {
        // Reload the avatar
        SetVisible(_isVisible);
    }
}