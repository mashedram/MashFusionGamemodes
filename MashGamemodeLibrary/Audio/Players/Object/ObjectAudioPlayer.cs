using LabFusion.Entities;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Modifiers;
using MashGamemodeLibrary.Audio.Players.Basic;
using MashGamemodeLibrary.Audio.Players.Basic.Providers;
using UnityEngine;

namespace MashGamemodeLibrary.Audio.Players.Object;

public class ObjectAudioPlayRequest : INetSerializable
{
    public ushort NetworkEntityId;

    public int? GetSize()
    {
        return sizeof(ushort);
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref NetworkEntityId);
    }
}

public class ObjectAudioPlayer<T> : SyncedAudioPlayer<ObjectAudioPlayRequest> where T : INetSerializable, new()
{
    public ObjectAudioPlayer(string name, ISyncedAudioContainer container, int maxObjectCount,
        AudioModifierFactory factory)
        : base(name, container, new PooledAudioSourceProvider(maxObjectCount, factory))
    {
    }

    protected override bool Modifier(ObjectAudioPlayRequest data, ref AudioSource source)
    {
        if (!new NetworkEntityReference(data.NetworkEntityId).TryGetEntity(out var networkEntity)) return false;
        var entity = networkEntity.GetExtender<IMarrowEntityExtender>().MarrowEntity;

        source.gameObject.transform.SetParent(entity.gameObject.transform);
        source.gameObject.transform.localPosition = Vector3.zero;
        source.gameObject.transform.localRotation = Quaternion.identity;

        return true;
    }

    public void Play(string name, NetworkEntity entity)
    {
        var request = new ObjectAudioPlayRequest
        {
            NetworkEntityId = entity.ID
        };

        Play(name, request);
    }
}