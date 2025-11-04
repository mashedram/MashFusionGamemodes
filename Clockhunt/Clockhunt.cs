using Clockhunt.Audio;
using Clockhunt.Audio.Hunt;
using Clockhunt.Config;
using Clockhunt.Entities;
using Clockhunt.Game;
using Clockhunt.Game.Player;
using Clockhunt.Game.Teams;
using Clockhunt.Nightmare;
using Clockhunt.Phase;
using Clockhunt.Vision;
using LabFusion.Entities;
using LabFusion.Menu.Data;
using LabFusion.Player;
using LabFusion.SDK.Gamemodes;
using MashGamemodeLibrary.Context;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Entities.Tagging.Player.Common;
using MashGamemodeLibrary.Environment;
using MashGamemodeLibrary.Environment.Effector.Weather;
using MashGamemodeLibrary.Environment.State;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Controller;
using MashGamemodeLibrary.Player.Spectating;
using MashGamemodeLibrary.Player.Stats;
using MashGamemodeLibrary.Player.Team;
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
        
        LimitedRespawnTag.RegisterSpectatePredicate<Clockhunt>(player =>
        {
            if (Config.DebugSkipSpectate)
                return false;
                
            if (AnyAliveSurvivors(player))
                return true;

            WinManager.Win<NightmareTeam>();
            return false;
        });
    }

    private static void ListenToAvatarChange(Avatar avatar, string barcode)
    {
        if (barcode == CalibrationAvatar)
            return;

        LocalAvatar.AvatarOverride = barcode;
        LocalAvatar.OnAvatarChanged -= ListenToAvatarChange;
    }

    protected override void OnRoundStart()
    {
        TeamManager.Enable<NightmareTeam>();
        TeamManager.Enable<SurvivorTeam>();
        
        Executor.RunIfHost(() =>
        {
            NightmareManager.ClearNightmares();
            SpectatorManager.Clear();
            
            GamePhaseManager.Enable<HidePhase>();
        });
        
        PlayerGunManager.DamageMultiplier = Config.DamageMultiplier;
        PlayerGunManager.NormalizePlayerDamage = Config.BalanceDamage;

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
        playerID.SetSpectating(true);
    }

    protected override void OnUpdate(float delta)
    {
        NightmareManager.Update(delta);
    }

    protected override void OnCleanup()
    {
        MarkerManager.ClearMarker();
        VisionManager.DisableNightVision();

        LocalAvatar.AvatarOverride = null;

        Executor.RunIfHost(() =>
        {
            NightmareManager.ClearNightmares();
            ClockManager.ClearClocks();
        });
    }

    private static bool AnyAliveSurvivors(NetworkPlayer? skip = null)
    {
        return NetworkPlayer.Players.Any(player => !player.PlayerID.IsSpectating() && player.PlayerID.IsTeam<SurvivorTeam>() && !player.PlayerID.Equals(skip?.PlayerID));
    }
}