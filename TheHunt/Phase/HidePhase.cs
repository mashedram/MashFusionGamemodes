using LabFusion.Entities;
using MashGamemodeLibrary.Entities.Interaction.Grabbing;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Team;
using TheHunt.Teams;

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
        if (LogicTeamManager.IsLocalTeam<NightmareTeam>())
        {
            PlayerGrabManager.GrabPredicate = d =>
            {
                if (d.GrabbedNetworkEntity == null)
                    return true;

                var player = d.GrabbedNetworkEntity.GetExtender<NetworkPlayer>();
                if (player == null)
                    return true;

                return player.PlayerID.IsMe;
            };
        }
    }

    protected override void OnPhaseExit()
    {
        PlayerGrabManager.GrabPredicate = null;
    }
}