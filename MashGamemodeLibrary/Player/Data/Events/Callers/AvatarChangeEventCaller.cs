using LabFusion.Entities;

namespace MashGamemodeLibrary.Player.Data.Events.Callers;

public class AvatarChangeEventCaller : EventCaller
{
    private void OnAvatarChanged()
    {
        if (NetworkPlayer != null)
            Invoke(new AvatarChangedEvent(NetworkPlayer.RigRefs.RigManager.avatar));
    }
    
    protected override void OnPlayerChanged(NetworkPlayer networkPlayer, NetworkPlayer? oldPlayer)
    {
        if (oldPlayer != null)
            oldPlayer.AvatarSetter.OnAvatarChanged -= OnAvatarChanged;
        
        networkPlayer.AvatarSetter.OnAvatarChanged += OnAvatarChanged;
    }
    
    protected override void OnPlayerRemoved(NetworkPlayer networkPlayer)
    {
        networkPlayer.AvatarSetter.OnAvatarChanged -= OnAvatarChanged;
    }
}