using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Modifiers;
using MashGamemodeLibrary.Audio.Players.Basic;
using MashGamemodeLibrary.Audio.Players.Basic.Providers;
using MashGamemodeLibrary.Audio.Players.Extensions;
using UnityEngine;

namespace MashGamemodeLibrary.Audio.Players.Object;

public class PositionalAudioPlayRequest : INetSerializable
{
    public Vector3 Position;

    public int? GetSize()
    {
        return sizeof(float) * 3;
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Position);
    }
}

public class PositionalAudioPlayer : SyncedAudioPlayer<PositionalAudioPlayRequest>
{
    public PositionalAudioPlayer(string name, ISyncedAudioContainer container, AudioModifierFactory factory) : base(name, container, new SingleAudioSourceProvider(factory))   
    {
    }

    protected override bool Modifier(PositionalAudioPlayRequest data, ref AudioSource source)
    {
        source.gameObject.transform.position = data.Position;

        return true;
    }
    
    public void Play(string name, Vector3 position)
    {
        var request = new PositionalAudioPlayRequest
        {
            Position = position
        };
        
        Play(name, request);
    }
    
    public void PlayRandom(Vector3 position)
    {
        var name = GetRandomAudioName();
        if (string.IsNullOrEmpty(name)) return;
        Play(name, position);
    }
}