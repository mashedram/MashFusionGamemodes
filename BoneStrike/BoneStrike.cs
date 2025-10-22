using BoneStrike.Audio;
using BoneStrike.Config;
using BoneStrike.Phase;
using BoneStrike.Player;
using BoneStrike.Tags;
using BoneStrike.Teams;
using LabFusion.Player;
using LabFusion.RPC;
using LabFusion.SDK.Gamemodes;
using MashGamemodeLibrary.Context;
using MashGamemodeLibrary.Entities;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Environment;
using MashGamemodeLibrary.Environment.Effector.Weather;
using MashGamemodeLibrary.Environment.State;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Loadout;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Controller;
using TeamManager = MashGamemodeLibrary.Player.Team.TeamManager;

namespace BoneStrike;

public class BoneStrike : GamemodeWithContext<BoneStrikeContext, BoneStrikeConfig>
{
    public override string Title => "Bone Strike";
    public override string Author => "Mash";

    protected override void OnRegistered()
    {
        EntityTagManager.RegisterAll<Mod>();
        GamePhaseManager.Registry.RegisterAll<Mod>();
        TeamManager.Registry.RegisterAll<Mod>();
    }


    protected override void OnStart()
    {
        TeamManager.Enable<TerroristTeam>();
        TeamManager.Enable<CounterTerroristTeam>();
        TeamManager.AssignAllRandom();
        PlayerControllerManager.Enable<BoneStrikePlayerController>();
        GamePhaseManager.Enable<PlantPhase>();
        
        Context.EnvironmentPlayer.StartPlaying(new EnvironmentProfile<EnvironmentContext>("all",
            new EnvironmentState<EnvironmentContext>[]
            {
                new PlantState(),
                new DefuseState()
            }, LocalWeatherManager.ClearLocalWeather));
    }

    protected override void OnCleanup()
    {
        LocalVision.Blind = false;
        LocalControls.LockedMovement = false;

        GameAssetSpawner.DespawnAll<BombMarker>();
    }

    public override bool CanAttack(PlayerID player)
    {
        if (!IsStarted)
            return true;

        if (GamePhaseManager.IsPhase<PlantPhase>())
            return false;

        return !TeamManager.IsTeamMember(player);
    }
}