using LabFusion.Entities;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Modifiers;
using MashGamemodeLibrary.Audio.Players.Basic;
using MashGamemodeLibrary.Audio.Players.Basic.Providers;
using MashGamemodeLibrary.Audio.Players.Extensions;
using MashGamemodeLibrary.Execution;
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

public class ObjectAudioPlayer : SyncedAudioPlayer<ObjectAudioPlayRequest>, IRandomAudioPlayer<NetworkEntity>
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

        var gameObject = source.gameObject;
        gameObject.transform.SetParent(entity.gameObject.transform);
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localRotation = Quaternion.identity;

        return true;
    }

    public void Play(string name, NetworkEntity entity)
    {
        Executor.RunIfHost(() =>
        {
            var request = new ObjectAudioPlayRequest
            {
                NetworkEntityId = entity.ID
            };

            Play(name, request);
        });
    }
    
    public void PlayRandom(NetworkEntity entity)
    {
        var name = GetRandomAudioName();
        if (string.IsNullOrEmpty(name)) return;

        Play(name, entity);
    }
}