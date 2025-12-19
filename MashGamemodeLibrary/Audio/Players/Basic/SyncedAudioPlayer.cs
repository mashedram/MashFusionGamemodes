using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Players.Basic.Providers;
using MashGamemodeLibrary.Audio.Players.Extensions;
using MashGamemodeLibrary.Context.Control;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MelonLoader;
using UnityEngine;

namespace MashGamemodeLibrary.Audio.Players.Basic;

public class PlayRequestPacket<T> : INetSerializable where T : INetSerializable, new()
{
    public ulong AudioHash;
    public T ExtraData = default!;

    public bool ShouldPlay => AudioHash != 0;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref AudioHash);
        serializer.SerializeValue(ref ExtraData);
    }
}

public abstract class SyncedAudioPlayer<TPacket> : AudioPlayer, ISyncedAudioPlayer, IStoppable where TPacket : INetSerializable, new()
{
    private readonly ISyncedAudioContainer _container;
    private readonly RemoteEvent<PlayRequestPacket<TPacket>> _playRequestEvent;

    public SyncedAudioPlayer(string name, ISyncedAudioContainer container, AudioSourceProvider provider) : base(
        container, provider)
    {
        Name = name;

        _container = container;
        _playRequestEvent = new RemoteEvent<PlayRequestPacket<TPacket>>($"{name}_PlayRequest", OnPlayRequest,
            CommonNetworkRoutes.HostToAll);
    }

    public new void Stop()
    {
        Executor.RunIfHost(() =>
        {
            _playRequestEvent.Call(new PlayRequestPacket<TPacket>
            {
                AudioHash = 0,
                ExtraData = new TPacket()
            });
        });
    }
    public string Name { get; }

    protected abstract bool Modifier(TPacket data, ref AudioSource source);

    private void OnPlayRequest(PlayRequestPacket<TPacket> packet)
    {
        if (!packet.ShouldPlay)
        {
            SourceProvider.StopAll();
            return;
        }

        _container.RequestClip(packet.AudioHash, clip =>
        {
            if (!clip)
                return;

            var source = SourceProvider.GetAudioSource();
            if (!Modifier(packet.ExtraData, ref source.SourceRef))
                return;

            source.Play(clip!);
        });
    }

    public void Play(string name, TPacket data)
    {
        var hash = _container.GetAudioHash(name);

        if (hash == null)
        {
            MelonLogger.Error($"Audio with name {name} not found in container of player: {GetType().FullName}");
            return;
        }

        _playRequestEvent.Call(new PlayRequestPacket<TPacket>
        {
            AudioHash = hash.Value,
            ExtraData = data
        });
    }
}