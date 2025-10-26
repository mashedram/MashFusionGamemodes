using Clockhunt.Audio;
using Clockhunt.Audio.Hunt;
using Clockhunt.Config;
using Clockhunt.Entities;
using Clockhunt.Game;
using Clockhunt.Game.Teams;
using Clockhunt.Nightmare;
using Clockhunt.Phase;
using Clockhunt.Vision;
using LabFusion.Menu.Data;
using LabFusion.Player;
using LabFusion.SDK.Gamemodes;
using MashGamemodeLibrary.Context;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Entities.Tagging.Player.Common;
using MashGamemodeLibrary.Environment;
using MashGamemodeLibrary.Environment.Effector.Weather;
using MashGamemodeLibrary.Environment.State;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Controller;
using MashGamemodeLibrary.Player.Stats;
using MashGamemodeLibrary.Spectating;
using MashGamemodeLibrary.Vision;
using Avatar = Il2CppSLZ.VRMK.Avatar;
using TeamManager = MashGamemodeLibrary.Player.Team.TeamManager;

namespace Clockhunt;

internal class Clockhunt : GamemodeWithContext<ClockhuntContext, ClockhuntConfig>
{
    private const string CalibrationAvatar = "c3534c5a-94b2-40a4-912a-24a8506f6c79";

    public override string Title => "Clockhunt";
    public override string Author => "Mash";

    public override bool AutoHolsterOnDeath => true;
    public override bool DisableDevTools => Config.DevToolsDisabled;
    public override bool DisableSpawnGun => Config.DevToolsDisabled;
    public override bool DisableManualUnragdoll => Config.DevToolsDisabled;

    public override void OnGamemodeRegistered()
    {
        base.OnGamemodeRegistered();

        EntityTagManager.RegisterAll<Mod>();
        NightmareManager.RegisterAll<Mod>();
        GamePhaseManager.Registry.RegisterAll<Mod>();
        TeamManager.Registry.RegisterAll<Mod>();
    }

    private static void ListenToAvatarChange(Avatar avatar, string barcode)
    {
        if (barcode == CalibrationAvatar)
            return;

        LocalAvatar.AvatarOverride = barcode;
        LocalAvatar.OnAvatarChanged -= ListenToAvatarChange;
    }

    protected override void OnStart()
    {
        TeamManager.Enable<NightmareTeam>();
        TeamManager.Enable<SurvivorTeam>();
        
        Executor.RunIfHost(() =>
        {
            PlayerControllerManager.Enable(() => new LimitedRespawnTag(Config.MaxRespawns, player =>
            {
                if (GamePhaseManager.IsPhase<HidePhase>())
                    return false;

                if (TeamManager.IsTeam<NightmareTeam>(player.PlayerID))
                    return false;

                if (Config.DebugSkipSpectate)
                    return false;

                return true;
            }));
            
            NightmareManager.ClearNightmares();
            SpectatorManager.Clear();
            
            GamePhaseManager.Enable<HidePhase>();
        });

        ClockhuntMusicContext.Reset();
        Context.EnvironmentPlayer.StartPlaying(new EnvironmentProfile<ClockhuntMusicContext>("night",
            new EnvironmentState<ClockhuntMusicContext>[]
            {
                new ChaseEnvironmentState(),
                new HuntEndEnvironmentState(),
                new HuntMiddlePhaseEnvironmentState(),
                new HuntStartPhaseEnvironmentState(),
                new HidePhaseEnvironmentState()
            }, LocalWeatherManager.ClearLocalWeather));

        MarkerManager.ClearMarker();
        
        PlayerHider.HideAllSpecials();

        var currentAvatar = LocalAvatar.AvatarBarcode;
        if (currentAvatar != CalibrationAvatar)
            LocalAvatar.AvatarOverride = currentAvatar;
        else
            LocalAvatar.OnAvatarChanged += ListenToAvatarChange;
    }

    public override void OnLateJoin(PlayerID playerID)
    {
        TeamManager.Assign<SurvivorTeam>(playerID);
    }

    protected override void OnUpdate(float delta)
    {
        NightmareManager.Update(delta);
    }

    public override void OnGamemodeStopped()
    {
        base.OnGamemodeStopped();
        
        MarkerManager.ClearMarker();
        VisionManager.DisableNightVision();

        LocalAvatar.AvatarOverride = null;

        Executor.RunIfHost(() =>
        {
            NightmareManager.ClearNightmares();
            ClockManager.ClearClocks();
        });
    }
}