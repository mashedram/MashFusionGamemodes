using LabFusion.Entities;
using LabFusion.Player;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Helpers;
using TheHunt.Components;

namespace TheHunt.Phase;

/// <summary>
/// A phase at the start of the round where the players need to find a hiding spot
/// </summary>
public class HidePhase : GamePhase
{
    public override string Name => "Hide";
    public override float Duration => Gamemode.TheHunt.Config.HideDuration;
    
    public override PhaseIdentifier GetNextPhase()
    {
        if (!HasReachedDuration())
            return PhaseIdentifier.Empty();

        return PhaseIdentifier.Of<HuntPhase>();
    }

    protected override void OnPhaseEnter()
    {
        foreach (var playerID in NetworkPlayer.Players)
        {
            playerID.AddComponents(new HiderFinallyMarker());
        }
    }
}