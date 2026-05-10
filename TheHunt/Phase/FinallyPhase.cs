using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.networking.Control;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.networking.Variable;
using MashGamemodeLibrary.networking.Variable.Encoder.Impl;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Helpers;
using MashGamemodeLibrary.Player.Team;
using TheHunt.Components;
using TheHunt.Teams;
using UnityEngine;

namespace TheHunt.Phase;

/// <summary>
/// Last player standing, epic chase scene ensues
/// Also triggers on the last minute of the round
/// </summary>
public class FinallyPhase : GamePhase, IHandTargetProvider, IHandTimerProvider
{
    public override string Name => "Finally";
    public override float Duration => Gamemode.TheHunt.Config.FinallyDuration;
    
    private static readonly SyncedVariable<Vector3> SyncedEscapePosition = new("FinallyPhase.EscapePosition", new Vector3Encoder(), Vector3.zero);
    private static readonly RemoteEvent EscapeEvent = new("FinallyPhase.EscapeEvent", OnEscape, CommonNetworkRoutes.AllToHost);

    private bool _isLocalEscaping;
    private bool _hasLocalEscaped;
    public float EscapeTime { get; private set; }
    
    public static Vector3 EscapePosition => SyncedEscapePosition.Value;
    public static void SetEscapePosition(Vector3 position)
    {
        Executor.RunIfHost(() =>
        {
            SyncedEscapePosition.SetAndSync(position);
        });
    }
    
    public override PhaseIdentifier GetNextPhase()
    {
        if (!HasReachedDuration())
            return PhaseIdentifier.Empty();

        WinManager.Win<HiderTeam>();
        
        return PhaseIdentifier.Empty();
    }

    protected override void OnPhaseEnter()
    {
        _hasLocalEscaped = false;
        EscapeTime = 0f;
    }

    protected override void OnUpdate()
    {
        // Escape is local
        var config = Gamemode.TheHunt.Config;
        if (!config.FinallyRequiresEscape)
            return;

        if (!RigData.HasPlayer)
            return;
        
        if (LogicTeamManager.IsLocalTeam<NightmareTeam>())
            return;
        
        var position = RigData.Refs.Head.position;
        var distanceSquared = (position - EscapePosition).sqrMagnitude;
        _isLocalEscaping = distanceSquared <= config.EscapeDistance * config.EscapeDistance;
        if (!_isLocalEscaping)
            return;
        
        EscapeTime += Time.deltaTime;
        if (_hasLocalEscaped || !(EscapeTime >= config.EscapeTime)) 
            return;
        
        _hasLocalEscaped = true;
        EscapeEvent.Call();
    }

    private static void OnEscape(byte senderId)
    {
        var playerId = PlayerIDManager.GetPlayerID(senderId);
        if (playerId == null)
            return;
        
        if (playerId.IsTeam<NightmareTeam>())
            return;
        
        // A player escaped, hiders win
        WinManager.Win<HiderTeam>();
    }
    
    public Vector3? GetHandTargetPosition()
    {
        if (!Gamemode.TheHunt.Config.FinallyRequiresEscape)
            return null;

        if (!RigData.HasPlayer)
            return null;
        
        if (LogicTeamManager.IsLocalTeam<NightmareTeam>())
            return null;
        
        return EscapePosition;
    }
    
    public float GetHandTimer()
    {
        // If we are escaping, show the time left until we escape
        if (_isLocalEscaping)
            return Gamemode.TheHunt.Config.EscapeTime - EscapeTime;
        
        return Duration - ElapsedTime;
    }
}