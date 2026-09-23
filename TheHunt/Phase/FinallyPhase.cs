using Il2CppSLZ.Marrow.Pool;
using LabFusion.Data;
using LabFusion.Player;
using MashGamemodeLibrary.Entities;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.networking.Variable;
using MashGamemodeLibrary.networking.Variable.Encoder.Impl;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Team;
using TheHunt.Components;
using TheHunt.Teams;
using UnityEngine;

namespace TheHunt.Phase;

/// <summary>
/// Last player standing, epic chase scene ensues
/// Also triggers on the last minute of the round
/// </summary>
public class FinallyPhase : GamePhase, IHandTimerProvider
{
    public override string Name => "Finally";
    public override float Duration => Gamemode.TheHunt.Config.FinallyDuration;
    
    
    public override PhaseIdentifier GetNextPhase()
    {
        if (!HasReachedDuration())
            return PhaseIdentifier.Empty();

        WinManager.Win<HiderTeam>();
        
        return PhaseIdentifier.Empty();
    }
    
    public float GetHandTimer()
    {
        return Duration - ElapsedTime;
    }
}