using Il2CppSLZ.Marrow;
using Il2CppSLZ.VRMK;
using LabFusion.Entities;

namespace MashGamemodeLibrary.Player.Data.Extenders.Visibility.Parts;

public class PlayerNametagVisibility : IPlayerVisibility
{
    private NetworkPlayer? _player;
    private bool _isVisible = true;

    public void SetVisible(PlayerVisibility visibility)
    {
        _isVisible = visibility.NametagVisible;

        if (_player == null)
            return;
        _player.HeadUI.Visible = _isVisible;
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