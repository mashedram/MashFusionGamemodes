using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Players.Basic.Providers;
using MashGamemodeLibrary.networking;
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

public abstract class SyncedAudioPlayer<T> : AudioPlayer where T : INetSerializable, new()
{
    private readonly RemoteEvent<PlayRequestPacket<T>> _playRequestEvent;
    
    public SyncedAudioPlayer(string name, ISyncedAudioContainer container, IAudioSourceProvider provider) : base(container, provider)
    {
        _playRequestEvent = new RemoteEvent<PlayRequestPacket<T>>($"{name}_PlayRequest", OnPlayRequest, true);
    }
    
    protected abstract bool Modifier(T data, ref AudioSource source);
    
    private void OnPlayRequest(PlayRequestPacket<T> packet)
    {
        if (!packet.ShouldPlay)
        {
            SourceProvider.StopAll();
            return;
        }
        
        var container = (ISyncedAudioContainer)Container;
        
        container.RequestClip(packet.AudioHash, clip =>
        {
            if (!clip)
                return;
            
            var source = SourceProvider.GetAudioSource();
            if (!Modifier(packet.ExtraData, ref source)) return;
            
            source.clip = clip;
            source.Play();
        });
    }

    public void Play(string name, T data)
    {
        var container = (ISyncedAudioContainer)Container;
        var hash = container.GetAudioHash(name);

        if (hash == null)
        {
            MelonLogger.Error($"Audio with name {name} not found in container of player: {GetType().FullName}");
            return;
        }
        
        _playRequestEvent.Call(new PlayRequestPacket<T>()
        {
            AudioHash = hash.Value,
            ExtraData = data
        });
    }

    public new void StopAll()
    {
        _playRequestEvent.Call(new PlayRequestPacket<T>
        {
            AudioHash = 0,
            ExtraData = new T()
        });
    }
}