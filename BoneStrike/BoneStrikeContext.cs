using BoneStrike.Audio;
using BoneStrike.Tags;
using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Modifiers;
using MashGamemodeLibrary.Audio.Players.Background.Timed;
using MashGamemodeLibrary.Audio.Players.Object;
using MashGamemodeLibrary.Audio.Registry;
using MashGamemodeLibrary.Context;
using MashGamemodeLibrary.Environment;

namespace BoneStrike;

public class BoneStrikeContext : GameModeContext<BoneStrikeContext>
{
    private static readonly AudioBin ClockAudioBin = AudioRegistry.CreateBin("Bonestrike.ClockPing", "Mash.BoneStrike.MonoDisc.ClockBeep");
    
    public readonly TimedComponentPlayer<BombMarker> BombAudioPlayer = new(new ObjectAudioPlayer("BombSound",
            new DesyncedAudioContainer(new LoadOnDemandContainer(new AudioBinLoader(ClockAudioBin))), 1,
            new AudioModifierFactory().AddModifier<AudioSettingsModifier>(settings =>
                settings.SetVolume(1f).SetMaxDistance(200f))),
        10);

    public readonly EnvironmentManager<BoneStrikeContext, EnvironmentContext> EnvironmentPlayer =
        new(EnvironmentContext.GetContext);
}