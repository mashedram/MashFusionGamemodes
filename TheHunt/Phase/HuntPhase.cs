using LabFusion.Data;
using LabFusion.SDK.Gamemodes;
using MashGamemodeLibrary.Entities.ECS;
using MashGamemodeLibrary.Entities.Queries;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.networking.Variable;
using MashGamemodeLibrary.networking.Variable.Encoder.Impl;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Team;
using TheHunt.Components;
using TheHunt.Modifiers;
using TheHunt.Teams;
using UnityEngine;

namespace TheHunt.Phase;

/// <summary>
/// The hunt begins
/// </summary>
public class HuntPhase : GamePhase, IHandTimerProvider
{
    private static readonly SyncedVariable<float> ExtendTime = new("HuntPhase.ExtendTime", new FloatEncoder(), 0f);
    public override string Name => "Hunt";
    public override float Duration => Gamemode.TheHunt.Config.HuntDuration + ExtendTime;
    
    public override PhaseIdentifier GetNextPhase()
    {
        var config = Gamemode.TheHunt.Config;
        
        if (!HasReachedDuration())
            return PhaseIdentifier.Empty();
        
        if (config.FinallyAlwaysPlays)
            return PhaseIdentifier.Of<FinallyPhase>();

        WinManager.Win<HiderTeam>();
        
        return PhaseIdentifier.Empty();
    }

    protected override void OnPhaseEnter()
    {
        Executor.RunIfHost(() =>
        {
            ExtendTime.Value = 0f;

            var descriptor = Nightmare.Nightmare.Nightmares.FirstOrDefault()?.Descriptor;
            if (descriptor == null)
                return;
            
            ModifierManager.Randomize(
                Gamemode.TheHunt.Config.ModifierCount, 
                descriptor.RequiredModifiers,
                descriptor.BannedModifiers
            );
        });
        
        Gamemode.TheHunt.Context.RandomAmbienceAudioPlayer.Start();
    }

    protected override void OnPhaseExit()
    {
        Gamemode.TheHunt.Context.RandomAmbienceAudioPlayer.Stop();
    }
    
    public static void Extend(float time)
    {
        Executor.RunIfHost(() =>
        {
            ExtendTime.Value += time;
        });
    }
    
    public float GetHandTimer()
    {
        return Duration - ElapsedTime;
    }
}