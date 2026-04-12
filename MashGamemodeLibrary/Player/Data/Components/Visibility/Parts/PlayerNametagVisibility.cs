using Il2CppSLZ.Marrow;
using LabFusion.Entities;

namespace MashGamemodeLibrary.Player.Spectating.Data.Components.Visibility.Parts;

public class PlayerNametagVisibility : IPlayerVisibility
{
    private readonly NetworkPlayer _player;
    private bool _isVisible = true;
    
    public PlayerNametagVisibility(NetworkPlayer player)
    {
        _player = player;
    }
    
    public void SetVisible(bool isVisible)
    {
        _isVisible = isVisible;
        _player.HeadUI.Visible = isVisible;
    }
    
    public void OnRigChanged(RigManager? rigManager)
    {
        _player.HeadUI.Visible = _isVisible;
    }
}