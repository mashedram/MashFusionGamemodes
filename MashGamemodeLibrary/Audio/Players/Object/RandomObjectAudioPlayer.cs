using LabFusion.Entities;
using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Players.Extensions;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Audio.Players.Object;

public class RandomObjectAudioPlayer : ObjectAudioPlayer<DummySerializable>, IRandomObjectAudioPlayer 
{
    public RandomObjectAudioPlayer(string name, ISyncedAudioContainer container, int maxObjectCount, ObjectModifier? modifier = null) : base(name, container, maxObjectCount, modifier)
    {
    }

    public void PlayRandomAt(NetworkEntity entity)
    {
        var name = GetRandomAudioName();
        if (string.IsNullOrEmpty(name)) return;
        Play(name, entity, new DummySerializable());
    }
}