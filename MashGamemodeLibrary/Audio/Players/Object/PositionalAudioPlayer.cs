using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Audio.Containers;
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
    public delegate bool ObjectModifier(T data, ref AudioSource source);

    private readonly ObjectModifier? _modifier;
    
    public PositionalAudioPlayer(string name, ISyncedAudioContainer container, ObjectModifier? modifier = null) : base(name, container, new SingleAudioSourceProvider())   
    {
        _modifier = modifier;
    }

    protected override bool Modifier(PositionalAudioPlayRequest<T> data, ref AudioSource source)
    {
        source.gameObject.transform.position = data.Position;
        
        return _modifier == null || _modifier(data.Data, ref source);
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