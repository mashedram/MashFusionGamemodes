using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Extensions;
using MashGamemodeLibrary.Entities.Interaction.Grabbing;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Data;
using MashGamemodeLibrary.Player.Data.Rules.Rules;
using MashGamemodeLibrary.Player.Team;
using TheHunt.Components;
using TheHunt.Teams;

namespace TheHunt.Phase;

public class PlantPhase : GamePhase, IHandTimerProvider
{
    public override string Name => "Plant";
    public override float Duration => Gamemode.TheHunt.Config.PlantDuration;

    public override PhaseIdentifier GetNextPhase()
    {
        if (!HasReachedDuration())
            return PhaseIdentifier.Empty();

        return PhaseIdentifier.Of<HidePhase>();
    }

    protected override void OnPhaseEnter()
    {
        if (LogicTeamManager.IsLocalTeam<NightmareTeam>())
        {
            PlayerGrabManager.GrabPredicate = d =>
            {
                var player = d.GrabbedNetworkEntity?.GetExtender<NetworkPlayer>();
                if (player == null)
                    return true;

                return player.PlayerID.IsMe;
            };
        }

        Executor.RunIfHost(() =>
        {
            PlayerDataManager.ModifyAll<HideEnemyNametagsRule>(rule => rule.IsEnabled = true);
        });
    }

    protected override void OnPhaseExit()
    {
        PlayerGrabManager.GrabPredicate = null;
        
        // Force nightmares to drop the clock
        DropObjectives();
    }
    
    public float GetHandTimer()
    {
        return Duration - ElapsedTime;
    }
    
    private static void DropObjectives()
    {
        if (!LogicTeamManager.IsLocalTeam<NightmareTeam>())
            return;

        if (!RigData.HasPlayer)
            return;
        
        foreach (var hand in RigData.Refs.GetHandsHoldingTag<ObjectiveItemComponent>())
        {
            hand.TryDetach();
        }
    }
}