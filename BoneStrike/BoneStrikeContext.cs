using BoneStrike.Audio;
using BoneStrike.Tags;
using LabFusion.SDK.Gamemodes;
using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Modifiers;
using MashGamemodeLibrary.Audio.Players.Background.Timed;
using MashGamemodeLibrary.Audio.Players.Object;
using MashGamemodeLibrary.Context;
using MashGamemodeLibrary.Environment;

namespace BoneStrike;

public class BoneStrikeContext : GameModeContext<BoneStrikeContext>
{
    public readonly TimedTagPlayer<BombMarker> BombAudioPlayer = new(new ObjectAudioPlayer("BombSound",
            new SyncedAudioContainer(new MonoDiscLoader(new [] { "Mash.BoneStrike.MonoDisc.ClockBeep" })), 1,
            new AudioModifierFactory().AddModifier<AudioSettingsModifier>(settings =>
                settings.SetVolume(1f).SetMaxDistance(200f))),
        10);
    
    public readonly EnvironmentManager<BoneStrikeContext, EnvironmentContext> EnvironmentPlayer =
        new(EnvironmentContext.GetContext);
}