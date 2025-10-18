using BoneStrike.Config;
using BoneStrike.Phase;
using BoneStrike.Teams;
using MashGamemodeLibrary.Context;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Team;

namespace BoneStrike;

public class BoneStrike : GamemodeWithContext<BoneStrikeContext, BoneStrikeConfig>
{
    public override string Title => "Bone Strike";
    public override string Author => "Mash";

    public override void OnGamemodeRegistered()
    {
        EntityTagManager.RegisterAll<Mod>();
        GamePhaseManager.Registry.RegisterAll<Mod>();
        TeamManager.Registry.RegisterAll<Mod>();
    }


    protected override void OnStart()
    {
        TeamManager.Enable<TerroristTeam>();
        TeamManager.Enable<CounterTerroristTeam>();

        Executor.RunIfHost(() =>
        {
            TeamManager.AssignAllRandom();
            GamePhaseManager.Enable<PlantPhase>();
        });
    }
}