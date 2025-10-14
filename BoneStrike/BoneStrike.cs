using BoneStrike.Phase;
using BoneStrike.Teams;
using MashGamemodeLibrary.Context;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Team;

namespace BoneStrike;

public class BoneStrike : GamemodeWithContext<BoneStrikeContext>
{
    public override string Title => "Bone Strike";
    public override string Author => "Mash";

    public override void OnGamemodeRegistered()
    {
        BoneStrikeContext.TeamManager.Register(this);
        BoneStrikeContext.TeamManager.AddTeam(BoneStrikeContext.Terrorists);
        BoneStrikeContext.TeamManager.AddTeam(BoneStrikeContext.CounterTerrorists);
    }


    protected override void OnStart()
    {
        TeamManager.Enable<TerroristTeam>();
        TeamManager.Enable<CounterTerroristTeam>();

        Executor.RunIfHost(() =>
        {
            TeamManager.AssignAll();
            GamePhaseManager.Enable<PlantPhase>();
        });
    }
}