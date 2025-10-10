using Clockhunt.Audio;
using Clockhunt.Audio.Hunt;
using Clockhunt.Entities;
using Clockhunt.Entities.Tags;
using Clockhunt.Game;
using Clockhunt.Nightmare;
using Clockhunt.Phase;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Modifiers;
using MashGamemodeLibrary.Audio.Players.Background;
using MashGamemodeLibrary.Audio.Players.Background.Timed;
using MashGamemodeLibrary.Audio.Players.Object;
using MashGamemodeLibrary.Context;
using MashGamemodeLibrary.Environment;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Spectating;
using MashGamemodeLibrary.Util;
using UnityEngine;

namespace Clockhunt;

public class ClockhuntContext : GameContext
{
    // Music

    public readonly EnvironmentManager<ClockhuntContext, ClockhuntMusicContext> EnvironmentPlayer =
        new(ClockhuntMusicContext.GetContext);

    // Audio
    public readonly TimedTagPlayer<ClockMarker> ClockAudioPlayer = new(new RandomObjectAudioPlayer("ClockSound",
            new SyncedAudioContainer(new AudioFileLoader("Pings")), 10,
            new AudioModifierFactory().AddModifier<AudioSettingsModifier>(settings =>
                    settings.SetVolume(1f).SetMaxDistance(120f))
                .AddModifier<MuffleAudioModifier>()),
        10, 30);

    public readonly PositionalAudioPlayer EscapeAudioPlayer = new("EscapeSound",
        new SyncedAudioContainer(new AudioFileLoader("Escape")),
        new AudioModifierFactory().AddModifier<AudioSettingsModifier>(settings =>
            settings.SetVolume(1f).SetMaxDistance(180f)
                .SetCustomRolloff(AnimationCurve.Linear(0, 1, 1, 0)).SetSpatialBlend(0.65f).SetLoop(true)));

    public IEnumerable<NetworkPlayer> GetActivePlayers()
    {
        return NetworkPlayer.Players.Where(p => !p.PlayerID.IsSpectating());
    }
    
    protected override void OnUpdate(float delta)
    {
        NightmareManager.Update(delta);

        ClockAudioPlayer.Update(delta);
        EscapeAudioPlayer.Update(delta);

        GamePhaseManager.Update(delta);
        EnvironmentPlayer.Update(delta);
        ClockAudioPlayer.Update(delta);
    }
}