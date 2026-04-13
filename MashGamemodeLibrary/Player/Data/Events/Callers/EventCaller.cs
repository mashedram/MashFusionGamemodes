using LabFusion.Entities;

namespace MashGamemodeLibrary.Player.Data.Events.Callers;

public abstract class EventCaller : IEventCaller
{
    private IEventReceiver? _eventReceiver;
    private NetworkPlayer? _networkPlayer;
    
    protected NetworkPlayer? NetworkPlayer => _networkPlayer;
    
    protected abstract void OnPlayerChanged(NetworkPlayer networkPlayer, NetworkPlayer? oldPlayer);
    protected abstract void OnPlayerRemoved(NetworkPlayer networkPlayer);
    
    protected void Invoke(IPlayerEvent playerEvent)
    {
        _eventReceiver?.ReceiveEvent(playerEvent);
    }
    
    public void OnEnable(IEventReceiver eventReceiver, NetworkPlayer networkPlayer)
    {
        _eventReceiver = eventReceiver;
        
        if (_networkPlayer != null && _networkPlayer.Equals(networkPlayer))
            return;
        
        var oldPlayer = _networkPlayer;
        _networkPlayer = networkPlayer;
        OnPlayerChanged(networkPlayer, oldPlayer);
    }
    
    public void OnDisable()
    {
        if (_networkPlayer == null) return;
        
        OnPlayerRemoved(_networkPlayer);
        _networkPlayer = null;
    }
}