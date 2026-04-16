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
using MashGamemodeLibrary.Player.Team;

namespace BoneStrike;

public class BoneStrikeContext : GameModeContext<BoneStrikeContext>
{
    private static readonly AudioBin ClockAudioBin = AudioRegistry.CreateBin("MashTags.SFX.Bonestrike.BombBeep", "Mash.BoneStrike.MonoDisc.ClockBeep");

    private static readonly AudioBin PlantPhaseStartAudioBin =
        AudioRegistry.CreateBin("MashTags.SFX.Bonestrike.PlantPhaseStart", "Mash.BoneStrike.MonoDisc.PlantPhaseStart");

    private static readonly AudioBin DefusePhaseStartAudioBin =
        AudioRegistry.CreateBin("MashTags.SFX.Bonestrike.DefusePhaseStart", "Mash.BoneStrike.MonoDisc.DefusePhaseStart");

    private static readonly AudioBin TerroristsWinAudioBin =
        AudioRegistry.CreateBin("MashTags.SFX.Bonestrike.TerroristsWin", "Mash.BoneStrike.MonoDisc.TerroristsWin");

    private static readonly AudioBin CounterTerroristsWinAudioBin =
        AudioRegistry.CreateBin("MashTags.SFX.Bonestrike.CounterTerroristsWin", "Mash.BoneStrike.MonoDisc.CounterTerroristsWin");

    public readonly TimedComponentPlayer<BombMarker> BombAudioPlayer = new(new ObjectAudioPlayer("BombSound",
            new DesyncedAudioContainer(new LoadOnDemandContainer(new AudioBinLoader(ClockAudioBin))), 1,
            new AudioModifierFactory().AddModifier<AudioSettingsModifier>(settings =>
                settings.SetVolume(1f).SetMaxDistance(200f))),
        5, 6);

    public readonly AnnouncementAudioPlayer PlantPhaseStartAudioPlayer = new("PlantPhaseStart",
        new DesyncedAudioContainer(new LoadOnDemandContainer(new AudioBinLoader(PlantPhaseStartAudioBin))));

    public readonly AnnouncementAudioPlayer DefusePhaseStartAudioPlayer = new("DefusePhaseStart",
        new DesyncedAudioContainer(new LoadOnDemandContainer(new AudioBinLoader(DefusePhaseStartAudioBin))));

    public readonly AnnouncementAudioPlayer TerroristsWinAudioPlayer =
        new("TerroristsWin", new DesyncedAudioContainer(new LoadOnDemandContainer(new AudioBinLoader(TerroristsWinAudioBin))));

    public readonly AnnouncementAudioPlayer CounterTerroristsWinAudioPlayer = new("CounterTerroristsWin",
        new DesyncedAudioContainer(new LoadOnDemandContainer(new AudioBinLoader(CounterTerroristsWinAudioBin))));

    public readonly EnvironmentManager<BoneStrikeContext, EnvironmentContext> EnvironmentPlayer =
        new(EnvironmentContext.GetContext);

    public readonly PersistentTeams PersistentTeams = new();
}