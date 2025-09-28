using Clockhunt.Audio;
using Clockhunt.Nightmare;
using Clockhunt.Phase;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Players.Background;
using MashGamemodeLibrary.Audio.Players.Background.Music;
using MashGamemodeLibrary.Audio.Players.Object;
using MashGamemodeLibrary.Context;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Util;
using UnityEngine;

namespace Clockhunt;

public class ClockhuntContext : GameContext
{
    public readonly GamePhaseManager PhaseManager = new(new GamePhase[] { new HidePhase(), new HuntPhase() });
    public readonly NightmareManager NightmareManager = new();
    
    // Music
    
    public readonly MusicPlayer<ClockhuntContext, ClockhuntMusicContext> MusicContext = new(new MusicState<ClockhuntMusicContext>[]
    {
        new HidePhaseMusicState()
    }, ClockhuntMusicContext.GetContext);
    
    // Audio
    public ObjectAudioPlayer<DummySerializable> ClockAudioPlayer { get; } = new("ClockSound",
        new SyncedAudioContainer(new AudioFileLoader("Pings")), 10, (NetworkEntity entity, MarrowEntity marrowEntity,
            DummySerializable _, ref AudioSource source) =>
        {
            source.volume = 0.4f;
            source.spatialBlend = 1f;
            return true;
        });

    public override void OnUpdate(float delta)
    {
        PhaseManager.Update(delta);
        NightmareManager.Update(delta);
        MusicContext.Update();
    }
}