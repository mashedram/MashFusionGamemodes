using Clockhunt.Audio;
using Clockhunt.Entities;
using Clockhunt.Entities.Tags;
using Clockhunt.Game;
using Clockhunt.Nightmare;
using Clockhunt.Phase;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Environment;
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
    
    // Music
    
    public readonly EnvironmentPlayer<ClockhuntContext, ClockhuntMusicContext> EnvironmentPlayer = new(new EnvironmentState<ClockhuntMusicContext>[]
    {
        new ChaseEnvironmentState(),
        new HuntStartPhaseEnvironmentState(),
        new HuntMiddlePhaseEnvironmentState(),
        new HuntEndPhaseEnvironmentState(),
        new HidePhaseEnvironmentState()
    }, ClockhuntMusicContext.GetContext);
    
    // Audio
    public TimedTagPlayer<ClockMarker> ClockAudioPlayer { get; } = new(new RandomObjectAudioPlayer("ClockSound",
        new SyncedAudioContainer(new AudioFileLoader("Pings")), 10, (NetworkEntity entity, MarrowEntity marrowEntity,
            DummySerializable _, ref AudioSource source) =>
        {
            source.volume = 2f;
            source.maxDistance = 120;
            source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, AnimationCurve.Linear(0, 1, 1, 0));
            source.spatialBlend = 0.85f;
            return true;
        }), 30, 60);

    public RandomObjectAudioPlayer AlarmAudioPlayer { get; } = new ("AlarmSound",
        new SyncedAudioContainer(new AudioFileLoader("Alarm")), 10, (NetworkEntity entity, MarrowEntity marrowEntity,
            DummySerializable _, ref AudioSource source) =>
        {
            source.volume = 3f;
            source.maxDistance = 300;
            source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, AnimationCurve.Linear(0, 1, 1, 0));
            source.spatialBlend = 0.65f;
            return true;
        });

    public PositionalAudioPlayer<DummySerializable> EscapeAudioPlayer = new("EscapeSound",
        new SyncedAudioContainer(new AudioFileLoader("Escape")),
        (DummySerializable _, ref AudioSource source) =>
        {
            source.volume = 2f;
            source.maxDistance = 180;
            source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, AnimationCurve.Linear(0, 1, 1, 0));
            source.spatialBlend = 0.65f;
            source.loop = true;
            return true;
        });

    protected override void OnUpdate(float delta)
    {
        NightmareManager.Update(delta);
        
        PhaseManager.Update(delta);
        EnvironmentPlayer.Update();
        ClockAudioPlayer.Update(delta);
    }
}