using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Players.Basic;
using MashGamemodeLibrary.Audio.Players.Basic.Providers;
using MashGamemodeLibrary.Audio.Players.Extensions;
using UnityEngine;

namespace MashGamemodeLibrary.Audio.Players.Object;

public class ObjectAudioPlayRequest<T> : INetSerializable where T : INetSerializable, new()
{
    public ushort NetworkEntityId;
    public T Data = new T();

    public int? GetSize()
    {
        return sizeof(ushort) + Data.GetSize();
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref NetworkEntityId);
        serializer.SerializeValue(ref Data);
    }
}

public class ObjectAudioPlayer<T> : SyncedAudioPlayer<ObjectAudioPlayRequest<T>> where T : INetSerializable, new()
{
    public delegate bool ObjectModifier(NetworkEntity networkEntity, MarrowEntity entity, T data, ref AudioSource source);

    private readonly ObjectModifier? _modifier;
    
    public ObjectAudioPlayer(string name, ISyncedAudioContainer container, int maxObjectCount, ObjectModifier? modifier = null) 
        : base(name, container, new PooledAudioSourceProvider(maxObjectCount))
    {
        _modifier = modifier;
    }

    protected override bool Modifier(ObjectAudioPlayRequest<T> data, ref AudioSource source)
    {
        if (!new NetworkEntityReference(data.NetworkEntityId).TryGetEntity(out var networkEntity)) return false;
        var entity = networkEntity.GetExtender<IMarrowEntityExtender>().MarrowEntity;
        
        source.gameObject.transform.SetParent(entity.gameObject.transform);
        source.gameObject.transform.localPosition = Vector3.zero;
        source.gameObject.transform.localRotation = Quaternion.identity;
        
        return _modifier?.Invoke(networkEntity, entity, data.Data, ref source) ?? true;
    }
    
    public void Play(string name, NetworkEntity entity, T data)
    {
        var request = new ObjectAudioPlayRequest<T>
        {
            NetworkEntityId = entity.ID,
            Data = data
        };
        
        Play(name, request);
    }
}