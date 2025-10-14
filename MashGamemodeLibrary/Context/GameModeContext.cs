using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.SDK.Gamemodes;

namespace MashGamemodeLibrary.Context;

public abstract class GameModeContext
{
    private bool _isReady;
    private bool _isStarted;
    public bool IsReady => _isReady;
    public bool IsStarted => _isStarted;
    
    private NetworkPlayer? _localPlayer;
    private NetworkPlayer? _hostPlayer;
    public NetworkPlayer LocalPlayer => 
        _localPlayer ?? throw new InvalidOperationException("LocalPlayer is not set. Make sure to call OnStart.");
    public NetworkPlayer HostPlayer => 
        _hostPlayer ?? throw new InvalidOperationException("Failed to get host NetworkPlayer.");

    protected virtual void OnUpdate(float delta) {}

    internal void Update(float delta)
    {
        if (!_isStarted) 
            return;
        
        OnUpdate(delta);
    }
    
    internal void OnReady()
    {
        _isReady = true;
        _localPlayer = LabFusion.Player.LocalPlayer.GetNetworkPlayer();
        if (_localPlayer == null)
            throw new InvalidOperationException("Failed to get local NetworkPlayer.");

        _hostPlayer = NetworkPlayer.Players.FirstOrDefault(e => e.PlayerID.IsHost);
        if (_hostPlayer == null)
            throw new InvalidOperationException("Failed to get host NetworkPlayer.");
    }

    internal void OnStart()
    {
        _isStarted = true;
    }

    internal void OnStop()
    {
        _isStarted = false;
    }

    internal void OnUnready()
    {
        _isReady = false;
    }
}