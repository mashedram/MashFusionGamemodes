using Clockhunt.Audio;
using Clockhunt.Entities;
using Clockhunt.Entities.Tags;
using Clockhunt.Game;
using Clockhunt.Nightmare;
using Clockhunt.Phase;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Players.Background;
using MashGamemodeLibrary.Audio.Players.Background.Music;
using MashGamemodeLibrary.Audio.Players.Background.Timed;
using MashGamemodeLibrary.Audio.Players.Object;
using MashGamemodeLibrary.Context;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Util;
using UnityEngine;

namespace Clockhunt;

public class ClockhuntContext : GameContext
{
    public readonly GamePhaseManager PhaseManager = new(new GamePhase[] { new HidePhase(), new HuntPhase(), new EscapePhase() });
    public readonly NightmareManager NightmareManager = new();
    
    // Music
    
    public readonly MusicPlayer<ClockhuntContext, ClockhuntMusicContext> MusicPlayer = new(new MusicState<ClockhuntMusicContext>[]
    {
        new ChaseMusicState(),
        new HuntStartPhaseMusicState(),
        new HuntMiddlePhaseMusicState(),
        new HuntEndPhaseMusicState(),
        new HidePhaseMusicState()
    }, ClockhuntMusicContext.GetContext);
    
    // Audio
    public TimedTagPlayer<ClockMarker> ClockAudioPlayer { get; } = new(new RandomObjectAudioPlayer("ClockSound",
        new SyncedAudioContainer(new AudioFileLoader("Pings")), 10, (NetworkEntity entity, MarrowEntity marrowEntity,
            DummySerializable _, ref AudioSource source) =>
        {
            source.volume = 0.4f;
            source.spatialBlend = 1f;
            return true;
        }), 30, 60);

    public RandomObjectAudioPlayer AlarmAudioPlayer { get; } = new ("AlarmSound",
        new SyncedAudioContainer(new AudioFileLoader("Alarm")), 10, (NetworkEntity entity, MarrowEntity marrowEntity,
            DummySerializable _, ref AudioSource source) =>
        {
            source.volume = 0.4f;
            source.spatialBlend = 1f;
            return true;
        });

    protected override void OnUpdate(float delta)
    {
        PhaseManager.Update(delta);
        NightmareManager.Update(delta);
        MusicPlayer.Update();
        ClockAudioPlayer.Update(delta);
        
        ClockManager.Update();
    }
}