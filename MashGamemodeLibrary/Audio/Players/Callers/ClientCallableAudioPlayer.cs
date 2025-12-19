using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Audio.Players.Extensions;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;

namespace MashGamemodeLibrary.Audio.Players.Callers;

public interface IParameterPacket<TParameter> : INetSerializable
{
    TParameter Value { get; init; }
}

public class ClientCallableAudioPlayer<TParameter, TPacket>
    where TPacket : class, IParameterPacket<TParameter>, new()
{
    private readonly RemoteEvent<TPacket> _clientRequestEvent;
    private readonly IParameterDriven<TParameter> _player;

    public ClientCallableAudioPlayer(IParameterDriven<TParameter> player)
    {
        _player = player;
        _clientRequestEvent = new RemoteEvent<TPacket>($"{player.Name}_clientPlayRequest", OnClientPlayRequest, CommonNetworkRoutes.AllToHost);
    }

    public void PlayRandom(TParameter parameter)
    {
        var packet = new TPacket
        {
            Value = parameter
        };

        _clientRequestEvent.Call(packet);
    }

    public void Update(float delta)
    {
        _player.Update(delta);
    }

    public void Stop()
    {
        _player.Stop();
    }

    public string GetRandomAudioName()
    {
        return _player.GetRandomAudioName();
    }

    // Events

    private void OnClientPlayRequest(TPacket packet)
    {
        var parameter = packet.Value;
        _player.PlayRandom(parameter);
    }
}