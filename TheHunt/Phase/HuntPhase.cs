using LabFusion.Data;
using MashGamemodeLibrary.Entities.ECS;
using MashGamemodeLibrary.Entities.Queries;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.networking.Variable;
using MashGamemodeLibrary.networking.Variable.Encoder.Impl;
using MashGamemodeLibrary.Phase;
using TheHunt.Components;
using TheHunt.Teams;
using UnityEngine;

namespace TheHunt.Phase;

/// <summary>
/// The hunt begins
/// </summary>
public class HuntPhase : GamePhase, IHandTargetProvider, IHandTimerProvider
{
    private static readonly SyncedVariable<float> ExtendTime = new("HuntPhase.ExtendTime", new FloatEncoder(), 0f);
    public override string Name => "Hunt";
    public override float Duration => Gamemode.TheHunt.Config.HuntDuration + ExtendTime;
    
    public override PhaseIdentifier GetNextPhase()
    {
        var config = Gamemode.TheHunt.Config;
        
        // If we use plant mode and there are no objective items
        if (config.PlantMode && !ObjectiveItemComponent.Query.Any())
        {
            // Initialize escape sequence
            if (config.FinallyRequiresEscape)
                return PhaseIdentifier.Of<FinallyPhase>();
            
            // Objective secured, hiders win
            WinManager.Win<HiderTeam>();
            return PhaseIdentifier.Empty();
        }
        
        if (!HasReachedDuration())
            return PhaseIdentifier.Empty();

        if (config.PlantMode && ObjectiveItemComponent.Query.Any())
        {
            // Time's up but the objectives hasn't been secured, nightmares win
            WinManager.Win<NightmareTeam>();
            return PhaseIdentifier.Empty();
        }
        
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
    
    public Vector3? GetHandTargetPosition()
    {
        if (!RigData.HasPlayer)
            return null;

        if (!Gamemode.TheHunt.Config.PlantMode)
            return null;
        
        var playerPosition = RigData.Refs.RightHand.transform.position;
        
        var objectiveItem = ObjectiveItemComponent.Query.MinBy(o => (o.Position - playerPosition).sqrMagnitude);
        return objectiveItem?.Position;
    }
    
    public float GetHandTimer()
    {
        return Duration - ElapsedTime;
    }
}