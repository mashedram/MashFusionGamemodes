using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Modifiers;
using MashGamemodeLibrary.Audio.Players.Basic;
using MashGamemodeLibrary.Audio.Players.Basic.Providers;
using MashGamemodeLibrary.Audio.Players.Extensions;
using UnityEngine;

namespace MashGamemodeLibrary.Audio.Players.Object;

public class PositionalAudioPlayRequest<T> : INetSerializable where T : INetSerializable, new()
{
    public Vector3 Position;
    public T Data = new();

    public int? GetSize()
    {
        return sizeof(float) * 3 + Data.GetSize();
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Position);
        serializer.SerializeValue(ref Data);
    }
}

public class PositionalAudioPlayer<T> : SyncedAudioPlayer<PositionalAudioPlayRequest<T>> where T : INetSerializable, new()
{
    public PositionalAudioPlayer(string name, ISyncedAudioContainer container, AudioModifierFactory factory) : base(name, container, new SingleAudioSourceProvider(factory))   
    {
    }

    protected override bool Modifier(PositionalAudioPlayRequest<T> data, ref AudioSource source)
    {
        source.gameObject.transform.position = data.Position;

        return true;
    }
    
    public void Play(string name, Vector3 position, T data)
    {
        var request = new PositionalAudioPlayRequest<T>
        {
            Position = position,
            Data = data
        };
        
        Play(name, request);
    }
}