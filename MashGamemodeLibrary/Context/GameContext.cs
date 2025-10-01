using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.SDK.Gamemodes;

namespace MashGamemodeLibrary.Context;

public abstract class GameContext
{
    private NetworkPlayer? _localPlayer;
    public NetworkPlayer LocalPlayer => _localPlayer ?? throw new InvalidOperationException("LocalPlayer is not set. Make sure to call OnStart.");

    protected virtual void OnUpdate(float delta) {}

    internal void Update(float delta)
    {
        OnUpdate(delta);
    }
    
    internal void OnReady()
    {
        _localPlayer = LabFusion.Player.LocalPlayer.GetNetworkPlayer();
        if (_localPlayer == null)
            throw new InvalidOperationException("Failed to get local NetworkPlayer.");
    }
}