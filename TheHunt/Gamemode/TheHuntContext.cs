using LabFusion.Data;
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
using TheHunt.Nightmare.Ability.Active;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheHunt.Gamemode;

public class TheHuntContext : GameModeContext<TheHuntContext>
{
    public static readonly AudioBin HideAudioBin = AudioRegistry.CreateBin("MashTags.Music.TheHunt.HideMusic",
        "Mash.TheHuntAssets.MonoDisc.3000Cycles",
        "Mash.TheHuntAssets.MonoDisc.FalkesTheme",
        "Mash.TheHuntAssets.MonoDisc.TrainRide",
        "Mash.TheHuntAssets.MonoDisc.Home"
    );

    public static readonly AudioBin HuntAudioBin = AudioRegistry.CreateBin("MashTags.Music.TheHunt.HuntMusic",
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

    public static readonly AudioBin TensionAudioBin =
        AudioRegistry.CreateBin("MashTags.Music.TheHunt.TensionMusic", "Mash.TheHuntAssets.MonoDisc.Silence");

    public readonly EnvironmentManager<TheHuntContext, EnvironmentContext> EnvironmentPlayer =
        new(EnvironmentContext.GetContext);

    // SFX
    
    private static readonly AudioBin StingerAudioBin = AudioRegistry.CreateBin("MashTags.SFX.TheHunt.Stinger", "Mash.TheHuntAssets.MonoDisc.Stinger1");
    
    public readonly AnnouncementAudioPlayer StingerAudioPlayer = new("Stinger",
        new DesyncedAudioContainer(new LoadOnDemandContainer(new AudioBinLoader(StingerAudioBin))),
        new AudioModifierFactory().AddModifier<AudioSettingsModifier>(modifier =>
            modifier.SetSpatialBlend(0f)));
    
    private static readonly AudioBin RandomAmbienceAudioBin = AudioRegistry.CreateBin("MashTags.SFX.TheHunt.RandomAmbience");
    
    public readonly TimedPositionalPlayer RandomAmbienceAudioPlayer = new (new PositionalAudioPlayer("RandomAmbience",
        new SyncedAudioContainer(new AudioBinLoader(RandomAmbienceAudioBin)),
        new AudioModifierFactory().AddModifier<AudioSettingsModifier>(modifier =>
            modifier.SetMaxDistance(400f).SetCustomRolloff(AnimationCurve.Linear(0f, 1f, 1f, 0.9f)))), () =>
    {
        var center = RigData.Refs.Head.position;
        var randomPos = center + Random.insideUnitSphere * 200f;
        return center + randomPos;
    }, 120f, 360f);
    
    // Abilities
    private static readonly AudioBin RoarAudioBin = AudioRegistry.CreateBin("MashTags.SFX.TheHunt.Roar", "Mash.TheHuntAssets.MonoDisc.Roar");

    public static readonly ClientCallableAudioPlayer<Vector3, RoarRequest> RoarAudioPlayer = new(new PositionalAudioPlayer("EntityRoar",
        new DesyncedAudioContainer(new LoadOnDemandContainer(new AudioBinLoader(RoarAudioBin))),
        new AudioModifierFactory().AddModifier<AudioSettingsModifier>(modifier =>
            modifier.SetMaxDistance(160f).SetCustomRolloff(AnimationCurve.Linear(0f, 1f, 1f, 0.2f)))));

}