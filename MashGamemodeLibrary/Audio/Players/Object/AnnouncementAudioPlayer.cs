using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Modifiers;
using MashGamemodeLibrary.Audio.Players.Basic;
using MashGamemodeLibrary.Audio.Players.Basic.Providers;
using MashGamemodeLibrary.Util;
using UnityEngine;

namespace MashGamemodeLibrary.Audio.Players.Object;

public class AnnouncementAudioPlayer : SyncedAudioPlayer<DummySerializable>
{

    public AnnouncementAudioPlayer(string name, ISyncedAudioContainer container, AudioModifierFactory? audioModifierFactory = null)
        : base(name, container, new SingleAudioSourceProvider(audioModifierFactory))
    {

    }

    protected override bool Modifier(DummySerializable data, AudioSource source)
    {
        source.spatialBlend = 0f;
        return true;
    }

    public void PlayRandom()
    {
        var name = GetRandomAudioName();
        if (string.IsNullOrEmpty(name)) return;

        Play(name, new DummySerializable());
    }
}