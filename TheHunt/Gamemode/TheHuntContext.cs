using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Modifiers;
using MashGamemodeLibrary.Audio.Players.Callers;
using MashGamemodeLibrary.Audio.Players.Object;
using MashGamemodeLibrary.Audio.Registry;
using MashGamemodeLibrary.Context;
using MashGamemodeLibrary.Environment;
using TheHunt.Audio;
using TheHunt.Nightmare.Ability.Active;
using UnityEngine;

namespace TheHunt.Gamemode;

public class TheHuntContext : GameModeContext<TheHuntContext>
{
    public static readonly AudioBin FinallyAudioBin = AudioRegistry.CreateBin("MashTags.Music.TheHunt.FinallyMusic", "Mash.TheHuntAssets.MonoDisc.TheFinallySong");
    
    public readonly EnvironmentManager<TheHuntContext, EnvironmentContext> EnvironmentPlayer =
        new(EnvironmentContext.GetContext);
    
    // Abilities
    private static readonly AudioBin RoarAudioBin = AudioRegistry.CreateBin("MashTags.SFX.TheHunt.Roar", "Mash.TheHuntAssets.MonoDisc.Roar");
    public static readonly ClientCallableAudioPlayer<Vector3, RoarRequest> RoarAudioPlayer = new(new PositionalAudioPlayer("EntityRoar",
        new DesyncedAudioContainer(new LoadOnDemandContainer(new AudioBinLoader(RoarAudioBin))),
        new AudioModifierFactory().AddModifier<AudioSettingsModifier>(modifier =>
            modifier.SetMaxDistance(160f).SetCustomRolloff(AnimationCurve.Linear(0f, 1f, 1f, 0.2f)))));

}