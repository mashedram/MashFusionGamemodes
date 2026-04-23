using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Modifiers;
using MashGamemodeLibrary.Audio.Players.Background.Timed;
using MashGamemodeLibrary.Audio.Players.Callers;
using MashGamemodeLibrary.Audio.Players.Object;
using MashGamemodeLibrary.Audio.Registry;
using MashGamemodeLibrary.Context;
using MashGamemodeLibrary.Environment;
using TheHunt.Audio;
using TheHunt.Nightmare;
using TheHunt.Nightmare.Ability.Active;
using UnityEngine;

namespace TheHunt.Gamemode;

public class TheHuntContext : GameModeContext<TheHuntContext>
{
    public static readonly AudioBin HideAudioBin = AudioRegistry.CreateBin("MashTags.Music.TheHunt.HideMusic",
        "Mash.TheHuntAssets.MonoDisc.3000Cycles",
        "Mash.TheHuntAssets.MonoDisc.FalkesTheme",
        "Mash.TheHuntAssets.MonoDisc.TrainRide",
        "Mash.TheHuntAssets.MonoDisc.Home"
    );

    public static readonly AudioBin HuntHiderAudioBin = AudioRegistry.CreateBin("MashTags.Music.TheHunt.HuntMusic",
        "Mash.TheHuntAssets.MonoDisc.TurnedAround",
        "Mash.TheHuntAssets.MonoDisc.Misremembered",
        "Mash.TheHuntAssets.MonoDisc.NearDarkbythePond"
    );
    
    public static readonly AudioBin HuntNightmareAudioBin = AudioRegistry.CreateBin("MashTags.Music.TheHunt.HuntingMusic",
        "Mash.TheHuntAssets.MonoDisc.TurnedAround",
        "Mash.TheHuntAssets.MonoDisc.Misremembered",
        "Mash.TheHuntAssets.MonoDisc.NearDarkbythePond"
    );

    public static readonly AudioBin ChaseAudioBin = AudioRegistry.CreateBin("MashTags.Music.TheHunt.ChaseMusic",
        "Mash.SignalisMonodiscs.MonoDisc.RiotControl",
        "Mash.SignalisMonodiscs.MonoDisc.Kolibri",
        "Mash.SignalisMonodiscs.MonoDisc.IntensiveCare",
        "Mash.SignalisMonodiscs.MonoDisc.BecameWholeAgain",
        "Mash.SignalisMonodiscs.MonoDisc.Blockwart"
    );

    public static readonly AudioBin FinallyAudioBin =
        AudioRegistry.CreateBin("MashTags.Music.TheHunt.FinallyMusic", "Mash.TheHuntAssets.MonoDisc.TheFinallySong");
    
    public readonly EnvironmentManager<TheHuntContext, EnvironmentContext> EnvironmentPlayer =
        new(EnvironmentContext.GetContext);
    
    private static readonly AudioBin RandomAmbienceAudioBin = AudioRegistry.CreateBin("MashTags.SFX.TheHunt.RandomAmbience");
    
    public readonly TimedComponentPlayer<NightmareComponent> RandomAmbienceAudioPlayer = new(new ObjectAudioPlayer("NightmareSound",
            new DesyncedAudioContainer(new LoadOnDemandContainer(new AudioBinLoader(RandomAmbienceAudioBin))), 1,
            new AudioModifierFactory().AddModifier<AudioSettingsModifier>(settings =>
                    settings.SetVolume(1f).SetMaxDistance(4700f).SetCustomRolloff(AnimationCurve.Linear(0f, 1f, 0f, 0.65f)))
                .AddModifier<MuffleAudioModifier>()),
        45, 150);
    
    // Bell
    
    private static readonly AudioBin BellAudioBin = AudioRegistry.CreateBin("MashTags.SFX.TheHunt.Bell", "Mash.TheHuntAssets.MonoDisc.BellToll");
    
    public readonly PositionalAudioPlayer BellAudioPlayer = new("Bell",
        new DesyncedAudioContainer(new LoadOnDemandContainer(new AudioBinLoader(BellAudioBin))),
        new AudioModifierFactory().AddModifier<AudioSettingsModifier>(modifier => 
            modifier.SetMaxDistance(1000f).SetCustomRolloff(AnimationCurve.Constant(0f, 1f, 1f)))
    );
    
    // Abilities
    private static readonly AudioBin RoarAudioBin = AudioRegistry.CreateBin("MashTags.SFX.TheHunt.Roar", "Mash.TheHuntAssets.MonoDisc.Roar");

    public static readonly ClientCallableAudioPlayer<Vector3, RoarRequest> RoarAudioPlayer = new(new PositionalAudioPlayer("EntityRoar",
        new DesyncedAudioContainer(new LoadOnDemandContainer(new AudioBinLoader(RoarAudioBin))),
        new AudioModifierFactory().AddModifier<AudioSettingsModifier>(modifier =>
            modifier.SetMaxDistance(800f).SetCustomRolloff(AnimationCurve.Linear(0f, 1f, 1f, 0.65f))).AddModifier<MuffleAudioModifier>()));

}